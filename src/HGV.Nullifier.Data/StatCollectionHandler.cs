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

namespace HGV.Nullifier
{
    public class StatCollectionHandler
    {
        readonly ConcurrentQueue<Match> qAll;
        readonly ConcurrentQueue<Match> qAD;
        readonly ConcurrentStack<Exception> exceptions;
 
        public StatCollectionHandler()
        {
            this.qAll = new ConcurrentQueue<Match>();
            this.qAD = new ConcurrentQueue<Match>();
            this.exceptions = new ConcurrentStack<Exception>();
        }

        public async Task Report()
        {
            while (true)
            {
                var context = new DataContext();

                var countAll = this.qAll.Count;
                var matchesTotal = context.GameModeStats.Sum(_ => _.picks);

                var countAD = this.qAD.Count;
                var matchesAD = context.GameModeStats.Where(_ => _.mode == 18).Select(_ => _.picks).FirstOrDefault();

                var exceptionCount = this.exceptions.Count;

                Console.Clear();

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Last Updated on: {0}", DateTime.Now.ToString("HH:mm"));

                Console.ForegroundColor = (countAll > 0) ? ConsoleColor.Red  : ConsoleColor.Green;
                Console.WriteLine("[All] Queue:{0}, Matches:{1}", countAll, matchesTotal);

                Console.ForegroundColor = (countAD > 0) ? ConsoleColor.Red : ConsoleColor.Green;
                Console.WriteLine("[AD] Queue:{0}, Matches:{1}", countAD, matchesAD);

                Console.ForegroundColor = (exceptionCount > 0) ? ConsoleColor.Red : ConsoleColor.Green;
                Console.WriteLine("Exceptions: {0}", exceptionCount);

                Console.ResetColor();

                await Task.Delay(TimeSpan.FromMinutes(1));
            }
        }

        public async Task Log(EventLog log)
        {
            while (true)
            {
                var context = new DataContext();

                var countAll = this.qAll.Count;
                var matchesTotal = context.GameModeStats.Sum(_ => _.picks);

                var countAD = this.qAD.Count;
                var matchesAD = context.GameModeStats.Where(_ => _.mode == 18).Select(_ => _.picks).FirstOrDefault();

                var summaryAll = string.Format("Queue: {0}", countAll) + System.Environment.NewLine + string.Format("Matches: {0}", matchesTotal);
                log.WriteEntry(summaryAll, EventLogEntryType.Information, 1000);

                var summaryAD = string.Format("Queue: {0}", countAD) + System.Environment.NewLine + string.Format("Matches: {0}", matchesAD);
                log.WriteEntry(summaryAD, EventLogEntryType.Information, 1001);

                if(this.exceptions.Count > 0)
                {
                    var errors = this.exceptions.Select(_ => _.Message).ToArray();
                    var errMessage = string.Join(System.Environment.NewLine, errors);
                    log.WriteEntry(errMessage, EventLogEntryType.Warning, 100);
                    this.exceptions.Clear();
                }

                await Task.Delay(TimeSpan.FromMinutes(10));
            }
        }

        public async Task Collect()
        {
            string key = System.Configuration.ConfigurationManager.AppSettings["DotaApiKey"].ToString();
            DotaApiClient client = new DotaApiClient(key);
            var latest = await client.GetLastestMatches();
            var match_number = latest.Max(_ => _.match_seq_num);

            while (true)
            {
                try
                {
                    var matches = await client.GetMatchesInSequence(match_number);
                    if(matches.Count == 0)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(5));
                        continue;
                    }

                    match_number = matches.Max(_ => _.match_seq_num) + 1;

                    foreach (var match in matches)
                    {
                        this.qAll.Enqueue(match);
                    }

                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
                catch (Exception ex)
                {
                    this.exceptions.Push(ex);

                    await Task.Delay(TimeSpan.FromSeconds(30));
                }

            }
        }

        public async Task Count()
        {
            var client = new MetaClient();

            while (true)
            {
                Match match;
                if (!this.qAll.TryDequeue(out match))
                {
                    continue;
                }

                // Send AD match on to processs
                if (match.game_mode == 18)
                {
                    this.qAD.Enqueue(match);
                }

                try
                {
                    var context = new DataContext();

                    var entity = context.GameModeStats.Where(_ => _.mode == match.game_mode).FirstOrDefault();
                    if (entity == null)
                    {
                        var name = client.GetModeName(match.game_mode);
                        entity = new GameModeStat() { mode = match.game_mode, name = name, picks = 1 };
                        context.GameModeStats.Add(entity);
                    }
                    else
                    {
                        entity.picks++;
                    }

                    await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    this.exceptions.Push(ex);
                }
            }
        }

        public async Task Process()
        {
            var client = new MetaClient();
            var heroes = client.GetHeroes();
            var abilities = client.GetAbilities();
            var skills = abilities.Select(_ => _.Id).ToList();

            while (true)
            {
                Match match;
                if (!this.qAD.TryDequeue(out match))
                {
                    continue;
                }

                try
                {
                    var context = new DataContext();

                    foreach (var player in match.players)
                    {
                        var result = player.player_slot < 6 ? match.radiant_win : !match.radiant_win;

                        var upgrades = player.ability_upgrades
                           .Select(_ => _.ability)
                           .Distinct()
                           .Intersect(skills)
                           .OrderBy(_ => _)
                           .ToArray();

                        if (upgrades.Count() != 4)
                            break;

                        // Draft
                        var draft = string.Join("", upgrades);
                        this.UpdateDraftCount(context, abilities, draft, upgrades, result);

                        // Hero
                        this.UpdateHeroCount(context, heroes, player.hero_id, result);

                        foreach (var id in upgrades)
                        {
                            // Ability
                            this.UpdateAbilityCount(context, abilities, id, result);

                            // Ability Hero
                            this.UpdateAbilityHeroCount(context, heroes, abilities, id, player.hero_id, result);
                        }

                        // Abilities Combos
                        var pairs =
                           from a in upgrades
                           from b in upgrades
                           where a.CompareTo(b) < 0
                           orderby a, b
                           select Tuple.Create(a, b);

                        foreach (var p in pairs)
                        {
                            this.UpdateAbilityComboCount(context, abilities, p.Item1, p.Item2, result);
                        }
                    }

                    await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    this.exceptions.Push(ex);
                }
            }
        }

        private void UpdateAbilityComboCount(DataContext context, List<Ability> abilities, int item1, int item2, bool result)
        {
            var entity = context.AbilityComboStats.Where(_ => _.ability1 == item1 && _.ability2 == item2).FirstOrDefault();
            if (entity == null)
            {
                var a1 = abilities.Where(_ => _.Id == item1).FirstOrDefault();
                var a2 = abilities.Where(_ => _.Id == item2).FirstOrDefault();
                var names = string.Format("{0} | {1}", a1.Name, a2.Name);
                var is_same_hero = a1.HeroId == a2.HeroId;
                entity = new AbilityComboStat() { ability1 = item1, ability2 = item2, names = names, is_same_hero = is_same_hero, picks = 1, wins = result ? 1 : 0, win_rate = 0 };
                context.AbilityComboStats.Add(entity);
            }
            else
            {
                entity.picks++;
                entity.wins += result ? 1 : 0;
                entity.win_rate = (float)entity.wins / (float)entity.picks;
            }
        }

        private void UpdateAbilityCount(DataContext context, List<Ability> abilities, int ability, bool result)
        {
            var entity = context.AbilityStats.Where(_ => _.ability == ability).FirstOrDefault();
            if (entity == null)
            {
                var name = abilities.Where(_ => _.Id == ability).Select(_ => _.Name).FirstOrDefault();
                entity = new AbilityStat() { ability = ability, name=name, picks = 1, wins = result ? 1 : 0, win_rate = 0 };
                context.AbilityStats.Add(entity);
            }
            else
            {
                entity.picks++;
                entity.wins += result ? 1 : 0;
                entity.win_rate = (float)entity.wins / (float)entity.picks;
            }
        }

        private void UpdateAbilityHeroCount(DataContext context, List<Hero> heroes, List<Ability> abilities, int ability, int hero, bool result)
        {
            var entity = context.AbilityHeroStats.Where(_ => _.ability == ability && _.hero == hero).FirstOrDefault();
            if (entity == null)
            {
                var a1 = abilities.Where(_ => _.Id == ability).FirstOrDefault();
                var h1 = heroes.Where(_ => _.Id == hero).FirstOrDefault();
                var names = string.Format("{0} | {1}", h1.Name, a1.Name);
                var is_same_hero = a1.HeroId == h1.Id;
                entity = new AbilityHeroStat() { ability = ability, hero = hero, names = names, is_same_hero = is_same_hero, picks = 1, wins = result ? 1 : 0, win_rate = 0 };
                context.AbilityHeroStats.Add(entity);
            }
            else
            {
                entity.picks++;
                entity.wins += result ? 1 : 0;
                entity.win_rate = (float)entity.wins / (float)entity.picks;
            }
        }

        private void UpdateHeroCount(DataContext context, List<Hero> heroes, int hero, bool result)
        {
            var entity = context.HeroStats.Where(_ => _.hero == hero).FirstOrDefault();
            if (entity == null)
            {
                var name = heroes.Where(_ => _.Id == hero).Select(_ => _.Name).FirstOrDefault();
                entity = new HeroStat() { hero = hero, name=name, picks = 1, wins = result ? 1 : 0, win_rate = 0 };
                context.HeroStats.Add(entity);
            }
            else
            {
                entity.picks++;
                entity.wins += result ? 1 : 0;
                entity.win_rate = (float)entity.wins / (float)entity.picks;
            }
        }

        private void UpdateDraftCount(DataContext context, List<Ability> abilities, string key, int[] upgrades, bool result)
        {
            var entity = context.DraftStat.Where(_ => _.key == key).FirstOrDefault();
            if (entity == null)
            {
                var collection = abilities.Join(upgrades, _ => _.Id, _ => _, (lhs, rhs) => lhs).ToList();
                var collectionNames = collection.Select(_ => _.Name).ToArray();
                var names = string.Join(" | ", collectionNames);
                var is_same_hero = collection.GroupBy(_ => _.HeroId).Count() < 2;
                entity = new DraftStat() { key = key, names=names, is_same_hero = is_same_hero, picks = 1, wins = result ? 1 : 0, win_rate = 0 };
                context.DraftStat.Add(entity);
            }
            else
            {
                entity.picks++;
                entity.wins += result ? 1 : 0;
                entity.win_rate = (float)entity.wins / (float)entity.picks;
            }
        }
    }
}
