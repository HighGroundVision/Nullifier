using HGV.Basilius;
using HGV.Daedalus;
using HGV.Daedalus.GetMatchDetails;
using HGV.Nullifier.Common.Extensions;
using HGV.Nullifier.Data;
using HGV.Nullifier.Data.Models;
using HGV.Nullifier.Logger;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace HGV.Nullifier.Common.Collection
{
    public class StatCollectionHandler
    {
        private ILogger logger;

        private readonly ConcurrentQueue<Daedalus.GetMatchDetails.Match> processingQueue;
        private readonly HashSet<long> processedMatches;
        private readonly PersistentCircularQueue<long> history;

        private DotaApiClient apiClient = null;

        private MetaClient metaclient = new MetaClient();
        private readonly List<Ability> skills;
        private readonly List<Hero> heroes;

        private readonly GregorianCalendar calendar;

        public StatCollectionHandler(ILogger l, string apiKey)
        {
            this.logger = l;
            this.processingQueue = new ConcurrentQueue<Match>();
            this.processedMatches = new HashSet<long>();
            this.history = new PersistentCircularQueue<long>(1000, "history");
            this.apiClient = new DotaApiClient(apiKey);
            this.metaclient = new MetaClient();
            this.skills = metaclient.GetSkills();
            this.heroes = metaclient.GetHeroes();
            this.calendar = new GregorianCalendar();
        }

        public async Task Run()
        {
            this.history.Load();

            await Task.WhenAll(Collecting(), Processing());
        }

        private async Task Collecting()
        {
            var history = await apiClient.GetLastestMatches();
            var lastest = history.OrderByDescending(_ => _.match_seq_num).FirstOrDefault();
            var loc_match_seq_num = lastest.match_seq_num;

            for (; ; )
            {
                try
                {
                    var matches = await this.apiClient.GetMatchesInSequence(loc_match_seq_num);
                    foreach (var match in matches)
                    {
                        // Find largest sequence number in the set
                        if (match.match_seq_num > loc_match_seq_num)
                            loc_match_seq_num = match.match_seq_num;


                        // Skip games that are not AD
                        if (match.game_mode != 18)
                            continue;

                        // Skip games that are outside the duration limit
                        var duration = match.GetDuration().TotalMinutes;
                        if (duration < 15.0)
                        {
                            this.logger.Warning($"Match {match.match_id} Below Duration Limt");
                            continue;
                        }
                        if (duration > 75.0)
                        {
                            this.logger.Warning($"Match {match.match_id} Above Duration Limt");
                            continue;
                        }

                        // Skip games that have a Bot
                        if (match.human_players < 10)
                        {
                            this.logger.Warning($"Match {match.match_id} Dose Not Have Enough Human Players");
                            continue;
                        }

                        // Skip games that is missing abilties
                        if (match.players.Any(p => p.ability_upgrades == null))
                        {
                            this.logger.Warning($"Match {match.match_id} Dose Not Have Enough Abilities");
                            continue;
                        }

                        // Skip games that a player abandoned
                        if (match.players.Any(p => p.leaver_status > 2))
                        {
                            this.logger.Warning($"Match {match.match_id} Had An Abandon");
                            continue;
                        }

                        this.processingQueue.Enqueue(match);
                    }

                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
                catch (Exception ex)
                {
                    this.logger.Error(ex);
                    await Task.Delay(TimeSpan.FromSeconds(20));
                }
            }
        }

        public async Task Processing()
        {
            for (; ; )
            {
                try
                {
                    using (var context = new DataContext())
                    {
                        // Wait for AD matches
                        Match match;
                        if (this.processingQueue.TryDequeue(out match) == false)
                            continue;

                        // Skip games that are already processed
                        if (this.history.Add(match.match_id) == false)
                            continue;

                        LogMatch(match);

                        var day = match.GetStart().DayOfYear;

                        await this.UpdateRegionCounts(context, match, day);

                        foreach (var player in match.players)
                        {
                            var team = GetTeam(player.player_slot);
                            var victory = team == Team.Radiant ? match.radiant_win : !match.radiant_win;
                            var draftOrder = GetDraftOrder(player.player_slot);

                            await this.UpdateHeroCounts(context, match, player, day, victory);
                            await this.UpdatePlayerCounts(context, match, player, day, victory);

                            var draft = GetDraft(player);
                            foreach (var ability in draft)
                            {
                                await this.UpdateAbilityCounts(context, match, player, ability, day, victory, draftOrder);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.logger.Error(ex);
                }
            }
        }

        private void LogMatch(Match match)
        {
            var delta = (int)Math.Round((DateTime.UtcNow - match.GetEnd()).TotalMinutes);
            this.logger.Info($"Processing match {match.match_id} that ended {delta} minutes ago");
        }

        private async Task UpdateAbilityCounts(DataContext context, Match match, Player player, Ability ability, int day, bool victory, int draftOrder)
        {
            var data = context.AbilityDailyCounts.Where(_ => _.AbilityId == ability.Id && _.Day == day).FirstOrDefault();
            if (data == null)
            {
                data = new AbilityDailyCount()
                {
                    AbilityId = ability.Id,
                    AbilityName = ability.Name,
                    IsSkill = ability.IsSkill,
                    Day = day,
                };
                context.AbilityDailyCounts.Add(data);
            }

            // Priorty
            data.DraftOrder += draftOrder;

            // Strength
            if (player.kills == match.players.Max(_ => _.kills))
                data.MostKills++;

            // Win Rate
            if (victory)
                data.Wins++;
            else
                data.Losses++;

            data.WinRate = (float)data.Wins / (data.Wins + data.Losses);

            await context.SaveChangesAsync();
        }

        private async Task UpdateHeroCounts(DataContext context, Match match, Player player, int day, bool victory)
        {
            var hero = this.heroes.Where(_ => _.Id == player.hero_id).FirstOrDefault();
            if (hero == null)
            {
                this.logger.Warning($"Hero {player.hero_id} dose not exist...");
                return;
            }

            var data = context.HeroDailyCounts.Where(_ => _.HeroId == player.hero_id && _.Day == day).FirstOrDefault();
            if (data == null)
            {
                data = new HeroDailyCount()
                {
                    HeroId = hero.Id,
                    HeroName = hero.Name,
                    Day = day,
                };
                context.HeroDailyCounts.Add(data);
            }

            if (victory)
                data.Wins++;
            else
                data.Losses++;

            data.WinRate = (float)data.Wins / (data.Wins + data.Losses);

            await context.SaveChangesAsync();
        }

        private async Task UpdatePlayerCounts(DataContext context, Match match, Player player, int day, bool victory)
        {
            // Guard aginst unknown account...
            if (player.account_id == 4294967295)
                return;

            var data = context.PlayerDailyCounts.Where(_ => _.AccountId == player.account_id && _.Day == day).FirstOrDefault();
            if (data == null)
            {
                var steamId = ConvertDotaIdToSteamId(player.account_id);
                var profile = await apiClient.GetPlayerSummary(steamId);

                data = new PlayerDailyCount()
                {
                    AccountId = player.account_id,
                    SteamId = profile.steamid,
                    Persona = profile.personaname,
                    Day = day,
                };
                context.PlayerDailyCounts.Add(data);
            }

            if (victory)
                data.Wins++;
            else
                data.Losses++;

            data.WinRate = (float)data.Wins / (data.Wins + data.Losses);

            await context.SaveChangesAsync();
        }

        private async Task UpdateRegionCounts(DataContext context, Match match, int day)
        {
            var hour = match.GetStart().Hour;

            var id = metaclient.ConvertClusterToRegion(match.cluster);
            if (id == 0)
            {
                Debug.WriteLine($"Region {id}, Cluster: {match.cluster}");
            }

            var data = context.RegionDailyCounts.Where(_ => _.RegionId == id && _.Day == day && _.Hour == hour).FirstOrDefault();
            if (data == null)
            {
                data = new RegionDailyCount()
                {
                    RegionId = id,
                    Day = day,
                    Hour = hour,
                };
                context.RegionDailyCounts.Add(data);
            }

            data.Matches++;

            await context.SaveChangesAsync();
        }

        private List<Ability> GetDraft(Player player)
        {
            return player.ability_upgrades
                        .Select(_ => _.ability)
                        .Distinct()
                        .Where(id => this.skills.Any(s => s.Id == id))
                        .Select(id => this.skills.Find(s => s.Id == id))
                        .OrderBy(_ => _.Id)
                        .ToList();
        }

        private int GetDraftOrder(int slot)
        {
            switch (slot)
            {
                case 0: return 10;
                case 128: return 9;
                case 1: return 8;
                case 129: return 7;
                case 2: return 6;
                case 130: return 5;
                case 3: return 4;
                case 131: return 3;
                case 4: return 2;
                case 132: return 1;
                default:
                    throw new ApplicationException(string.Format("Unknown Slot: {0}", slot));
            }
        }

        private Team GetTeam(int slot)
        {
            return slot < 6 ? Team.Radiant : Team.Dire;
        }

        const long ID_OFFSET = 76561197960265728L;
        static public long ConvertDotaIdToSteamId(long input)
        {
            if (input < 1L)
                return 0;
            else
                return input + ID_OFFSET;
        }
    }
}