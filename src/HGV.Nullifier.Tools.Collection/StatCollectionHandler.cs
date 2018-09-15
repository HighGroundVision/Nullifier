using HGV.Daedalus;
using HGV.Daedalus.GetMatchDetails;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HGV.Nullifier.Data;
using HGV.Basilius;

namespace HGV.Nullifier.Tools.Collection
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
                var matchesAD = context.GameModeStats.Where(_ => _.game_mode == 18).Select(_ => _.picks).FirstOrDefault();

                var exceptionCount = this.exceptions.Count;

                // Gruads
                if(exceptionCount > 20 || countAll > 20 || countAD > 20)
                {
                    return; // Reset the system...
                }

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

                    var entity = context.GameModeStats.Where(_ => _.game_mode == match.game_mode).FirstOrDefault();
                    if (entity == null)
                    {
                        entity = new GameModeStat() { game_mode = match.game_mode, picks = 1 };
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
            var skills = client.GetSkills().Select(_ => _.Id).ToList();

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

                        var abilities = player.ability_upgrades
                           .Select(_ => _.ability)
                           .Distinct()
                           .Intersect(skills)
                           .OrderBy(_ => _)
                           .ToArray();

                        if (abilities.Count() != 4)
                            break;

                        // Draft
                        var draft = string.Join("", abilities);
                        this.UpdateDraftCount(context, draft, result);

                        // Hero
                        this.UpdateHeroCount(context, player.hero_id, result);

                        foreach (var id in abilities)
                        {
                            // Ability
                            this.UpdateAbilityCount(context, id, result);

                            // Ability Hero
                            this.UpdateAbilityHeroCount(context, id, player.hero_id, result);
                        }

                        // Abilities Combos
                        var pairs =
                           from a in abilities
                           from b in abilities
                           where a.CompareTo(b) < 0
                           orderby a, b
                           select Tuple.Create(a, b);

                        foreach (var p in pairs)
                        {
                            this.UpdateAbilityComboCount(context, p.Item1, p.Item2, result);
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

        private void UpdateAbilityComboCount(DataContext context, int item1, int item2, bool result)
        {
            var entity = context.AbilityComboStats.Where(_ => _.ability1 == item1 && _.ability2 == item2).FirstOrDefault();
            if (entity == null)
            {
                entity = new AbilityComboStat() { ability1 = item1, ability2 = item2, picks = 1, wins = result ? 1 : 0 };
                context.AbilityComboStats.Add(entity);
            }
            else
            {
                entity.picks++;
                entity.wins += result ? 1 : 0;
            }
        }

        private void UpdateAbilityCount(DataContext context, int ability, bool result)
        {
            var entity = context.AbilityStats.Where(_ => _.ability == ability).FirstOrDefault();
            if (entity == null)
            {
                entity = new AbilityStat() { ability = ability, picks = 1, wins = result ? 1 : 0 };
                context.AbilityStats.Add(entity);
            }
            else
            {
                entity.picks++;
                entity.wins += result ? 1 : 0;
            }
        }

        private void UpdateAbilityHeroCount(DataContext context, int ability, int hero, bool result)
        {
            var entity = context.AbilityHeroStats.Where(_ => _.ability == ability && _.hero == hero).FirstOrDefault();
            if (entity == null)
            {
                entity = new AbilityHeroStat() { ability = ability, hero = hero, picks = 1, wins = result ? 1 : 0 };
                context.AbilityHeroStats.Add(entity);
            }
            else
            {
                entity.picks++;
                entity.wins += result ? 1 : 0;
            }
        }

        private void UpdateHeroCount(DataContext context, int hero, bool result)
        {
            var entity = context.HeroStats.Where(_ => _.hero == hero).FirstOrDefault();
            if (entity == null)
            {
                entity = new HeroStat() { hero = hero, picks = 1, wins = result ? 1 : 0 };
                context.HeroStats.Add(entity);
            }
            else
            {
                entity.picks++;
                entity.wins += result ? 1 : 0;
            }
        }

        private void UpdateDraftCount(DataContext context, string key, bool result)
        {
            var entity = context.DraftStat.Where(_ => _.key == key).FirstOrDefault();
            if (entity == null)
            {
                entity = new DraftStat() { key = key, picks = 1, wins = result ? 1 : 0 };
                context.DraftStat.Add(entity);
            }
            else
            {
                entity.picks++;
                entity.wins += result ? 1 : 0;
            }
        }
    }
}
