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

            var heroesCollection = context.HeroStats.ToList().Join(pool, h => h.id, h => h.Id, (lhs, rhs) => new {
                Id = rhs.Id,
                Name = rhs.Name,
                Img = rhs.ImageBanner,
                Picks = lhs.picks,
                Wins = lhs.wins,
                WinRate = lhs.win_rate
            }).ToList();

            Directory.CreateDirectory(outputDirectory + "/heroes");

            var json_heroes = JsonConvert.SerializeObject(draft_pool, jsonSettings);
            File.WriteAllText("./output/heroes/collection.json", json_heroes);

            foreach (var hero in pool)
            {
                string dir = outputDirectory + "/heroes/" + hero.Id.ToString();
                Directory.CreateDirectory(dir);

                var json_hero =JsonConvert.SerializeObject(hero, jsonSettings);
                File.WriteAllText(dir + "/hero.json", json_hero);

                // Top Abilities
                var top10Abilities = context.AbilityHeroStats
                    .Where(_ => _.hero == hero.Id)
                    .OrderByDescending(_ => _.win_rate)
                    .Take(10)
                    .ToList()
                    .Join(abilities, a => a.id, a => a.Id, (lhs, rhs) => new { })
                    .ToList();

                var json_top_abilities = JsonConvert.SerializeObject(top10Abilities, jsonSettings);
                File.WriteAllText(dir + "/abilities.json", json_top_abilities);
            }

            var abilitiesCollection = context.AbilityStats.ToList().Join(abilities, a => a.id, a => a.Id, (lhs, rhs) => new {
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

            var json_abilities = JsonConvert.SerializeObject(draft_pool, jsonSettings);
            File.WriteAllText("./output/abilities/collection.json", json_abilities);

            foreach (var ability in abilities)
            {
                string dir = outputDirectory + "/abilities/" + ability.Id.ToString();
                Directory.CreateDirectory(dir);

                var json_ability = JsonConvert.SerializeObject(ability, jsonSettings);
                File.WriteAllText(dir + "/ability.json", json_ability);

                // Top Heroes
                var top10heroes = context.AbilityHeroStats
                    .Where(_ => _.ability == ability.Id)
                    .OrderByDescending(_ => _.win_rate)
                    .Take(10)
                    .ToList()
                    .Join(pool, h => h.id, h => h.Id, (lhs, rhs) => new {
                        Id = rhs.Id,
                        Name = rhs.Name,
                        Img = rhs.ImageBanner,
                        Picks = lhs.picks,
                        Wins = lhs.wins,
                        WinRate = lhs.win_rate
                    })
                    .ToList();

                var json_top_heroes = JsonConvert.SerializeObject(top10heroes, jsonSettings);
                File.WriteAllText(dir + "/heroes.json", json_top_heroes);

                // Top Combos
                var top10Combos = context.AbilityComboStats
                    .Where(_ => _.ability1 == ability.Id || _.ability2 == ability.Id)
                    .OrderByDescending(_ => _.win_rate)
                    .Take(10)
                    .ToList()
                    .Join(abilities, a => a.id, a => a.Id, (lhs, rhs) => new {
                        Id = rhs.Id,
                        Name = rhs.Name,
                        Desc = rhs.Description,
                        Img = rhs.Image,
                        IsUltimate = rhs.IsUltimate,
                        HasUpgrade = rhs.HasScepterUpgrade,
                        Picks = lhs.picks,
                        Wins = lhs.wins,
                        WinRate = lhs.win_rate
                    })
                    .ToList();

                var json_top_combos = JsonConvert.SerializeObject(top10Combos, jsonSettings);
                File.WriteAllText(dir + "/combos.json", json_top_combos);

                var top10Drafts = context.DraftStat
                    .Where(_ => _.key.Contains(ability.Id.ToString()))
                    .OrderByDescending(_ => _.win_rate)
                    .Take(10)
                    .ToList();

                var json_top_drafts = JsonConvert.SerializeObject(top10Drafts, jsonSettings);
                File.WriteAllText(dir + "/drafts.json", json_top_drafts);
            }
        }
    }
}
