using HGV.Basilius;
using HGV.Nullifier.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace HGV.Nullifier.Tools.Export
{
    class Program
    {
        static void Main(string[] args)
        {
            var jsonSettings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            };

            string outputDirectory = "./output";

            if (Directory.Exists(outputDirectory))
            {
                Directory.Delete(outputDirectory, true);
                Directory.CreateDirectory(outputDirectory);
            }
            else
            {
                Directory.CreateDirectory(outputDirectory);
            }

            Console.WriteLine("Exporting Pool Data");

            var client = new MetaClient();

            var heroes = client.GetHeroes();
            var pool = heroes
                .Where(_ => _.AbilityDraftEnabled)
                .ToList();

            var abilities = client.GetAbilities()
                .Where(_ => _.AbilityDraftEnabled == true)
                .ToList();

            var draft_pool = heroes
                .Where(h => h.Enabled == true)
                .Select(h => new
                {
                    Id = h.Id,
                    Enabled = h.AbilityDraftEnabled,
                    Name = h.Name,
                    Img = h.ImageBanner,
                    Abilities = h.Abilities
                        .Where(_ => _.AbilityBehaviors.Contains("DOTA_ABILITY_BEHAVIOR_HIDDEN") == false || _.HasScepterUpgrade == true)
                        .Where(_ => _.Id != Ability.GENERIC)
                        .Select(a => new {
                            Id = a.Id,
                            HeroId = a.HeroId,
                            Name = a.Name,
                            Img = a.Image,
                            IsUltimate = a.IsUltimate,
                            HasUpgrade = a.HasScepterUpgrade,
                            Enabled = a.AbilityDraftEnabled,
                        }).ToList(),
                })
                .OrderBy(_ => _.Name)
                .ToList();

            var json_draft_pool = JsonConvert.SerializeObject(draft_pool, jsonSettings);
            File.WriteAllText("./output/pool.json", json_draft_pool);

            var context = new DataContext();

            Console.WriteLine("Exporting Heroes Data");

            var heroesCollection = context.HeroStats.ToList().Join(pool, h => h.hero, h => h.Id, (lhs, rhs) => new {
                Id = rhs.Id,
                Name = rhs.Name,
                Img = rhs.ImageBanner,
                Picks = lhs.picks,
                Wins = lhs.wins,
                WinRate = lhs.win_rate
            }).ToList();

            Directory.CreateDirectory(outputDirectory + "/heroes");

            var json_heroes = JsonConvert.SerializeObject(heroesCollection, jsonSettings);
            File.WriteAllText("./output/heroes/collection.json", json_heroes);

            var groupsAttributeBaseStrength = pool.GroupBy(_ => _.AttributeBaseStrength).Select(_ => _.Key).OrderBy(_ => _).ToList();
            var groupsAttributeStrengthGain = pool.GroupBy(_ => _.AttributeStrengthGain).Select(_ => _.Key).OrderBy(_ => _).ToList();

            var groupsAttributeBaseAgility = pool.GroupBy(_ => _.AttributeBaseAgility).Select(_ => _.Key).OrderBy(_ => _).ToList();
            var groupsAttributeAgilityGain = pool.GroupBy(_ => _.AttributeAgilityGain).Select(_ => _.Key).OrderBy(_ => _).ToList();

            var groupsAttributeBaseIntelligence = pool.GroupBy(_ => _.AttributeBaseIntelligence).Select(_ => _.Key).OrderBy(_ => _).ToList();
            var groupsAttributeIntelligenceGain = pool.GroupBy(_ => _.AttributeIntelligenceGain).Select(_ => _.Key).OrderBy(_ => _).ToList();

            var groupsStatusHealth = pool.GroupBy(_ => _.StatusHealth).Select(_ => _.Key).OrderBy(_ => _).ToList();
            var groupsStatusHealthRegen = pool.GroupBy(_ => _.StatusHealthRegen).Select(_ => _.Key).OrderBy(_ => _).ToList();

            var groupsStatusMana = pool.GroupBy(_ => _.StatusMana).Select(_ => _.Key).OrderBy(_ => _).ToList();
            var groupsStatusManaRegen = pool.GroupBy(_ => _.StatusManaRegen).Select(_ => _.Key).OrderBy(_ => _).ToList();

            var groupsAttackRange = pool.GroupBy(_ => _.AttackRange).Select(_ => _.Key).OrderBy(_ => _).ToList();
            var groupsAttackDamageMin = pool.GroupBy(_ => _.AttackDamageMin).Select(_ => _.Key).OrderBy(_ => _).ToList();
            var groupsAttackDamageMax = pool.GroupBy(_ => _.AttackDamageMax).Select(_ => _.Key).OrderBy(_ => _).ToList();

            var groupsMovementSpeed = pool.GroupBy(_ => _.MovementSpeed).Select(_ => _.Key).OrderBy(_ => _).ToList();
            var groupsMovementTurnRate = pool.GroupBy(_ => _.MovementTurnRate).Select(_ => _.Key).OrderBy(_ => _).ToList();

            var groupsVisionDaytimeRange = pool.GroupBy(_ => _.VisionDaytimeRange).Select(_ => _.Key).OrderBy(_ => _).ToList();
            var groupsVisionNighttimeRange = pool.GroupBy(_ => _.VisionNighttimeRange).Select(_ => _.Key).OrderBy(_ => _).ToList();

            foreach (var hero in pool)
            {
                string dir = outputDirectory + "/heroes/" + hero.Id.ToString();
                Directory.CreateDirectory(dir);

                var json_hero =JsonConvert.SerializeObject(hero, jsonSettings);
                File.WriteAllText(dir + "/hero.json", json_hero);

                // Attributes...
                var attributes = new HeroAttributes();
                attributes.HeroId = hero.Id;

                attributes.AttributeBaseStrength = hero.AttributeBaseStrength;
                attributes.RankingBaseStrength = (groupsAttributeBaseStrength.IndexOf(hero.AttributeBaseStrength) + 1.0) / groupsAttributeBaseStrength.Count();
                attributes.AttributeStrengthGain = hero.AttributeStrengthGain;
                attributes.RankingStrengthGain = (groupsAttributeStrengthGain.IndexOf(hero.AttributeStrengthGain) + 1.0) / groupsAttributeStrengthGain.Count();

                attributes.AttributeBaseAgility = hero.AttributeBaseAgility;
                attributes.RankingBaseAgility = (groupsAttributeBaseAgility.IndexOf(hero.AttributeBaseAgility) + 1.0) / groupsAttributeBaseAgility.Count();
                attributes.AttributeAgilityGain = hero.AttributeAgilityGain;
                attributes.RankingAgilityGain = (groupsAttributeAgilityGain.IndexOf(hero.AttributeAgilityGain) + 1.0) / groupsAttributeAgilityGain.Count();

                attributes.AttributeBaseIntelligence = hero.AttributeBaseIntelligence;
                attributes.RankingBaseIntelligence = (groupsAttributeBaseIntelligence.IndexOf(hero.AttributeBaseIntelligence) + 1.0) / groupsAttributeBaseIntelligence.Count();
                attributes.AttributeIntelligenceGain = hero.AttributeIntelligenceGain;
                attributes.RankingIntelligenceGain = (groupsAttributeIntelligenceGain.IndexOf(hero.AttributeIntelligenceGain) + 1.0) / groupsAttributeIntelligenceGain.Count();

                attributes.StatusHealth = hero.StatusHealth;
                attributes.RankingHealth = (groupsStatusHealth.IndexOf(hero.StatusHealth) + 1.0) / groupsStatusHealth.Count();
                attributes.StatusHealthRegen = hero.StatusHealthRegen;
                attributes.RankingHealthRegen = (groupsStatusHealthRegen.IndexOf(hero.StatusHealthRegen) + 1.0) / groupsStatusHealthRegen.Count();

                attributes.StatusMana = hero.StatusMana;
                attributes.RankingMana = (groupsStatusMana.IndexOf(hero.StatusMana) + 1.0) / groupsStatusMana.Count();
                attributes.StatusManaRegen = hero.StatusManaRegen;
                attributes.RankingManaRegen = (groupsStatusManaRegen.IndexOf(hero.StatusManaRegen) + 1.0) / groupsStatusManaRegen.Count();

                attributes.AttackRange = hero.AttackRange;
                attributes.RankingAttackRange = (groupsAttackRange.IndexOf(hero.AttackRange) + 1.0) / groupsAttackRange.Count();
                attributes.AttackDamageMin = hero.AttackDamageMin;
                attributes.AttackDamageMax = hero.AttackDamageMax;

                var min = (groupsAttackDamageMin.IndexOf(hero.AttackDamageMin) + 1.0) / groupsAttackDamageMin.Count();
                var max = (groupsAttackDamageMax.IndexOf(hero.AttackDamageMax) + 1.0) / groupsAttackDamageMax.Count();
                var avg = min + max / 2.0;

                attributes.RankingAttackDamage = avg;

                attributes.MovementSpeed = hero.MovementSpeed;
                attributes.RankingMovementSpeed = (groupsMovementSpeed.IndexOf(hero.MovementSpeed) + 1.0) / groupsMovementSpeed.Count();
                attributes.MovementTurnRate = hero.MovementTurnRate;
                attributes.RankingMovementTurnRate = (groupsMovementTurnRate.IndexOf(hero.MovementTurnRate) + 1.0) / groupsMovementTurnRate.Count();

                attributes.VisionDaytimeRange = hero.VisionDaytimeRange;
                attributes.RankingVisionDaytimeRange = (groupsVisionDaytimeRange.IndexOf(hero.VisionDaytimeRange) + 1.0) / groupsVisionDaytimeRange.Count();
                attributes.VisionNighttimeRange = hero.VisionNighttimeRange;
                attributes.RankingVisionNighttimeRange = (groupsVisionNighttimeRange.IndexOf(hero.VisionNighttimeRange) + 1.0) / groupsVisionNighttimeRange.Count();

                var json_attributes = JsonConvert.SerializeObject(attributes, jsonSettings);
                File.WriteAllText(dir + "/attributes.json", json_attributes);

                var stats = heroesCollection.Where(_ => _.Id == hero.Id).FirstOrDefault();
                var json_stats = JsonConvert.SerializeObject(stats, jsonSettings);
                File.WriteAllText(dir + "/stats.json", json_stats);

                // Top Abilities
                var heroAbilities = context.AbilityHeroStats
                    .Where(_ => _.hero == hero.Id)
                    .ToList();

                var totalPicks = (float)heroAbilities.Where(_ => _.is_same_hero == false).Max(_ => _.picks);
                var totalWins = (float)heroAbilities.Where(_ => _.is_same_hero == false).Max(_ => _.wins);
                var totalPicksSame = (float)heroAbilities.Where(_ => _.is_same_hero == true).Max(_ => _.picks);
                var totalWinsSame = (float)heroAbilities.Where(_ => _.is_same_hero == true).Max(_ => _.wins);

                var heroAbilityCollection = heroAbilities.Join(abilities, a => a.ability, a => a.Id, (lhs, rhs) => new {
                        Id = rhs.Id,
                        Name = rhs.Name,
                        Desc = rhs.Description,
                        Img = rhs.Image,
                        IsUltimate = rhs.IsUltimate,
                        HasUpgrade = rhs.HasScepterUpgrade,
                        Picks = lhs.is_same_hero ? lhs.picks / totalPicksSame : lhs.picks / totalPicks,
                        Wins = lhs.is_same_hero ? lhs.wins / totalWinsSame : lhs.wins / totalWins,
                        WinRate = lhs.win_rate
                    })
                    .ToList();

                var json_top_abilities = JsonConvert.SerializeObject(heroAbilityCollection, jsonSettings);
                File.WriteAllText(dir + "/abilities.json", json_top_abilities);
            }

            Console.WriteLine("Exporting Abilities Data");

            var abilitiesCollection = context.AbilityStats.ToList().Join(abilities, a => a.ability, a => a.Id, (lhs, rhs) => new {
                Id = rhs.Id, 
                Name = rhs.Name,
                Desc = rhs.Description,
                Img = rhs.Image,
                IsUltimate = rhs.IsUltimate,
                HasUpgrade = rhs.HasScepterUpgrade,
                Picks = lhs.picks,
                Wins = lhs.wins,
                WinRate = lhs.win_rate
            }).ToList();

            Directory.CreateDirectory(outputDirectory + "/abilities");

            var json_abilities = JsonConvert.SerializeObject(abilitiesCollection, jsonSettings);
            File.WriteAllText("./output/abilities/collection.json", json_abilities);

            foreach (var ability in abilities)
            {
                string dir = outputDirectory + "/abilities/" + ability.Id.ToString();
                Directory.CreateDirectory(dir);

                var json_ability = JsonConvert.SerializeObject(ability, jsonSettings);
                File.WriteAllText(dir + "/ability.json", json_ability);

                var stats = abilitiesCollection.Where(_ => _.Id == ability.Id).FirstOrDefault();
                var json_stats = JsonConvert.SerializeObject(stats, jsonSettings);
                File.WriteAllText(dir + "/stats.json", json_stats);

                var abilityHeroes = context.AbilityHeroStats
                    .Where(_ => _.ability == ability.Id)
                    .Where(_ => _.is_same_hero == false)
                    .ToList();

                if (abilityHeroes.Count == 0)
                    continue;

                var totalHeroPicks = (float)abilityHeroes.Max(_ => _.picks);
                var totalHeroWins = (float)abilityHeroes.Max(_ => _.wins);

                // Top Heroes
                var abilityHeroesCollection = abilityHeroes
                    .Join(pool, h => h.hero, h => h.Id, (lhs, rhs) => new {
                        Id = rhs.Id,
                        Name = rhs.Name,
                        Img = rhs.ImageBanner,
                        Picks = lhs.picks / totalHeroPicks,
                        Wins = lhs.wins / totalHeroWins,
                        WinRate = lhs.win_rate
                    })
                    .ToList();

                var json_top_heroes = JsonConvert.SerializeObject(abilityHeroesCollection, jsonSettings);
                File.WriteAllText(dir + "/heroes.json", json_top_heroes);

                // Top Combos
                var combos = context.AbilityComboStats
                    .Where(_ => _.ability1 == ability.Id || _.ability2 == ability.Id)
                    .Where(_ => _.is_same_hero == false)
                    .ToList();

                var totalComboPicks = (float)combos.Max(_ => _.picks);
                var totalComboWins = (float)combos.Max(_ => _.wins);

                var combo_stats = from s in combos
                                  join a1 in abilities on s.ability1 equals a1.Id
                                  join a2 in abilities on s.ability2 equals a2.Id
                                  select new { Stats = s, Ability = a1.Id == ability.Id ? a2 : a1 };

                var combosCollection = combo_stats.Select(_ => new
                {
                    Id = _.Ability.Id,
                    Name = _.Ability.Name,
                    Img = _.Ability.Image,
                    IsUltimate = _.Ability.IsUltimate,
                    HasUpgrade = _.Ability.HasScepterUpgrade,
                    Picks = _.Stats.picks / totalComboPicks,
                    Wins = _.Stats.wins / totalComboWins,
                    WinRate = _.Stats.win_rate
                }).ToList();

                var json_top_combos = JsonConvert.SerializeObject(combosCollection, jsonSettings);
                File.WriteAllText(dir + "/combos.json", json_top_combos);

               var top10Drafts = context.DraftStat
                    .Where(_ => _.key.Contains(ability.Id.ToString()))
                    .OrderByDescending(_ => _.wins)
                    .Take(10)
                    .ToList()
                    .Select(_ => new {
                        Key = _.key,
                        Images = GetImagesFromKey(_.key, abilities),
                        Abilties = _.names,
                        Wins = _.wins,
                        Picks = _.picks,
                        WinRate = _.win_rate
                    })
                    .ToList();

                var json_top_drafs = JsonConvert.SerializeObject(top10Drafts, jsonSettings);
                File.WriteAllText(dir + "/drafts.json", json_top_drafs);
            }
        }

        private static List<string> GetImagesFromKey(string key, List<Ability> abilities)
        {
            var images = new List<string>();
            for (int i = 0; i < key.Length; i += 4)
            {
                var part = key.Substring(i, 4);
                var id = int.Parse(part);
                var ability = abilities.Where(_ => _.Id == id).FirstOrDefault();
                if (ability != null)
                {
                    images.Add(ability.Image);
                }
            }

            return images;
        } 
    }
}
