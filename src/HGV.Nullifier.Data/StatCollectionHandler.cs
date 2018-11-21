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

                Console.Clear();

                // Console.WriteLine("Last Updated on: {0}", DateTime.Now.ToString("HH:mm"));
                Console.WriteLine("Summary");
                Console.WriteLine("Queue[All]:{0}, Matches:{1}", countAll, matchesTotal);
                Console.WriteLine("Queue[AD]:{0}, Matches:{1}", countAD, matchesAD);
                Console.WriteLine("Errors: {0}", this.exceptions.Count);

                Exception error;
                if (this.exceptions.TryPeek(out error))
                {
                    Console.WriteLine("Last Error: {0}", error.Message);
                }

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
                        await Task.Delay(TimeSpan.FromSeconds(10));
                        continue;
                    }

                    match_number = matches.Max(_ => _.match_seq_num) + 1;

                    foreach (var match in matches)
                    {
                        this.qAll.Enqueue(match);
                    }

                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
                catch (Exception ex)
                {
                    this.exceptions.Push(ex);

                    await Task.Delay(TimeSpan.FromSeconds(60));
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
            var abilities = client.GetAbilities().Where(_ => _.AbilityDraftEnabled == true).ToList();
            var ability_keys = abilities.Select(_ => _.Id).ToList();
            var talents = heroes.SelectMany(_ => _.Talents).ToList();
            var talent_keys = talents.Select(_ => _.Id).ToList();

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
                           .ToList();

                        var upgrades_abilities = upgrades.Intersect(ability_keys).OrderBy(_ => _).ToArray();
                        if (upgrades_abilities.Count() != 4)
                            break;

                        // Hero
                        this.UpdateHeroCount(context, heroes, player.hero_id, result);

                        foreach (var id in upgrades_abilities)
                        {
                            // Ability
                            this.UpdateAbilityCount(context, abilities, id, result);

                            // Ability Hero
                            this.UpdateAbilityHeroCount(context, heroes, abilities, id, player.hero_id, result);
                        }

                        // Abilities Combos
                        var pairs =
                           from a in upgrades_abilities
                           from b in upgrades_abilities
                           where a.CompareTo(b) < 0
                           orderby a, b
                           select Tuple.Create(a, b);

                        foreach (var p in pairs)
                        {
                            this.UpdateAbilityComboCount(context, abilities, p.Item1, p.Item2, result);
                        }

                        var upgrades_talents = upgrades.Intersect(talent_keys).OrderBy(_ => _).ToArray();
                        foreach (var id in upgrades_talents)
                        {
                            // Talent Hero
                            this.UpdateTalentHeroCount(context, heroes, talents, id, player.hero_id, result);
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
                var wins = result ? 1 : 0;
                entity = new AbilityComboStat() { ability1 = item1, ability2 = item2, names = names, is_same_hero = is_same_hero, picks = 1, wins = wins, win_rate = wins };
                context.AbilityComboStats.Add(entity);
            }
            else
            {
                entity.picks++;
                entity.wins += result ? 1 : 0;
                entity.win_rate = (float)entity.wins / (float)entity.picks;
            }
        }

        private void UpdateAbilityCount(DataContext context, List<Ability> abilities, int abilityId, bool result)
        {
            var entity = context.AbilityStats.Where(_ => _.ability == abilityId).FirstOrDefault();
            if (entity == null)
            {
                var name = abilities.Where(_ => _.Id == abilityId).Select(_ => _.Name).FirstOrDefault();
                var wins = result ? 1 : 0;
                entity = new AbilityStat() { ability = abilityId, name=name, picks = 1, wins = wins, win_rate = wins };
                context.AbilityStats.Add(entity);
            }
            else
            {
                entity.picks++;
                entity.wins += result ? 1 : 0;
                entity.win_rate = (float)entity.wins / (float)entity.picks;
            }
        }

        private void UpdateAbilityHeroCount(DataContext context, List<Hero> heroes, List<Ability> abilities, int abilityId, int heroId, bool result)
        {
            var entity = context.AbilityHeroStats.Where(_ => _.ability == abilityId && _.hero == heroId).FirstOrDefault();
            if (entity == null)
            {
                var hero = heroes.Where(_ => _.Id == heroId).FirstOrDefault();
                var ability = abilities.Where(_ => _.Id == abilityId).FirstOrDefault();
                var names = string.Format("{0} | {1}", hero.Name, ability.Name);
                var is_same_hero = ability.HeroId == hero.Id;
                var wins = result ? 1 : 0;
                entity = new AbilityHeroStat() { ability = abilityId, hero = heroId, names = names, is_same_hero = is_same_hero, picks = 1, wins = wins, win_rate = wins };
                context.AbilityHeroStats.Add(entity);
            }
            else
            {
                entity.picks++;
                entity.wins += result ? 1 : 0;
                entity.win_rate = (float)entity.wins / (float)entity.picks;
            }
        }

        private void UpdateTalentHeroCount(DataContext context, List<Hero> heroes, List<Talent> talents, int talentId, int heroId, bool result)
        {
            var entity = context.TalentHeroStats.Where(_ => _.talent == talentId && _.hero == heroId).FirstOrDefault();
            if (entity == null)
            {
                var hero = heroes.Where(_ => _.Id == heroId).FirstOrDefault();
                var talent = talents.Where(_ => _.Id == talentId).FirstOrDefault();
                var names = string.Format("{0} | {1}", hero.Name, talent.Name);
                var wins = result ? 1 : 0;
                entity = new TalentHeroStat() { talent = talentId, hero = heroId, names = names, picks = 1, wins = wins, win_rate = wins };
                context.TalentHeroStats.Add(entity);
            }
            else
            {
                entity.picks++;
                entity.wins += result ? 1 : 0;
                entity.win_rate = (float)entity.wins / (float)entity.picks;
            }
        }

        private void UpdateHeroCount(DataContext context, List<Hero> heroes, int heroId, bool result)
        {
            var entity = context.HeroStats.Where(_ => _.hero == heroId).FirstOrDefault();
            if (entity == null)
            {
                var name = heroes.Where(_ => _.Id == heroId).Select(_ => _.Name).FirstOrDefault();
                var wins = result ? 1 : 0;
                entity = new HeroStat() { hero = heroId, name=name, picks = 1, wins = wins, win_rate = wins };
                context.HeroStats.Add(entity);
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
