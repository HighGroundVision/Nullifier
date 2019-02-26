using HGV.Daedalus;
using HGV.Daedalus.GetMatchDetails;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using HGV.Nullifier.Data;
using HGV.Basilius;
using System.Threading;
using System.Diagnostics;
using HGV.Nullifier.Data.Models;
using HGV.Nullifier.Logger;

namespace HGV.Nullifier
{
    public class StatCollectionHandler
    {
        private ILogger logger;
        private readonly ConcurrentQueue<Match> qProcessing;
        private readonly string api_key;
        private long match_number;

        public static void Run(string apiKey, CancellationToken t, ILogger l)
        {
            var handler = new StatCollectionHandler(apiKey, l);
            handler.Initialize();

            var tasks = new Task[2] { handler.Collecting(), handler.Processing() };
            var result = Task.WaitAny(tasks, t);
            switch (result)
            {
                case 0:
                    throw new Exception("Collecting Failed");
                case 1:
                    throw new Exception("Processing Failed");
                default:
                    break;
            }
        }

        private StatCollectionHandler(string apiKey, ILogger l)
        {
            this.logger = l;
            this.qProcessing = new ConcurrentQueue<Match>();
            this.api_key = apiKey; 
        }

        public void Initialize()
        {
            var context = new DataContext();
            var client = new DotaApiClient(this.api_key);

            var count = context.Matches.Count();
            if(count > 0)
            {
                this.match_number = context.Matches.Max(_ => _.match_number) + 1;
            }
            else
            {       
                var latest = client.GetLastestMatches().Result;
                this.match_number = latest.Max(_ => _.match_seq_num);
            }
        }

        private async Task Collecting()
        {
            if (this.match_number == 0)
                throw new ApplicationException("match_number cannot be zero");

            var client = new DotaApiClient(this.api_key);
            while (true)
            {
                try
                {
                    this.match_number++;
                    var matches = await client.GetMatchesInSequence(this.match_number);
                    foreach (var match in matches)
                    {
                        // Retarget match number
                        if (match.match_seq_num > this.match_number)
                            this.match_number = match.match_seq_num;

                        // Send AD match on to processs
                        if (match.game_mode == 18)
                            this.qProcessing.Enqueue(match);
                    }

                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
                catch (Exception)
                {
                    await Task.Delay(TimeSpan.FromSeconds(60));
                }

            }
        }

        public async Task Processing()
        {
            var client = new MetaClient();
            var heroes = client.GetHeroes().Select(_ => new { Key = _.Id, Abilities = _.Abilities.Select(__ => -__.Id).ToList() }).ToDictionary(_ => _.Key, _ => _.Abilities);
            var abilities = client.GetAbilities().Select(_ => _.Id).ToList();
            var ultimates = client.GetUltimates().Select(_ => _.Id).ToList();
            var talents = client.GetTalents().Select(_ => _.Id).ToList();

            while (true)
            {
                try
                {
                    var then = DateTime.Now;

                    Match match;
                    if (!this.qProcessing.TryDequeue(out match))
                        continue;

                    var context = new DataContext();

                    var count = context.Matches.Where(_ => _.match_number == match.match_seq_num).Count();
                    if (count > 0)
                        continue;

                    var date = DateTimeOffset.FromUnixTimeSeconds(match.start_time).UtcDateTime;
                    var duration = DateTimeOffset.FromUnixTimeSeconds(match.duration).TimeOfDay.TotalMinutes;
                    var day_of_week = (int)date.DayOfWeek;

                    var match_summary = new MatchSummary()
                    {
                        id = match.match_id,
                        match_number = match.match_seq_num,
                        duration = duration,
                        day_of_week = day_of_week,
                        date = date,
                        victory_radiant = match.radiant_win ? 1 : 0,
                        victory_dire = match.radiant_win ? 0 : 1,
                    };
                    context.Matches.Add(match_summary);

                    foreach (var player in match.players)
                    {
                        var team = player.player_slot < 6 ? 0 : 1;
                        var order = this.ConvertPlayerSlotToDraftOrder(player.player_slot);
                        var result = team == 0 ? match.radiant_win : !match.radiant_win;
                        var hero_id = player.hero_id;

                        var heroes_abilities = new List<int>();
                        heroes.TryGetValue(hero_id, out heroes_abilities);

                        var player_summary = new PlayerSummary()
                        {
                            match_result = result == true ? 1 : 0,
                            team = team,
                            hero_id = hero_id,
                            player_slot = player.player_slot,
                            draft_order = order,
                            account_id = player.account_id,
                            kills = player.kills,
                            deaths = player.deaths,
                            assists = player.assists,
                            last_hits = player.last_hits,
                            denies = player.denies,
                            gold = player.gold,
                            level = player.level,
                            gold_per_min = player.gold_per_min,
                            xp_per_min = player.xp_per_min,
                            gold_spent = player.gold_spent,
                            hero_damage = player.hero_damage,
                            tower_damage = player.tower_damage,
                            hero_healing = player.hero_healing,
                            match_id = match.match_id
                        };
                        context.Players.Add(player_summary);

                        var collection = player.ability_upgrades.Select(_ => _.ability).Distinct().ToList();
                        foreach (var ability_id in collection)
                        {
                            var skill_summary = new SkillSummary()
                            {
                                ability_id = ability_id,
                                is_skill = abilities.Contains(ability_id) ? 1 : 0,
                                is_ulimate = ultimates.Contains(ability_id) ? 1 : 0,
                                is_taltent = talents.Contains(ability_id) ? 1 : 0,
                                is_self = heroes_abilities.Contains(ability_id) ? 1 : 0,
                                match_result = result == true ? 1 : 0,
                                hero_id = hero_id,
                                account_id = player.account_id,
                                draft_order = order,
                                match_id = match.match_id,
                            };
                            context.Skills.Add(skill_summary);
                        }
                    }

                    await context.SaveChangesAsync();

                    var now = DateTime.Now;
                    var processing_delta = now - then;
                    var match_delta = now - match_summary.date.ToLocalTime().AddMinutes(match_summary.duration);

                    this.logger.Info(string.Format("Processing took {1:0.00} secs on Match[{0}] which ended {2:0.00} mins ago", match.match_id, processing_delta.TotalSeconds, match_delta.TotalMinutes));
                }
                catch (Exception ex)
                {
                    this.logger.Error(ex);
                }
            }
        }

        private int ConvertPlayerSlotToDraftOrder(int slot)
        {
            switch (slot)
            {
                case 0: return 0;
                case 128: return 1;
                case 1: return 2;
                case 129: return 3;
                case 2: return 4;
                case 130: return 5;
                case 3: return 6;
                case 131: return 7;
                case 4: return 8;
                case 132: return 9;
                default:
                    throw new ApplicationException(string.Format("Unknown Slot: {0}", slot));
            }
        }
    }
}
