using HGV.Basilius;
using HGV.Daedalus;
using HGV.Nullifier.Data;
using HGV.Nullifier.Logger;
using Humanizer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HGV.Nullifier
{
    public class StatCollectionHandler
    {
        private ILogger logger;
        private readonly ConcurrentQueue<Daedalus.GetMatchDetails.Match> qProcessing;
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
            this.qProcessing = new ConcurrentQueue<Daedalus.GetMatchDetails.Match>();
            this.api_key = apiKey; 
        }

        public void Initialize()
        {
            var context = new DataContext();
            var count = context.Matches.Count();
            if(count > 0)
            {
                this.match_number = context.Matches.Max(_ => _.match_number) + 1;
            }
            else
            {
                this.match_number = 3873978702; // Match from start of 7.21d
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
            var heroesTest = client.GetHeroes();
            var heroes = client.GetHeroes().Select(_ => new { Key = _.Id, Abilities = _.Abilities.Select(__ => __.Id).ToList() }).ToDictionary(_ => _.Key, _ => _.Abilities);
            var abilities = client.GetAbilities().Select(_ => _.Id).ToList();
            var ultimates = client.GetUltimates().Select(_ => _.Id).ToList();
            var talents = client.GetTalents().Select(_ => _.Id).ToList();

            while (true)
            {
                var context = new DataContext();
                using (var transation = context.Database.BeginTransaction())
                {
                    try
                    {
                        Daedalus.GetMatchDetails.Match match;
                        if (!this.qProcessing.TryDequeue(out match))
                            continue;

                        var count = context.Matches.Where(_ => _.match_number == match.match_seq_num).Count();
                        if (count > 0)
                            continue;

                        var date = DateTimeOffset.FromUnixTimeSeconds(match.start_time).UtcDateTime;
                        var duration = DateTimeOffset.FromUnixTimeSeconds(match.duration).TimeOfDay.TotalMinutes;
                        
                        var match_summary = new Data.Models.Match()
                        {
                            match_id = match.match_id,
                            match_number = match.match_seq_num,
                            league_id = match.leagueid,
                            duration = duration,
                            day_of_week = (int)date.DayOfWeek,
                            hour_of_day = date.Hour,
                            date = date,
                            cluster = match.cluster,
                            region = client.ConvertClusterToRegion(match.cluster),
                            victory_radiant = match.radiant_win ? 1 : 0,
                            victory_dire = match.radiant_win ? 0 : 1,
                            valid = true,
                        };

                        var time_limit = 10;
                        if (duration < time_limit)
                        {
                            this.logger.Warning($"    Error: match {match.match_id} is invalid as its durration of {duration:0.00} is lower then the time limit of {time_limit}");
                            match_summary.valid = false;
                        }

                        context.Matches.Add(match_summary);
                        await context.SaveChangesAsync();

                        foreach (var player in match.players)
                        {
                            var team = player.player_slot < 6 ? 0 : 1;
                            var order = this.ConvertPlayerSlotToDraftOrder(player.player_slot);
                            var victory = team == 0 ? match.radiant_win : !match.radiant_win;
                            var hero_id = player.hero_id;

                            if (player.leaver_status > 1)
                            {
                                // https://wiki.teamfortress.com/wiki/WebAPI/GetMatchDetails
                                var status = player.leaver_status == 2 ? "DISCONNECTED" : player.leaver_status == 3 ? "ABANDONED" : player.leaver_status == 4 ? "AFK" : "Unknown";
                                this.logger.Warning($"  Warning: match {match.match_id} ({duration:0.00}) is invalid as player {order} has a leaver status of {player.leaver_status} ({status}) ");
                                match_summary.valid = false;
                            }

                            var heroes_abilities = new List<int>();
                            heroes.TryGetValue(hero_id, out heroes_abilities);

                            var match_player = new Data.Models.Player()
                            {
                                // Match
                                match_ref = match_summary.id,
                                victory = victory ? 1 : 0,
                                // Player
                                team = team,
                                draft_order = order,
                                player_slot = player.player_slot,
                                account_id = player.account_id,
                                // Hero
                                hero_id = hero_id,
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
                            };
                            context.Players.Add(match_player);
                            await context.SaveChangesAsync();

                            if (player.ability_upgrades == null)
                            {
                                throw new Exception($"    Error: match {match.match_id} is invalid as player {order} has no abilities.");
                            }

                            var query = player.ability_upgrades.Select(_ => _).ToList();
                            var collection = query.Select(_ => _.ability).Distinct().ToList();
                            foreach (var ability_id in collection)
                            {
                                var id = ConvertAbilityID(ability_id);

                                var match_skill = new Data.Models.Skill()
                                {
                                    // Match
                                    match_ref = match_summary.id,
                                    // Player
                                    player_ref = match_player.id,
                                    // Ability
                                    ability_id = id,
                                    is_skill = abilities.Contains(id) ? 1 : 0,
                                    is_ulimate = ultimates.Contains(id) ? 1 : 0,
                                    is_taltent = talents.Contains(id) ? 1 : 0,
                                    is_hero_same = heroes_abilities.Contains(id) ? 1 : 0,
                                    level = query.Where(_ => _.ability == id || _.ability == ability_id).Max(_ => _.level),
                                };
                                context.Skills.Add(match_skill);
                            }

                            await context.SaveChangesAsync();
                        }

                        var match_delta = DateTime.Now - match_summary.date.ToLocalTime().AddMinutes(match_summary.duration);
                        this.logger.Info($"Processed: match {match_summary.match_id} which ended {match_delta.Humanize(3)} mins ago.");

                        transation.Commit();
                    }
                    catch (Exception ex)
                    {
                        transation.Rollback();
                        this.logger.Error(ex);
                    }
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

        private int ConvertAbilityID(int ability_id)
        {
            if (ability_id == 6340) // Bedlem to Terrow
                return 8340;
            else if (ability_id == 5034) // Return
                return 5033;
            else if (ability_id == 5367) // Unstable Concoction Throw
                return 5366;
            else if (ability_id == 5631) // Launch Fire Spirit
                return 5625;
            else if (ability_id == 6937) // Tree Throw
                return 5108;
            else
                return ability_id;
        }
    }
}
