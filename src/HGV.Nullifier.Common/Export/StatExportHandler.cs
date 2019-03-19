using HGV.Basilius;
using HGV.Daedalus;
using HGV.Nullifier.Data;
using HGV.Nullifier.Logger;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HGV.Nullifier
{
    public class StatExportHandler
    {
        private ILogger logger;
        private JsonSerializerSettings jsonSettings;
        private string outputDirectory;
        private readonly string apiKey;
  
        public static void Run(string apiKey, CancellationToken t, ILogger l)
        {
            var handler = new StatExportHandler(apiKey, l);
            handler.Initialize();

            var tasks = new Task[] {
                // handler.ExportSummary(),
                // handler.ExportDraftPool(),
                // handler.ExportHeroesSummary(),
                handler.ExportHeroes(),
                // handler.ExportHeroDetails(),
                // handler.ExportAbilities(),
                // handler.ExportUltimates(),
                // handler.ExportTaltents(),
                // handler.ExportAbilityDetails(),
                // handler.ExportAccounts()
            };
            Task.WaitAll(tasks, t);
        }

        public StatExportHandler(string apiKey, ILogger l)
        {
            this.logger = l;

            this.apiKey = apiKey;

            this.outputDirectory = Environment.CurrentDirectory + "\\output";

            this.jsonSettings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            };
        }

        private void WriteResultsToFile(string name, object obj)
        {
            var json = JsonConvert.SerializeObject(obj, jsonSettings);
            File.WriteAllText(this.outputDirectory + "/" + name, json);
        }

        public void Initialize()
        {
            if (Directory.Exists(this.outputDirectory))
                Directory.Delete(this.outputDirectory, true);

            Directory.CreateDirectory(this.outputDirectory);

            if (Directory.Exists(this.outputDirectory) == false)
                throw new DirectoryNotFoundException(this.outputDirectory);

        }

        private int ConvertHODtoCycle(int hour)
        {
            if (hour > 0 && hour <= 6)
                return 1; // Night
            else if (hour > 6 && hour <= 12)
                return 2; // Morning
            else if (hour > 12 && hour <= 18)
                return 3; // Afternoon
            else if (hour > 18 && hour <= 24)
                return 4; // Evening
            else
                return 1; // Unknown
        }

        public async Task ExportSummary()
        {
            var context = new DataContext();
            var client = new MetaClient();
            var apiClient = new DotaApiClient(this.apiKey);

            var matchesCollection = await context.Matches.ToListAsync();

            var totalMatches = (float)matchesCollection.Count();
            var abandonedMatches = matchesCollection.Where(_ => _.valid == 0).Count();
            var minDate = matchesCollection.Min(_ => _.date);
            var maxDate = matchesCollection.Max(_ => _.date);
            var radiant = matchesCollection.Sum(_ => _.victory_radiant);
            var dire = matchesCollection.Sum(_ => _.victory_dire);

            var dailyCounts = matchesCollection.GroupBy(_ => _.day_of_week).ToDictionary(_ => _.Key, _ => _.Count());
            var dailyCycleCounts = matchesCollection.GroupBy(_ => new { day = _.day_of_week, cycle = ConvertHODtoCycle(_.date.Hour) }).ToList();

            var heroes = await GetHeroes();
            var h1 = heroes.OrderByDescending(_ => _.WinRate).FirstOrDefault();

            var abilities = await GetAbilities();
            var a1 = abilities.OrderByDescending(_ => _.Wins).FirstOrDefault();
            var a2 = abilities.OrderByDescending(_ => _.WinRate).FirstOrDefault();

            var summary = new
            {
                Hero = new
                {
                    BestWinRate = h1,
                },
                Ability = new
                {
                    MostWins = a1,
                    BestWinRate = a2,
                },
                Range = new
                {
                    Start = minDate,
                    End = maxDate,
                    Matches = (int)totalMatches,
                    Abandoned = Math.Round(abandonedMatches / totalMatches * 100, 2)
                },
                Team = new
                {
                    Radiant = radiant / totalMatches,
                    Dire = dire / totalMatches,
                },
                Daily = new
                {
                    Sunday = new
                    {
                        total = (int)Math.Round(dailyCounts.GetValueOrDefault(0) / totalMatches * 100),
                        hours = dailyCycleCounts.Where(_ => _.Key.day == 0).ToDictionary(_ => _.Key.cycle, _ => (int)Math.Round(_.Count() / (float)dailyCounts.GetValueOrDefault(0) * 100)),
                    },
                    Monday = new
                    {
                        total = (int)Math.Round(dailyCounts.GetValueOrDefault(1) / totalMatches * 100),
                        hours = dailyCycleCounts.Where(_ => _.Key.day == 1).ToDictionary(_ => _.Key.cycle, _ => (int)Math.Round(_.Count() / (float)dailyCounts.GetValueOrDefault(1) * 100)),
                    },
                    Tuesday = new
                    {
                        total = (int)Math.Round(dailyCounts.GetValueOrDefault(2) / totalMatches * 100),
                        hours = dailyCycleCounts.Where(_ => _.Key.day == 2).ToDictionary(_ => _.Key.cycle, _ => (int)Math.Round(_.Count() / (float)dailyCounts.GetValueOrDefault(2) * 100)),
                    },
                    Wednesday = new
                    {
                        total = (int)Math.Round(dailyCounts.GetValueOrDefault(3) /totalMatches * 100),
                        hours = dailyCycleCounts.Where(_ => _.Key.day == 3).ToDictionary(_ => _.Key.cycle, _ => (int)Math.Round(_.Count() / (float)dailyCounts.GetValueOrDefault(3) * 100)),
                    },
                    Thursday = new
                    {
                        total = (int)Math.Round(dailyCounts.GetValueOrDefault(4) / totalMatches * 100),
                        hours = dailyCycleCounts.Where(_ => _.Key.day == 4).ToDictionary(_ => _.Key.cycle, _ => (int)Math.Round(_.Count() / (float)dailyCounts.GetValueOrDefault(4) * 100)),
                    },
                    Friday = new
                    {
                        total = (int)Math.Round(dailyCounts.GetValueOrDefault(5) / totalMatches * 100),
                        hours = dailyCycleCounts.Where(_ => _.Key.day == 5).ToDictionary(_ => _.Key.cycle, _ => (int)Math.Round(_.Count() / (float)dailyCounts.GetValueOrDefault(5) * 100)),
                    },
                    Saturday = new
                    {
                        total = (int)Math.Round(dailyCounts.GetValueOrDefault(6) / totalMatches * 100),
                        hours = dailyCycleCounts.Where(_ => _.Key.day == 6).ToDictionary(_ => _.Key.cycle, _ => (int)Math.Round(_.Count() / (float)dailyCounts.GetValueOrDefault(6) * 100)),
                    }
                }
            };

            this.WriteResultsToFile("summary.json", summary);
        }

        public async Task ExportDraftPool()
        {
            var context = new DataContext();
            var client = new MetaClient();

            var abilitiesCaptured = await context.Skills.GroupBy(_ => _.ability_id).Select(_ => _.Key).ToListAsync();

            var draftPool = client.GetHeroes()
                .Select(_ => new
                {
                    Id = _.Id,
                    Enabled = _.AbilityDraftEnabled,
                    Name = _.Name,
                    Key = _.Key,
                    Image = string.Format("https://hgv-hyperstone.azurewebsites.net/heroes/banner/{0}.png", _.Key),
                    Abilities = _.Abilities
                        .Where(__ => __.AbilityBehaviors.Contains("DOTA_ABILITY_BEHAVIOR_HIDDEN") == false || __.HasScepterUpgrade == true)
                        .Where(__ => __.AbilityBehaviors.Contains("DOTA_ABILITY_BEHAVIOR_NOT_LEARNABLE") == false || __.HasScepterUpgrade == true)
                        .Select(__ => new
                        {
                            Id = __.Id,
                            HeroId = _.Id,
                            Name = __.Name,
                            Key = __.Key,
                            Image = string.Format("https://hgv-hyperstone.azurewebsites.net/abilities/{0}.png", __.Key),
                            IsUltimate = __.IsUltimate,
                            HasUpgrade = __.HasScepterUpgrade,
                            Enabled = __.AbilityDraftEnabled,
                            HasData = abilitiesCaptured.Contains(__.Id),
                        }).ToList()
                })
                .OrderBy(_ => _.Name)
                .ToList();

            this.WriteResultsToFile("draft-pool.json", draftPool);
        }

        public async Task ExportHeroesSummary()
        {
            var context = new DataContext();
            var client = new MetaClient();

            var heroes = client.GetHeroes();

            var players = await context.Players.GroupBy(_ => _.hero_id).Select(_ => new
            {
                Id = _.Key,
                Wins = (float)_.Sum(__ => __.match_result),
                Picks = (float)_.Count(),
            })
            .ToListAsync();
    
            var collection = players.Join(heroes, _ => _.Id, _ => _.Id, (lhs, rhs) => new
            {
                rhs.AttributePrimary,
                rhs.AttackCapabilities,
                Picks = (float)lhs.Picks,
                Wins = (float)lhs.Wins,
            })
            .ToList();

            var capabilities = collection.GroupBy(_ => _.AttackCapabilities).Select(_ => new
            {
                Key = _.Key,
                WinRate = _.Sum(__ => __.Wins) / _.Sum(__ => __.Picks)
            });

            var attributes = collection.GroupBy(_ => _.AttributePrimary).Select(_ => new
            {
                Key = _.Key,
                WinRate = _.Sum(__ => __.Wins) / _.Sum(__ => __.Picks)
            });

            var list = new List<Tuple<string, string, string>>()
            {
                Tuple.Create("DOTA_UNIT_CAP_RANGED_ATTACK", "Range", "https://hgv-hyperstone.azurewebsites.net/mics/type_range.png"),
                Tuple.Create("DOTA_UNIT_CAP_MELEE_ATTACK", "Melee", "https://hgv-hyperstone.azurewebsites.net/mics/type_melee.png"),
                Tuple.Create("DOTA_ATTRIBUTE_INTELLECT", "Int", "https://hgv-hyperstone.azurewebsites.net/mics/primary_int.png"),
                Tuple.Create("DOTA_ATTRIBUTE_AGILITY", "Agi", "https://hgv-hyperstone.azurewebsites.net/mics/primary_agi.png"),
                Tuple.Create("DOTA_ATTRIBUTE_STRENGTH", "Str", "https://hgv-hyperstone.azurewebsites.net/mics/primary_str.png"),
            };

            var summary = capabilities
                .Union(attributes)
                .Join(list, _ => _.Key, _ => _.Item1, (lhs, rhs) => new
                {
                    Key = lhs.Key,
                    Name = rhs.Item2,
                    WinRate = lhs.WinRate,
                    Image = rhs.Item3,
                });

            this.WriteResultsToFile("hero-summary.json", summary);
        }

        private async Task<List<Common.Export.Hero>> GetHeroes()
        {
            var context = new DataContext();
            var client = new MetaClient();

            var heroes = client.GetHeroes();

            var players = await context.Players.GroupBy(_ => _.hero_id).Select(_ => new
            {
                Id = _.Key,
                Wins = (float)_.Sum(__ => __.match_result),
                Picks = (float)_.Count(),
            })
            .ToListAsync();

            (double sdPicks, double meanPicks, double maxPicks, double minPicks) = players.Deviation(_ => _.Picks);
            (double sdWins, double meanWins, double maxWins, double minWins) = players.Deviation(_ => _.Wins);

            var collection = players.Join(heroes, _ => _.Id, _ => _.Id, (lhs, rhs) => new Common.Export.Hero()
            {
                Id = rhs.Id,
                Name = rhs.Name,
                Key = rhs.Key,
                Image = string.Format("https://hgv-hyperstone.azurewebsites.net/heroes/icons/{0}.png", rhs.Key),
                AttributePrimary = rhs.AttributePrimary,
                AttackCapabilities = rhs.AttackCapabilities,
                WinRateDelta = Math.Round((lhs.Wins / lhs.Picks) * 100, 2) - 50,
                WinRate = Math.Round((lhs.Wins / lhs.Picks) * 100, 2),
                Color = lhs.Picks > meanPicks ? "#67b7dc" : "#FF8800", 
            })
            .OrderBy(_ => _.Name)
            .ToList();

            return collection;
        }

        public async Task ExportHeroes()
        {
            // http://jsfiddle.net/5qho3kmL/15/

            var collection = await GetHeroes();

            this.WriteResultsToFile("hero-collection.json", collection);
        }

        public List<Common.Export.HeroAttribute> GetHeroAttribute()
        {
            var client = new MetaClient();

            var heroesCollection = client.GetHeroes();

            var groupsAttributeBaseStrength = heroesCollection.GroupBy(_ => _.AttributeBaseStrength).Select(_ => _.Key).OrderBy(_ => _).ToList();
            var groupsAttributeStrengthGain = heroesCollection.GroupBy(_ => _.AttributeStrengthGain).Select(_ => _.Key).OrderBy(_ => _).ToList();

            var groupsAttributeBaseAgility = heroesCollection.GroupBy(_ => _.AttributeBaseAgility).Select(_ => _.Key).OrderBy(_ => _).ToList();
            var groupsAttributeAgilityGain = heroesCollection.GroupBy(_ => _.AttributeAgilityGain).Select(_ => _.Key).OrderBy(_ => _).ToList();

            var groupsAttributeBaseIntelligence = heroesCollection.GroupBy(_ => _.AttributeBaseIntelligence).Select(_ => _.Key).OrderBy(_ => _).ToList();
            var groupsAttributeIntelligenceGain = heroesCollection.GroupBy(_ => _.AttributeIntelligenceGain).Select(_ => _.Key).OrderBy(_ => _).ToList();

            var groupsStatusHealth = heroesCollection.GroupBy(_ => _.StatusHealth).Select(_ => _.Key).OrderBy(_ => _).ToList();
            var groupsStatusHealthRegen = heroesCollection.GroupBy(_ => _.StatusHealthRegen).Select(_ => _.Key).OrderBy(_ => _).ToList();

            var groupsStatusMana = heroesCollection.GroupBy(_ => _.StatusMana).Select(_ => _.Key).OrderBy(_ => _).ToList();
            var groupsStatusManaRegen = heroesCollection.GroupBy(_ => _.StatusManaRegen).Select(_ => _.Key).OrderBy(_ => _).ToList();

            var groupsAttackRange = heroesCollection.GroupBy(_ => _.AttackRange).Select(_ => _.Key).OrderBy(_ => _).ToList();
            var groupsAttackDamageMin = heroesCollection.GroupBy(_ => _.AttackDamageMin).Select(_ => _.Key).OrderBy(_ => _).ToList();
            var groupsAttackDamageMax = heroesCollection.GroupBy(_ => _.AttackDamageMax).Select(_ => _.Key).OrderBy(_ => _).ToList();

            var groupsMovementSpeed = heroesCollection.GroupBy(_ => _.MovementSpeed).Select(_ => _.Key).OrderBy(_ => _).ToList();
            var groupsMovementTurnRate = heroesCollection.GroupBy(_ => _.MovementTurnRate).Select(_ => _.Key).OrderBy(_ => _).ToList();

            var groupsVisionDaytimeRange = heroesCollection.GroupBy(_ => _.VisionDaytimeRange).Select(_ => _.Key).OrderBy(_ => _).ToList();
            var groupsVisionNighttimeRange = heroesCollection.GroupBy(_ => _.VisionNighttimeRange).Select(_ => _.Key).OrderBy(_ => _).ToList();

            var collection = heroesCollection.Select(_ => new Common.Export.HeroAttribute()
            {
                HeroId = _.Id,

                AttributeBaseStrength = _.AttributeBaseStrength,
                RankingBaseStrength = (groupsAttributeBaseStrength.IndexOf(_.AttributeBaseStrength) + 1.0) / groupsAttributeBaseStrength.Count(),
                AttributeStrengthGain = _.AttributeStrengthGain,
                RankingStrengthGain = (groupsAttributeStrengthGain.IndexOf(_.AttributeStrengthGain) + 1.0) / groupsAttributeStrengthGain.Count(),

                AttributeBaseAgility = _.AttributeBaseAgility,
                RankingBaseAgility = (groupsAttributeBaseAgility.IndexOf(_.AttributeBaseAgility) + 1.0) / groupsAttributeBaseAgility.Count(),
                AttributeAgilityGain = _.AttributeAgilityGain,
                RankingAgilityGain = (groupsAttributeAgilityGain.IndexOf(_.AttributeAgilityGain) + 1.0) / groupsAttributeAgilityGain.Count(),

                AttributeBaseIntelligence = _.AttributeBaseIntelligence,
                RankingBaseIntelligence = (groupsAttributeBaseIntelligence.IndexOf(_.AttributeBaseIntelligence) + 1.0) / groupsAttributeBaseIntelligence.Count(),
                AttributeIntelligenceGain = _.AttributeIntelligenceGain,
                RankingIntelligenceGain = (groupsAttributeIntelligenceGain.IndexOf(_.AttributeIntelligenceGain) + 1.0) / groupsAttributeIntelligenceGain.Count(),

                StatusHealth = _.StatusHealth,
                RankingHealth = (groupsStatusHealth.IndexOf(_.StatusHealth) + 1.0) / groupsStatusHealth.Count(),
                StatusHealthRegen = _.StatusHealthRegen,
                RankingHealthRegen = (groupsStatusHealthRegen.IndexOf(_.StatusHealthRegen) + 1.0) / groupsStatusHealthRegen.Count(),

                StatusMana = _.StatusMana,
                RankingMana = (groupsStatusMana.IndexOf(_.StatusMana) + 1.0) / groupsStatusMana.Count(),
                StatusManaRegen = _.StatusManaRegen,
                RankingManaRegen = (groupsStatusManaRegen.IndexOf(_.StatusManaRegen) + 1.0) / groupsStatusManaRegen.Count(),

                AttackRange = _.AttackRange,
                RankingAttackRange = (groupsAttackRange.IndexOf(_.AttackRange) + 1.0) / groupsAttackRange.Count(),

                AttackDamageMin = _.AttackDamageMin,
                AttackDamageMax = _.AttackDamageMax,
                RankingAttackDamage = (((groupsAttackDamageMin.IndexOf(_.AttackDamageMin) + 1.0) / groupsAttackDamageMin.Count()) + ((groupsAttackDamageMax.IndexOf(_.AttackDamageMax) + 1.0) / groupsAttackDamageMax.Count()) / 2.0),

                MovementSpeed = _.MovementSpeed,
                RankingMovementSpeed = (groupsMovementSpeed.IndexOf(_.MovementSpeed) + 1.0) / groupsMovementSpeed.Count(),
                MovementTurnRate = _.MovementTurnRate,
                RankingMovementTurnRate = (groupsMovementTurnRate.IndexOf(_.MovementTurnRate) + 1.0) / groupsMovementTurnRate.Count(),

                VisionDaytimeRange = _.VisionDaytimeRange,
                RankingVisionDaytimeRange = (groupsVisionDaytimeRange.IndexOf(_.VisionDaytimeRange) + 1.0) / groupsVisionDaytimeRange.Count(),
                VisionNighttimeRange = _.VisionNighttimeRange,
                RankingVisionNighttimeRange = (groupsVisionNighttimeRange.IndexOf(_.VisionNighttimeRange) + 1.0) / groupsVisionNighttimeRange.Count(),
            })
            .ToList();

            return collection;
        }

        public List<Common.Export.HeroCombo> GetHeroCombos(int heroId, bool flag)
        {
            var context = new DataContext();
            var client = new MetaClient();

            var abilities = client.GetSkills();

            var start = flag == true ? context.Skills.Where(__ => __.is_ulimate == 1) : context.Skills.Where(__ => __.is_skill == 1);
            var query = start
                   .Where(__ => __.hero_id == heroId)
                   .GroupBy(__ => __.ability_id)
                   .Select(__ => new
                   {
                       AbilityId = __.Key,
                       Wins = (float)__.Sum(___ => ___.match_result),
                       Picks = (float)__.Count()
                   })
                   .ToList();

            (double sd, double mean, double max, double min) = query.Deviation(__ => __.Picks);

            var collection = query
                .Where(__ => __.Picks > mean)
                .Join(abilities, _ => _.AbilityId, _ => _.Id, (lhs, rhs) => new Common.Export.HeroCombo()
                {
                    AbilityId = lhs.AbilityId,
                    Image = string.Format("https://hgv-hyperstone.azurewebsites.net/abilities/{0}.png", rhs.Key),
                    Key = rhs.Key,
                    Name = rhs.Name,
                    HasUpgrade = rhs.HasScepterUpgrade,
                    Wins = (int)lhs.Wins,
                    Picks = (int)lhs.Picks,
                    WinRate = lhs.Wins / lhs.Picks
                })
                .OrderByDescending(_ => _.WinRate)
                .Take(10)
                .ToList();

            return collection;
        }

        public async Task ExportHeroDetails()
        {
            var client = new MetaClient();
            var details = client.GetHeroes();

            var heroes = await GetHeroes();
            var abilities = await GetAbilities();
            var ultimates = await GetUltimates();
            var talents = await GetTaltents();
            var attributes = GetHeroAttribute();

            var collection = heroes.Select(_ => 
            {
                return new
                {
                    Summary = _,
                    // Hero = details.Find(__ => __.Id == _.Id),
                    Attributes = attributes.Where(__ => __.HeroId == _.Id).FirstOrDefault(),
                    Abilities = abilities.Where(__ => __.HeroId == _.Id).ToList(),
                    Ultimates = ultimates.Where(__ => __.HeroId == _.Id).ToList(),
                    Talents = talents.Where(__ => __.HeroId == _.Id).ToList(),
                    Combos = new
                    {
                        Abilities = GetHeroCombos(_.Id, false),
                        Ultimates = GetHeroCombos(_.Id, true),
                    }
                };
            })
            .OrderBy(_ => _.Summary.Id)
            .ToDictionary(_ => _.Summary.Id);

            this.WriteResultsToFile("hero-details.json", collection);
        }

        private async Task<List<Common.Export.Ability>> GetAbilities()
        {
            var context = new DataContext();
            var client = new MetaClient();

            var abilities = client.GetAbilities();
            var heroes = client.GetHeroes();

            var query = context.Skills.Where(_ => _.is_skill == 1).GroupBy(_ => _.ability_id);
            var skills = await query.Select(_ => new
            {
                Id = _.Key,
                Wins = (float)_.Sum(__ => __.match_result),
                Picks = (float)_.Count(),
            })
            .ToListAsync();

            (double sdPicks, double meanPicks, double maxPicks, double minPicks) = skills.Deviation(_ => _.Picks);
            (double sdWins, double meanWins, double maxWins, double minWins) = skills.Deviation(_ => _.Wins);

            var collection = skills.Join(abilities, _ => _.Id, _ => _.Id, (lhs, rhs) => new Common.Export.Ability()
            {
                Id = rhs.Id,
                Name = rhs.Name,
                Key = rhs.Key,
                Image = string.Format("https://hgv-hyperstone.azurewebsites.net/abilities/{0}.png", rhs.Key),
                HeroId = rhs.HeroId,
                HasUpgrade = rhs.HasScepterUpgrade,
                Picks = lhs.Picks,
                PicksPercentage = lhs.Picks / maxPicks,
                PicksDeviation = (meanPicks - sdPicks) > lhs.Picks ? -1 : (meanPicks + sdPicks) < lhs.Picks ? 1 : 0,
                Wins = lhs.Wins,
                WinsPercentage = lhs.Wins / maxWins,
                WinsDeviation = (meanWins - sdWins) > lhs.Wins ? -1 : (meanWins + sdWins) < lhs.Wins ? 1 : 0,
                WinRate = lhs.Wins / lhs.Picks,
            })
            .ToList();

            return collection;
        }

        public async Task ExportAbilities()
        {
            var client = new MetaClient();
            var heroes = client.GetHeroes();

            var abilties = await GetAbilities();

            var collection = abilties.Join(heroes, _ => _.HeroId, _ => _.Id, (lhs, rhs) => new
            {
                lhs.Id,
                lhs.Name,
                lhs.Key,
                lhs.Image,
                lhs.HasUpgrade,
                lhs.Picks,
                lhs.PicksPercentage,
                lhs.PicksDeviation,
                lhs.Wins,
                lhs.WinsPercentage,
                lhs.WinsDeviation,
                lhs.WinRate,
                Hero = new
                {
                    Id = lhs.HeroId,
                    rhs.Name,
                    rhs.Key,
                    Image = string.Format("https://hgv-hyperstone.azurewebsites.net/heroes/banner/{0}.png", rhs.Key),
                    rhs.AttributePrimary,
                    rhs.AttackCapabilities,
                }
            })
            .OrderBy(_ => _.Name)
            .ToList();

            this.WriteResultsToFile("abilities-collection.json", collection);
        }

        private async Task<List<Common.Export.Ability>> GetUltimates()
        {
            var context = new DataContext();
            var client = new MetaClient();

            var abilities = client.GetUltimates();
            

            var query = context.Skills.Where(_ => _.is_ulimate == 1).GroupBy(_ => _.ability_id);
            var skills = await query.Select(_ => new
            {
                Id = _.Key,
                Wins = (float)_.Sum(__ => __.match_result),
                Picks = (float)_.Count(),
            })
            .ToListAsync();

            (double sdPicks, double meanPicks, double maxPicks, double minPicks) = skills.Deviation(_ => _.Picks);
            (double sdWins, double meanWins, double maxWins, double minWins) = skills.Deviation(_ => _.Wins);

            var collection = skills.Join(abilities, _ => _.Id, _ => _.Id, (lhs, rhs) => new Common.Export.Ability()
            {
                Id = rhs.Id,
                Name = rhs.Name,
                Key = rhs.Key,
                Image = string.Format("https://hgv-hyperstone.azurewebsites.net/abilities/{0}.png", rhs.Key),
                HeroId = rhs.HeroId,
                HasUpgrade = rhs.HasScepterUpgrade,
                Picks = lhs.Picks,
                PicksPercentage = lhs.Picks / maxPicks,
                PicksDeviation = (meanPicks - sdPicks) > lhs.Picks ? -1 : (meanPicks + sdPicks) < lhs.Picks ? 1 : 0,
                Wins = lhs.Wins,
                WinsPercentage = lhs.Wins / maxWins,
                WinsDeviation = (meanWins - sdWins) > lhs.Wins ? -1 : (meanWins + sdWins) < lhs.Wins ? 1 : 0,
                WinRate = lhs.Wins / lhs.Picks,
            })
            .ToList();

            return collection;
        }

        public async Task ExportUltimates()
        {
            var client = new MetaClient();
            var heroes = client.GetHeroes();

            var ulimates = await GetUltimates();

            var collection = ulimates.Join(heroes, _ => _.HeroId, _ => _.Id, (lhs, rhs) => new
            {
                lhs.Id,
                lhs.Name,
                lhs.Key,
                lhs.Image,
                lhs.HasUpgrade,
                lhs.Picks,
                lhs.PicksPercentage,
                lhs.PicksDeviation,
                lhs.Wins,
                lhs.WinsPercentage,
                lhs.WinsDeviation,
                lhs.WinRate,
                Hero = new
                {
                    Id = lhs.HeroId,
                    rhs.Name,
                    rhs.Key,
                    Image = string.Format("https://hgv-hyperstone.azurewebsites.net/heroes/banner/{0}.png", rhs.Key),
                    rhs.AttributePrimary,
                    rhs.AttackCapabilities,
                }
            })
            .OrderBy(_ => _.Name)
            .ToList();

            this.WriteResultsToFile("ultimates-collection.json", collection);
        }

        public async Task<List<Common.Export.HeroTalent>> GetTaltents()
        {
            var context = new DataContext();
            var client = new MetaClient();

            var abilities = client.GetTalents();
            
            var query = context.Skills.Where(_ => _.is_taltent == 1).GroupBy(_ => _.ability_id);
            var skills = await query.Select(_ => new
            {
                Id = _.Key,
                Wins = (float)_.Sum(__ => __.match_result),
                Picks = (float)_.Count(),
            })
            .ToListAsync();

            (double sdPicks, double meanPicks, double maxPicks, double minPicks) = skills.Deviation(_ => _.Picks);
            (double sdWins, double meanWins, double maxWins, double minWins) = skills.Deviation(_ => _.Wins);

            var collection = skills.Join(abilities, _ => _.Id, _ => _.Id, (lhs, rhs) => new Common.Export.HeroTalent()
            {
                Id = rhs.Id,
                Name = rhs.Name,
                Key = rhs.Key,
                HeroId = rhs.HeroId,
                Picks = lhs.Picks,
                PicksPercentage = lhs.Picks / maxPicks,
                PicksDeviation = (meanPicks - sdPicks) > lhs.Picks ? -1 : (meanPicks + sdPicks) < lhs.Picks ? 1 : 0,
                Wins = lhs.Wins,
                WinsPercentage = lhs.Wins / maxWins,
                WinsDeviation = (meanWins - sdWins) > lhs.Wins ? -1 : (meanWins + sdWins) < lhs.Wins ? 1 : 0,
                WinRate = lhs.Wins / lhs.Picks,
            })
            .ToList();

            return collection;
        }

        public async Task ExportTaltents()
        {
            var client = new MetaClient();
            var heroes = client.GetHeroes();

            var talents = await GetTaltents();

            var collection = talents.Join(heroes, _ => _.HeroId, _ => _.Id, (lhs, rhs) => new
            {
                lhs.Id,
                lhs.Name,
                lhs.Key,
                lhs.Picks,
                lhs.PicksPercentage,
                lhs.PicksDeviation,
                lhs.Wins,
                lhs.WinsPercentage,
                lhs.WinsDeviation,
                lhs.WinRate,
                Hero = new
                {
                    Id = lhs.HeroId,
                    rhs.Name,
                    rhs.Key,
                    Image = string.Format("https://hgv-hyperstone.azurewebsites.net/heroes/banner/{0}.png", rhs.Key),
                    rhs.AttributePrimary,
                    rhs.AttackCapabilities,
                }
            })
            .OrderBy(_ => _.Name)
            .ToList();

            this.WriteResultsToFile("taltents-collection.json", collection);
        }

        public List<Common.Export.AbilityHero> GetAbilityHeroes(int abilityId)
        {
            var context = new DataContext();
            var client = new MetaClient();

            var heroes = client.GetHeroes();

            var query = context.Skills
                .Where(_ => _.ability_id == abilityId)
                .GroupBy(_ => new { _.ability_id, _.hero_id }).Select(_ => new
                {
                    AbilityId = _.Key.ability_id,
                    HeroId = _.Key.hero_id,
                    Wins = (float)_.Sum(__ => __.match_result),
                    Picks = (float)_.Count()
                })
                .ToList();

            (double sd, double mean, double max, double min) = query.Deviation(__ => __.Picks);

            var collection = query
                .Where(__ => __.Picks > mean)
                .Join(heroes, _ => _.HeroId, _ => _.Id, (lhs, rhs) => new Common.Export.AbilityHero()
                {
                    HeroId = rhs.Id,
                    Image = string.Format("https://hgv-hyperstone.azurewebsites.net/heroes/banner/{0}.png", rhs.Key),
                    Key = rhs.Key,
                    Name = rhs.Name,
                    Wins = (int)lhs.Wins,
                    Picks = (int)lhs.Picks,
                    WinRate = lhs.Wins / lhs.Picks,
                })
                .OrderByDescending(_ => _.WinRate)
                .Take(10)
                .ToList();

            return collection;
        }

        public List<Common.Export.HeroCombo> GetAbilityCombos(int abilityId, bool flag)
        {
            var context = new DataContext();
            var client = new MetaClient();

            var skills = client.GetSkills();

            var query = (
                from lhs in context.Skills
                from rhs in context.Skills
                where lhs.ability_id == abilityId && lhs.ability_id != rhs.ability_id &&
                lhs.match_id == rhs.match_id && lhs.hero_id == rhs.hero_id &&
                ((flag == true && rhs.is_ulimate == 1) || (flag == false && rhs.is_skill == 1))
                group rhs by rhs.ability_id into combo
                select new { AbilityId = combo.Key, Picks = (float)combo.Count(), Wins = (float)combo.Sum(_ => _.match_result) } into c
                select new { c.AbilityId, c.Picks, c.Wins, WinRate = c.Wins / c.Picks }
            ).ToList();

            (double sd, double mean, double max, double min) = query.Deviation(__ => __.Picks);

            var collection = query
               .Where(__ => __.Picks > mean)
               .Join(skills, _ => _.AbilityId, _ => _.Id, (lhs, rhs) => new Common.Export.HeroCombo()
               {
                   AbilityId = lhs.AbilityId,
                   Image = string.Format("https://hgv-hyperstone.azurewebsites.net/abilities/{0}.png", rhs.Key),
                   Key = rhs.Key,
                   Name = rhs.Name,
                   Wins = (int)lhs.Wins,
                   Picks = (int)lhs.Picks,
                   WinRate = lhs.Wins / lhs.Picks,
               })
               .OrderByDescending(_ => _.WinRate)
               .Take(10)
               .ToList();

            return collection;
        }

        public async Task ExportAbilityDetails()
        {
            var client = new MetaClient();

            var abilities = await GetAbilities();
            var ultimates = await GetUltimates();
            var details = client.GetSkills();

            var collection = abilities.Union(ultimates).Select(_ =>
            {
                return new
                {
                    Summary = _,
                    Ability = details.Find(__ => __.Id == _.Id),
                    Heroes = GetAbilityHeroes(_.Id),
                    Combos = new
                    {
                        Abilities = GetAbilityCombos(_.Id, false),
                        Ultimates = GetAbilityCombos(_.Id, true),
                    }
                };
            })
            .OrderBy(_ => _.Ability.Id)
            .ToDictionary(_ => _.Ability.Id);

            this.WriteResultsToFile("ability-details.json", collection);

            await Task.Delay(TimeSpan.FromSeconds(1));
        }

        private long GetSteamId(long accountId)
        {
            var account_id = (uint)accountId;
            var id = new SteamID(account_id, EUniverse.Public, EAccountType.Individual);
            return (long)id.ConvertToUInt64();
        }

        public async Task<(List<Common.Export.Player>, double)> GetAccounts()
        {
            const long CATCH_ALL_ACCOUNT_ID = 4294967295;

            var context = new DataContext();
            var metaClient = new MetaClient();

            var players = await context.Players
                .Where(_ => _.account_id != CATCH_ALL_ACCOUNT_ID)
                .GroupBy(_ => _.account_id)
                .Select(_ => new
                {
                    AccountId = _.Key,
                    Wins = (float)_.Sum(__ => __.match_result),
                    Matches = (float)_.Count(),
                })
                .ToListAsync();

            (double sdMatches, double meanMatches, double maxMatches, double minMatches) = players.Deviation(_ => _.Matches);
            var limit = meanMatches;
  
            var collection = players
                .Where(_ => _.Matches > limit)
                .Select(_ => new Common.Export.Player
                {
                    AccountId = _.AccountId,
                    ProfileId = GetSteamId(_.AccountId),
                    Wins = _.Wins,
                    Matches = _.Matches,
                    WinRate = _.Wins / _.Matches,
                })
                .OrderByDescending(_ => _.Matches)
                .ThenByDescending(_ => _.WinRate)
                .ToList();

            return (collection, limit);
        }

        public async Task ExportAccounts()
        {
            var apiClient = new DotaApiClient(this.apiKey);

            var data = await GetAccounts();
            var players = data.Item1;
            var limit = data.Item2;
            var chunks = players.Split(100);

            var collection = new List<Common.Export.Player>();
            foreach (var chunk in chunks)
            {
                var ids = chunk.Select(_ => _.ProfileId).ToList();
                var profiles = await apiClient.GetPlayersSummary(ids);

                var accounts = chunk.Join(profiles, _ => _.ProfileId, _ => _.steamid, (lhs, rhs) => new Common.Export.Player
                {
                    AccountId = lhs.AccountId,
                    ProfileId = lhs.ProfileId,
                    ProfileUrl = rhs.profileurl,
                    Avatar = rhs.avatar,
                    Name = rhs.personaname,
                    Wins = lhs.Wins,
                    Matches = lhs.Matches,
                    WinRate = lhs.WinRate,
                });

                collection.AddRange(accounts);

                await Task.Delay(TimeSpan.FromSeconds(1));
            }

            var ranking = 1;

            var ranked = collection
                .OrderByDescending(_ => _.WinRate)
                .ThenByDescending(_ => _.Matches)
                .Select(_ => new Common.Export.Player
                {
                    AccountId = _.AccountId,
                    ProfileId = _.ProfileId,
                    ProfileUrl = _.ProfileUrl,
                    Avatar = _.Avatar,
                    Name = _.Name,
                    Wins = _.Wins,
                    Matches = _.Matches,
                    WinRate = _.WinRate,
                    Rank = ranking++,
                })
                .ToList();

            this.WriteResultsToFile("players-collection.json", ranked);

            var creators = new List<long>() { 13029812 };

            var leaderboard = new {
                AverageMatches = (int)limit,
                Creators = ranked.Where(_ => creators.Contains(_.AccountId)).ToList(),
                Wins = ranked.OrderByDescending(_ => _.Wins).Take(3).ToList(),
                Matches = ranked.OrderByDescending(_ => _.Matches).Take(3).ToList(),
                WinRate = ranked.OrderByDescending(_ => _.WinRate).Take(3).ToList(),
            };

            this.WriteResultsToFile("leaderboard.json", leaderboard);
            
        }
    }
}
