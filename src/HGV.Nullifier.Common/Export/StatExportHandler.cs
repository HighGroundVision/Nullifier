using HGV.Basilius;
using HGV.Daedalus;
using HGV.Nullifier.Common.Extensions;
using HGV.Nullifier.Data;
using HGV.Nullifier.Logger;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
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

        public StatExportHandler(ILogger l, string apiKey, string output)
        {
            this.logger = l;
            this.outputDirectory = output;

            this.metaClient = new MetaClient();
            this.apiClient = new DotaApiClient(apiKey);
            this.context = new DataContext();

            this.jsonSettings = new JsonSerializerSettings()
            {
                Formatting = Formatting.None,
                // Formatting = Formatting.Indented,
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            };
        }

        public void Run()
        {
            Initialize();

            Export_DraftPool_Page();
            Export_Heroes_Page();
            Export_Abilities_Page();
            Export_Timeline_Page();
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

        public void Export_DraftPool_Page()
        {
            var abilitiesCaptured = this.context.AbilityDailyCounts.Select(_ => _.AbilityId).Distinct().ToList();

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

        public void Export_Heroes_Page()
        {
            var today = DateTime.UtcNow.Subtract(TimeSpan.FromDays(3));
            var current = today.DayOfYear;
            var preivous = today.Subtract(TimeSpan.FromDays(3)).DayOfYear;

            var yesterday_heroes = this.context.HeroDailyCounts
               .Where(_ => _.Day > current)
               .GroupBy(_ => new { _.HeroId, _.HeroName }) 
               .Select(_ => new
               {
                   HeroId = _.Key.HeroId,
                   HeroName = _.Key.HeroName,
                   Wins = _.Sum(x => x.Wins),
                   Losses = _.Sum(x => x.Losses),
               })
               .ToList();

            var days_ago_heroes = this.context.HeroDailyCounts
               .Where(_ => _.Day > preivous && _.Day < current)
               .GroupBy(_ => new { _.HeroId, _.HeroName })
               .Select(_ => new
               {
                   HeroId = _.Key.HeroId,
                   HeroName = _.Key.HeroName,
                   Wins = _.Sum(x => x.Wins),
                   Losses = _.Sum(x => x.Losses),
               })
               .ToList();

            var collection = yesterday_heroes
               .Join(days_ago_heroes, _ => _.HeroId, _ => _.HeroId, (lhs, rhs) => new
               {
                   Id = lhs.HeroId,
                   Name = lhs.HeroName,
                   Preivous = (float)rhs.Wins / (rhs.Wins + rhs.Losses),
                   Current = (float)lhs.Wins / (lhs.Wins + lhs.Losses),
               })
               .ToList();

            this.WriteResultsToFile("chart-heroes-data.json", collection);


            var categories = this.metaClient.GetHeroes().Select(_ => new
            {
                _.Name,
                Image = _.ImageIcon,
            }).ToList();

            this.WriteResultsToFile("chart-heroes-categories.json", categories);
        }

        public void Export_Abilities_Page()
        {
            var categories = this.metaClient.GetSkills()
                .Where(_ => _.IsSkill == true || _.IsUltimate == true)
                .Select(_ => new
                {
                    _.Id,
                    _.Name,
                    Image = _.Image,
                }).ToList();

            this.WriteResultsToFile("chart-abilities-categories.json", categories);

            var current = DateTime.UtcNow.Subtract(TimeSpan.FromDays(3)).DayOfYear;
            var query = this.context.AbilityDailyCounts
               .Where(_ => _.Day > current)
               .GroupBy(_ => new { _.AbilityId, _.AbilityName })
               .Select(_ => new
               {
                   _.Key.AbilityId,
                   _.Key.AbilityName,
                   Wins = (float)_.Sum(x => x.Wins),
                   Losses = (float)_.Sum(x => x.Losses),
                   DraftOrder = _.Sum(x => x.DraftOrder),
                   MostKills = _.Sum(x => x.MostKills),
               })
               .ToList();

            var collection = query
                .Join(categories, _ => _.AbilityId, _ => _.Id, (lhs, rhs) => lhs)
               .Select(_ => new
               {
                   Id = _.AbilityId,
                   Name = _.AbilityName,
                   WinRate = Math.Round(_.Wins  / (_.Wins + _.Losses), 4),
                   _.DraftOrder,
                   _.MostKills,
               })
               .ToList();

            this.WriteResultsToFile("chart-abilities-data.json", collection);
        }

        public void Export_Timeline_Page()
        {
            var today = DateTime.Today;
            var daysAgo = today.Subtract(TimeSpan.FromDays(14)).DayOfYear;

            { 
                var query = this.context.RegionDailyCounts
                   .Where(_ => _.Day > daysAgo)
                   .GroupBy(_ => new { _.Day, _.Hour })
                   .Select(_ => new
                   {
                       Hour = _.Key.Hour,
                       Day = _.Key.Day,
                       Value = _.Sum(x => x.Matches),
                   })
                   .OrderBy(_ => _.Day)
                   .ThenBy(_ => _.Hour)
                   .ToList();

                var hours = Enumerable.Range(0, 24);
                var min = query.Min(_ => _.Day);
                var max = query.Max(_ => _.Day);
                var defaults = Enumerable.Range(min, max - min)
                    .SelectMany(d => hours.Select(h => new
                    {
                        Hour = h,
                        Day = d,
                        Value = 0,
                    }))
                   .ToList();

                var results = defaults
                    .GroupJoin(query, _ => new { _.Hour, _.Day }, _ => new { _.Hour, _.Day }, (lhs, rhs) => new { lhs, rhs })
                    .SelectMany(_ => _.rhs.DefaultIfEmpty(), (pair, obj) => new {
                        Hour = pair.lhs.Hour,
                        Day = pair.lhs.Day,
                        Value = obj == null ? pair.lhs.Value : obj.Value,
                    })  
                    .ToList();

                 var collection = results
                    .Select(_ => new
                    {
                        Date = new DateTime(today.Year, 1, 1).AddDays(_.Day - 1).AddHours(_.Hour),
                        _.Value,
                    })
                    .OrderBy(_ => _.Date)
                    .Select(_ => new
                     {
                        Date = _.Date.ToString("u"),
                         _.Value,
                     })
                    .ToList();

                this.WriteResultsToFile("chart-regions-all.json", collection);
            }
            {
                var query = this.context.RegionDailyCounts
                   .Where(_ => _.Day > daysAgo)
                   .GroupBy(_ => new { _.RegionId, _.Day, _.Hour })
                   .Select(_ => new
                   {
                       _.Key.RegionId,
                       _.Key.Hour,
                       _.Key.Day,
                       Value = _.Sum(x => x.Matches),
                   })
                   .OrderBy(_ => _.Day)
                   .ThenBy(_ => _.Hour)
                   .ToList();

                var regions = this.metaClient.GetRegions().Keys;
                var hours = Enumerable.Range(0, 24);
                var min = query.Min(_ => _.Day);
                var max = query.Max(_ => _.Day);
                var defaults = Enumerable.Range(min, max - min)
                    .SelectMany(d => hours.Select(h => new
                    {
                        Hour = h,
                        Day = d,
                        Value = 0,
                    }))
                    .SelectMany(_ => regions.Select(r => new
                    {
                        RegionId = r,
                        Hour = _.Hour,
                        Day = _.Day,
                        Value = 0,
                    }))
                   .ToList();

                var results = defaults
                    .GroupJoin(query, _ => new { _.Hour, _.Day, _.RegionId }, _ => new { _.Hour, _.Day, _.RegionId }, (lhs, rhs) => new { lhs, rhs })
                    .SelectMany(_ => _.rhs.DefaultIfEmpty(), (pair, obj) => new {
                        RegionId = pair.lhs.RegionId,
                        Hour = pair.lhs.Hour,
                        Day = pair.lhs.Day,
                        Value = obj == null ? pair.lhs.Value : obj.Value,
                    })
                    .ToList();

                var collection = results
                    .Select(_ => new
                    {
                        RegionId = _.RegionId,
                        Date = new DateTime(today.Year, 1, 1).AddDays(_.Day - 1).AddHours(_.Hour),
                        _.Value,
                    })
                    .OrderBy(_ => _.RegionId)
                    .ThenBy(_ => _.Date)
                    .Select(_ => new
                    {
                        RegionId = _.RegionId,
                        Date = _.Date.ToString("u"),
                        _.Value,
                    })
                    .ToList();

                this.WriteResultsToFile("chart-regions-data.json", collection);
            }
            {
                var summary = this.context.RegionDailyCounts
                   .GroupBy(_ => _.RegionId)
                   .Select(_ => new
                   {
                       _.Key,
                       Total = _.Sum(r => r.Matches)
                   })
                   .OrderByDescending(_ => _.Total)
                   .ToList()
                   .Select(_ => new
                   {
                       RegionId = _.Key,
                       RegionName = this.metaClient.GetRegionName(_.Key),
                       _.Total
                   })
                   .ToList();

                summary.Insert(0, new { RegionId = -1, RegionName = "World", Total = summary.Sum(_ => _.Total) });

                this.WriteResultsToFile("summary-regions.json", summary);
            }
        }

    }
}
