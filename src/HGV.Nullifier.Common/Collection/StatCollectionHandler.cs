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

                    await Task.Delay(TimeSpan.FromSeconds(3));
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

                    var valid = 1;
                    var date = DateTimeOffset.FromUnixTimeSeconds(match.start_time).UtcDateTime;
                    var duration = DateTimeOffset.FromUnixTimeSeconds(match.duration).TimeOfDay.TotalMinutes;
                    var day_of_week = (int)date.DayOfWeek;

                    var time_limit = 10;
                    if (duration < time_limit)
                    {
                        valid = 0;
                        this.logger.Warning($"Match {match.match_id} is invalid as it is less then {time_limit} mins.");
                    }
                    else
                    {
                        ProcessPlayer(heroes, abilities, ultimates, talents, match, context, ref valid);
                    }

                    var match_summary = new MatchSummary()
                    {
                        id = match.match_id,
                        match_number = match.match_seq_num,
                        league_id = match.leagueid,
                        duration = duration,
                        day_of_week = day_of_week,
                        date = date,
                        victory_radiant = match.radiant_win ? 1 : 0,
                        victory_dire = match.radiant_win ? 0 : 1,
                        valid = valid,
                    };
                    context.Matches.Add(match_summary);

                    await context.SaveChangesAsync();

                    LogResults(then, match, match_summary);
                }
                catch (Exception ex)
                {
                    this.logger.Error(ex);
                }
            }
        }

        private void LogResults(DateTime then, Match match, MatchSummary match_summary)
        {
            var now = DateTime.Now;
            var processing_delta = now - then;
            var match_delta = now - match_summary.date.ToLocalTime().AddMinutes(match_summary.duration);
            var end_time = Math.Round(match_delta.TotalMinutes, 2);
            var processing_time = Math.Round(processing_delta.TotalSeconds, 2);

            this.logger.Info($"Match {match.match_id} took {processing_time} secs to process which ended {end_time} mins ago");
        }

        private void ProcessPlayer(Dictionary<int, List<int>> heroes, List<int> abilities, List<int> ultimates, List<int> talents, Match match, DataContext context, ref int valid)
        {
            foreach (var player in match.players)
            {
                var team = player.player_slot < 6 ? 0 : 1;
                var order = this.ConvertPlayerSlotToDraftOrder(player.player_slot);
                var result = team == 0 ? match.radiant_win : !match.radiant_win;
                var hero_id = player.hero_id;

                var heroes_abilities = new List<int>();
                heroes.TryGetValue(hero_id, out heroes_abilities);

                if (player.ability_upgrades == null)
                {
                    valid = 0;
                    this.logger.Warning($"Match {match.match_id} is invalid as player {order} has no abilities, no players and abilities will be logged.");
                    continue;
                }

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
                        team = team,
                        hero_id = hero_id,
                        draft_order = order,
                        account_id = player.account_id,
                        match_id = match.match_id,
                        league_id = match.leagueid
                    };
                    context.Skills.Add(skill_summary);
                }

                var player_summary = new PlayerSummary()
                {
                    match_result = result == true ? 1 : 0,
                    team = team,
                    hero_id = hero_id,
                    draft_order = order,
                    account_id = player.account_id,
                    match_id = match.match_id,
                    league_id = match.leagueid
                };
                context.Players.Add(player_summary);
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
