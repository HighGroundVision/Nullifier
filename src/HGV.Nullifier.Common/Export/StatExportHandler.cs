using HGV.Basilius;
using HGV.Daedalus;
using HGV.Nullifier.Data;
using HGV.Nullifier.Logger;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace HGV.Nullifier
{
    public class StatExportHandler
    {
        private ILogger logger;
        private JsonSerializerSettings jsonSettings;
        private string outputDirectory;

        private MetaClient metaClient;
        private DotaApiClient apiClient;
        private DataContext context;

        private StatExportHandler(ILogger l, string apiKey, string output)
        {
            this.logger = l;
            this.outputDirectory = output;

            this.metaClient = new MetaClient();
            this.apiClient = new DotaApiClient(apiKey);
            this.context = new DataContext();

            this.jsonSettings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            };
        }

        public static void Run(ILogger l, string apiKey, string output)
        {
            var handler = new StatExportHandler(l, apiKey, output);
            handler.Initialize();
            handler.ExportSummary();
            handler.ExportSchedule();
            handler.ExportDraftPool();
            handler.ExportHeroesChart();
            handler.ExportHeroesTypes();
            handler.ExportAbilitiesGroups();
            // TODO: Top Ability for each Hero VS Top Abilties Singles
            // TODO: Ability Combos

            // handler.ExportAccounts();
        }

        public void Initialize()
        {
            if (Directory.Exists(this.outputDirectory) == false)
                throw new DirectoryNotFoundException(this.outputDirectory);
        }

        private void WriteResultsToFile(string name, object obj)
        {
            var json = JsonConvert.SerializeObject(obj, this.jsonSettings);
            File.WriteAllText(this.outputDirectory + "/" + name, json);

            this.logger.Info($"Finsihed: {name}");
        }

        private int ConvertAttributePrimary(string primary)
        {
            return primary == "DOTA_ATTRIBUTE_STRENGTH" ? 1 : primary == "DOTA_ATTRIBUTE_AGILITY" ? 2 : primary == "DOTA_ATTRIBUTE_INTELLECT" ? 3 : 0;
        }

        public void ExportSummary()
        {
            // TODO: Remove Summary as all data should be avaible in ohter files...

            // Heroes Summary
            // - Top Hero in each STR, AGI, INT
            // Abilities Summary
            // - Top 3 (By KDA)
            // - Top 3 (By Win Rate)

            var data = new
            {

            };

            this.WriteResultsToFile("summary.json", data);
        }

        public void ExportSchedule()
        {
            var query = this.context.Matches.Where(_ => _.valid == true);

            // Range
            var validMatches = query.Count();
            var abandonedMatches = this.context.Matches.Where(_ => _.valid == false).Count();
            float totalMatches = validMatches + abandonedMatches;
            float abandonedRaito = abandonedMatches / totalMatches;
            var minDate = query.Min(_ => _.date);
            var maxDate = query.Max(_ => _.date);
            var range = new
            {
                Start = minDate,
                End = maxDate,
                Matches = totalMatches,
                AbandonedRaito = abandonedRaito,
            };

            // Teams
            var radiantVictories = query.Sum(_ => _.victory_radiant);
            var direVictories = query.Sum(_ => _.victory_dire);
            var team = new
            {
                Radiant = radiantVictories / validMatches,
                Dire = direVictories / totalMatches,
            };

            // Regions
            var regions = query.GroupBy(_ => _.region).ToDictionary(_ => _.Key, _ => _.Count());

            // Schedules
            var schedule = query
                .GroupBy(_ => new { _.region, _.day_of_week, _.hour_of_day })
                .Select(_ => new
                {
                    Region = _.Key.region,
                    Day = _.Key.day_of_week,
                    Hour = _.Key.hour_of_day,
                    Matches = _.Count()
                })
                .OrderBy(_ => _.Region)
                .ThenBy(_ => _.Day)
                .ThenBy(_ => _.Hour)
                .ToList();

            var data = new
            {
                Range = range,
                Team = team,
                Regions = regions,
                Schedule = schedule
            };

            this.WriteResultsToFile("schedule.json", data);
        }

        public void ExportDraftPool()
        {
            var abilitiesCaptured = this.context.Skills.GroupBy(_ => _.ability_id).Select(_ => _.Key).ToList();

            var draftPool = this.metaClient.GetHeroes()
                .Select(_ => new
                {
                    Id = _.Id,
                    Enabled = _.AbilityDraftEnabled,
                    Name = _.Name,
                    Key = _.Key,
                    Image = _.ImageBanner,
                    Abilities = _.Abilities
                        .Where(__ => __.AbilityBehaviors.Contains("DOTA_ABILITY_BEHAVIOR_HIDDEN") == false || __.HasScepterUpgrade == true)
                        .Where(__ => __.AbilityBehaviors.Contains("DOTA_ABILITY_BEHAVIOR_NOT_LEARNABLE") == false || __.HasScepterUpgrade == true)
                        .Select(__ => new
                        {
                            Id = __.Id,
                            HeroId = _.Id,
                            Name = __.Name,
                            Key = __.Key,
                            Image = __.Image,
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

        public void ExportHeroesChart()
        {
            var vaildMatches = this.context.Matches.Where(_ => _.valid == true);

            var players = this.context.Players
                .Join(vaildMatches, _ => _.match_ref, _ => _.id, (lhs, rhs) => new { lhs, rhs })
                .GroupBy(_ => new { _.lhs.hero_id, _.rhs.region } )
                .Select(_ => new
                {
                    Region = _.Key.region,
                    HeroId = _.Key.hero_id,
                    Wins = _.Sum(x => x.lhs.victory),
                    Matches = _.Count(),
                })
                .ToList();

            var meanCount = players.Average(_ => _.Matches);

            var heroes = this.metaClient.GetHeroes();
            
            var collection = players
                .Join(heroes, _ => _.HeroId, _ => _.Id, (lhs, rhs) => new
                {
                    Region = lhs.Region,
                    Id = lhs.HeroId,
                    Name = rhs.Name,
                    Image = rhs.ImageIcon,
                    Attribute = ConvertAttributePrimary(rhs.AttributePrimary),
                    Matches = lhs.Matches,
                    Wins = lhs.Wins,
                    WinRate = lhs.Wins / (float)lhs.Matches,
                    Limit = lhs.Matches < meanCount ? "#f80" : "#67b7dc",
                })
                .OrderBy(_ => _.Region)
                .ToList();

            this.WriteResultsToFile("heroes-chart.json", collection);
        }

        public void ExportHeroesTypes()
        {
            var vaildMatches = this.context.Matches.Where(_ => _.valid == true);

            var players = this.context.Players
                .Join(vaildMatches, _ => _.match_ref, _ => _.id, (lhs, rhs) => new { lhs, rhs })
                .GroupBy(_ => new { _.lhs.hero_id, _.rhs.region })
                .Select(_ => new
                {
                    Region = _.Key.region,
                    HeroId = _.Key.hero_id,
                    Wins = _.Sum(x => x.lhs.victory),
                    Matches = _.Count(),
                })
                .ToList();

            var heroes = this.metaClient.GetHeroes();

            var collection = players
                .Join(heroes, _ => _.HeroId, _ => _.Id, (lhs, rhs) => new
                {
                    Region = lhs.Region,
                    Attribute = ConvertAttributePrimary(rhs.AttributePrimary),
                    Matches = lhs.Matches,
                    Wins = lhs.Wins,
                })
                .GroupBy(_ => new { _.Region, _.Attribute, })
                .Select(_ => new
                {
                    _.Key.Region,
                    _.Key.Attribute,
                    Matches = _.Sum(x => x.Matches),
                    Wins = _.Sum(x => x.Wins),
                    WinRate = (float)_.Sum(x => x.Wins) / (float)_.Sum(x => x.Matches),
                })
                .OrderBy(_ => _.Region)
                .ThenBy(_ => _.Attribute)
                .ToList();

            this.WriteResultsToFile("heroes-types.json", collection);
        }

        public void ExportAbilitiesGroups()
        {
            var matches = this.context.Matches.Where(_ => _.valid == true);
            var players = this.context.Players;

            var skills = this.context.Skills
                 .Where(_ => _.is_skill == 1 || _.is_ulimate == 1)
                .Join(players, _ => _.player_ref, _ => _.id, (lhs, rhs) => new
                {
                    MatchRef = lhs.match_ref,
                    AbilityId = lhs.ability_id,
                    Victory = rhs.victory
                })
                .Join(matches, _ => _.MatchRef, _ => _.id, (lhs, rhs) => new
                {
                    AbilityId = lhs.AbilityId,
                    Victory = lhs.Victory,
                    Region = rhs.region,
                })
                .GroupBy(_ => new { _.Region, _.AbilityId })
                .Select(_ => new
                {
                    Region = _.Key.Region,
                    AbilityId = _.Key.AbilityId,
                    Wins = _.Sum(x => x.Victory),
                    Matches = _.Count(),
                })
                .ToList();

            var meanCount = skills.Average(_ => _.Matches);

            var abilities = this.metaClient.GetSkills();

            var collection = skills
                .Join(abilities, _ => _.AbilityId, _ => _.Id, (lhs, rhs) => new
                {
                    Region = lhs.Region,
                    Id = lhs.AbilityId,
                    Name = rhs.Name,
                    Image = rhs.Image,
                    Matches = lhs.Matches,
                    Wins = lhs.Wins,
                    WinRate = lhs.Wins / (float)lhs.Matches,
                    Keywords = rhs.Keywords,
                })
                .Select(_ =>
                {
                    return _.Keywords.Select(k => new
                    {
                        _.Region,
                        _.Id,
                        _.Name,
                        _.Image,
                        _.Matches,
                        _.Wins,
                        _.WinRate,
                        Keyword = k
                    }).ToList();
                })
                .SelectMany(_ => _)
                .ToList();

            var groups = collection.GroupBy(_ => new { _.Keyword, _.Region }).ToList();
            var query = groups
                .Select(_ => new
                {
                    Region = _.Key.Region,
                    Keyword = _.Key.Keyword,
                    Count = _.Count(),
                    WinRate = _.Sum(x => x.Wins) / (float)_.Sum(x => x.Matches),
                })
                .OrderByDescending(_ => _.WinRate)
                .ToList();


            this.WriteResultsToFile("abilities-group.json", collection);
        }

        private void ExportAbilitiesSearch()
        {
            var heroes = this.metaClient.GetADHeroes();
            var abilities = this.metaClient.GetAbilities();
            var ultimates = this.metaClient.GetUltimates();

            var collection = abilities
                .Union(ultimates)
                .Join(heroes, _ => _.HeroId, _ => _.Id, (a, h) => new
                {
                    Id = a.Id,
                    Name = a.Name,
                    Image = a.Image,
                    HeroId = h.Id,
                    HeroName = h.Name,
                })
                .ToList();

            this.WriteResultsToFile("abilities-search.json", collection);
        }

        private void ExportBestAbilityOverAllHeroes()
        {

        }

        const long ID_OFFSET = 76561197960265728L;
        static public long ConvertDotaIdToSteamId(long input)
        {
            if (input < 1L)
                return 0;
            else
                return input + ID_OFFSET;
        }

        const long CATCH_ALL_ACCOUNT_ID = 4294967295;
        public void ExportAccounts()
        {
            var matches = this.context.Matches.Where(_ => _.valid == true);

            var players = this.context.Players
                .Where(_ => _.account_id != CATCH_ALL_ACCOUNT_ID)
                .Join(matches, _ => _.match_ref, _ => _.id, (lhs, rhs) => new
                {
                    AccountId = lhs.account_id,
                    Victory = lhs.victory,
                    Region = rhs.region,
                })
                .GroupBy(_ => new { _.AccountId, _.Region })
                .ToList()
                .Select(_ => new
                {
                    ProfileId = ConvertDotaIdToSteamId(_.Key.AccountId),
                    AccountId = _.Key.AccountId,
                    Region = _.Key.Region,
                    Wins = _.Sum(x => x.Victory),
                    Matches = _.Count(),
                    WinRate = (float)_.Sum(x => x.Victory) / (float)_.Count(),
                });

            var AverageMatches = players.Average(_ => _.Matches);

            var collection = players
                .Where(_ => _.Matches > AverageMatches)
                .ToList();

            var chunks = collection
                .GroupBy(_ => _.ProfileId)
                .Select(_ => _.Key)
                .ToList()
                .Split(100);

            var profiles = new List<Daedalus.GetPlayerSummaries.Player>();
            foreach (var chunk in chunks)
            {
                var results = apiClient.GetPlayersSummary(chunk).Result;
                profiles.AddRange(results);

                Task.Delay(TimeSpan.FromSeconds(1)).Wait();
            }

            var accounts = collection
               .Join(profiles, _ => _.ProfileId, _ => _.steamid, (lhs, rhs) => new
               {
                   AccountId = lhs.AccountId,
                   ProfileId = lhs.ProfileId,
                   ProfileUrl = rhs.profileurl,
                   Avatar = rhs.avatar,
                   Name = rhs.personaname,
                   Wins = lhs.Wins,
                   Matches = lhs.Matches,
                   WinRate = lhs.WinRate,
                   Region = lhs.Region,
               })
               .ToList();

            this.WriteResultsToFile("leaderboard-collection.json", accounts);

            var regions = accounts
                .GroupBy(_ => _.Region)
                .ToDictionary(_ => _.Key, _ => _.OrderByDescending(x => x.WinRate).Take(3).ToList());

            this.WriteResultsToFile("leaderboard-regions.json", regions);
        }
    }
}
