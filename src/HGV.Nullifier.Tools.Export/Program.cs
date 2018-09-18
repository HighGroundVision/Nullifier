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

            string outputDirectory = "./Output";

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
                .Where(_ => _.OnCastbar == true)
                .ToList();

            var draft_pool = heroes
                .Where(h => h.Enabled == true)
                .Select(h => new
                {
                    Id = h.Id,
                    Enabled = h.AbilityDraftEnabled,
                    Name = h.Name,
                    Img = h.ImageBanner,
                    Abilities = h.Abilities.Where(_ => _.Id != Ability.GENERIC).Select(a => new {
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
            File.WriteAllText("./Output/DraftPool.json", json_draft_pool);

            var context = new DataContext();

            var heroesCollection = context.HeroStats.Join(pool, h => h.id, h => h.Id, (lhs, rhs) => new {
                Id = rhs.Id,
                Name = rhs.Name,
                Img = rhs.ImageBanner,
                Picks = lhs.picks,
                Wins = lhs.wins,
                WinRate = lhs.win_rate
            }).ToList();

            Directory.CreateDirectory(outputDirectory + "/Heroes");

            var json_heroes = JsonConvert.SerializeObject(draft_pool, jsonSettings);
            File.WriteAllText("./Output/Heroes/Collection.json", json_heroes);

            foreach (var hero in pool)
            {
                string dir = outputDirectory + "/Heroes/" + hero.Id.ToString();
                Directory.CreateDirectory(dir);

                var json_hero =JsonConvert.SerializeObject(hero, jsonSettings);
                File.WriteAllText(dir + "/Hero.json", json_hero);

                // Top Abilities
                var top10Abilities = context.AbilityHeroStats
                    .Where(_ => _.hero == hero.Id)
                    .OrderByDescending(_ => _.win_rate)
                    .Take(10)
                    .ToList();

                var json_top_abilities = JsonConvert.SerializeObject(top10Abilities, jsonSettings);
                File.WriteAllText(dir + "/Abilities.json", json_top_abilities);
            }

            var abilitiesCollection = context.AbilityStats.Join(abilities, a => a.id, a => a.Id, (lhs, rhs) => new {
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

            Directory.CreateDirectory(outputDirectory + "/Abilities");

            var json_abilities = JsonConvert.SerializeObject(draft_pool, jsonSettings);
            File.WriteAllText("./Output/Abilities/Collection.json", json_abilities);

            foreach (var ability in abilities)
            {
                string dir = outputDirectory + "/Abilities/" + ability.Id.ToString();
                Directory.CreateDirectory(dir);

                var json_ability = JsonConvert.SerializeObject(ability, jsonSettings);
                File.WriteAllText(dir + "/Ability.json", json_ability);

                // Top Heroes
                var top10heroes = context.AbilityHeroStats
                    .Where(_ => _.ability == ability.Id)
                    .OrderByDescending(_ => _.win_rate)
                    .Take(10)
                    .ToList();

                var json_top_heroes = JsonConvert.SerializeObject(top10heroes, jsonSettings);
                File.WriteAllText(dir + "/Heroes.json", json_top_heroes);

                // Top Combos
                var top10Combos = context.AbilityComboStats
                    .Where(_ => _.ability1 == ability.Id || _.ability2 == ability.Id)
                    .OrderByDescending(_ => _.win_rate)
                    .Take(10)
                    .ToList();

                var json_top_combos = JsonConvert.SerializeObject(top10Combos, jsonSettings);
                File.WriteAllText(dir + "/Combos.json", json_top_combos);
            }
        }
    }
}
