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
using System.Text;
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

            var tasks = new Task[1] {
                /*
                handler.ExportSummary(),
                handler.ExportDraftPool(),
                handler.ExportHeroes(),
                handler.ExportAbilities(),
                handler.ExportUlimates(),
                handler.ExportTaltents(),
                */
                handler.ExportAccounts()
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

        public async Task ExportSummary()
        {
            var context = new DataContext();
            var client = new MetaClient();

            var totalMatches = await context.Matches.CountAsync();
            var minDate = await context.Matches.MinAsync(_ => _.date);
            var maxDate = await context.Matches.MaxAsync(_ => _.date);

            var collection = await context.Players.GroupBy(_ => _.match).Select(_ => new
            {
                Radiant = _.Where(__ => __.team == 0).Select(__ => __.match_result).FirstOrDefault(),
                Dire = _.Where(__ => __.team == 1).Select(__ => __.match_result).FirstOrDefault(),
            }).ToListAsync();

            var radiant = (float)collection.Sum(_ => _.Radiant);
            var dire = (float)collection.Sum(_ => _.Dire);

            var obj = new
            {
                MatchesProcessed = totalMatches,
                StartOfRange = minDate,
                EndOfRange = maxDate,
                RadiantWinRate = radiant / totalMatches,
                DireWinRate = dire / totalMatches,
            };

            this.WriteResultsToFile("summary.json", obj);
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
                    Abilities = _.Abilities.Where(__ => __.Id != Ability.GENERIC).Select(__ => new
                    {
                        Id = __.Id,
                        HeroId = _.Id,
                        Name = __.Name,
                        Key = _.Key,
                        IsUltimate = __.IsUltimate,
                        HasUpgrade = __.HasScepterUpgrade,
                        Enabled = __.AbilityDraftEnabled,
                        HasData = abilitiesCaptured.Contains(__.Id),
                    }).ToList()
                })
                .OrderBy(_ => _.Name)
                .ToList();

            this.WriteResultsToFile("pool.json", draftPool);
        }

        public async Task ExportHeroes()
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

            var collection = players.Join(heroes, _ => _.Id, _ => _.Id, (lhs, rhs) => new
            {
                Id = rhs.Id,
                Name = rhs.Name,
                Key = rhs.Key,
                AttributePrimary = rhs.AttributePrimary,
                AttackCapabilities = rhs.AttackCapabilities,
                Picks = lhs.Picks / maxPicks,
                PicksDeviation = (meanPicks - sdPicks) > lhs.Picks ? -1 : (meanPicks + sdPicks) < lhs.Picks ? 1 : 0,
                Wins = lhs.Wins / maxWins,
                WinsDeviation = (meanWins - sdWins) > lhs.Wins ? -1 : (meanWins + sdWins) < lhs.Wins ? 1 : 0,
                WinRate = lhs.Wins / lhs.Picks,
            })
            .OrderBy(_ => _.Name)
            .ToList();

            this.WriteResultsToFile("heroes.json", collection);
        }

        public async Task ExportAbilities()
        {
            var context = new DataContext();
            var client = new MetaClient();

            var abilities = client.GetAbilities();

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

            var collection = skills.Join(abilities, _ => _.Id, _ => _.Id, (lhs, rhs) => new
            {
                Id = rhs.Id,
                Name = rhs.Name,
                Key = rhs.Key,
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
            .OrderBy(_ => _.Name)
            .ToList();

            this.WriteResultsToFile("abilities.json", collection);
        }

        public async Task ExportUlimates()
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

            var collection = skills.Join(abilities, _ => _.Id, _ => _.Id, (lhs, rhs) => new
            {
                Id = rhs.Id,
                Name = rhs.Name,
                Key = rhs.Key,
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
            .OrderBy(_ => _.Name)
            .ToList();

            this.WriteResultsToFile("ulimates.json", collection);
        }

        public async Task ExportTaltents()
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

            var collection = skills.Join(abilities, _ => _.Id, _ => _.Id, (lhs, rhs) => new
            {
                Id = rhs.Id,
                Name = rhs.Name,
                Key = rhs.Key,
                HeroId = 0,
                Picks = lhs.Picks,
                PicksPercentage = lhs.Picks / maxPicks,
                PicksDeviation = (meanPicks - sdPicks) > lhs.Picks ? -1 : (meanPicks + sdPicks) < lhs.Picks ? 1 : 0,
                Wins = lhs.Wins,
                WinsPercentage = lhs.Wins / maxWins,
                WinsDeviation = (meanWins - sdWins) > lhs.Wins ? -1 : (meanWins + sdWins) < lhs.Wins ? 1 : 0,
                WinRate = lhs.Wins / lhs.Picks,
            })
            .OrderBy(_ => _.Name)
            .ToList();

            this.WriteResultsToFile("taltents.json", collection);
        }

        public async Task ExportAccounts()
        {
            const long CATCH_ALL_ACCOUNT_ID = 4294967295;

            var context = new DataContext();
            var metaClient = new MetaClient();
            var apiClient = new DotaApiClient(this.apiKey);

            var players = await context.Players.Where(_ => _.account_id != CATCH_ALL_ACCOUNT_ID).GroupBy(_ => _.account_id).Select(_ => new
            {
                AccountId = _.Key,
                Wins = (float)_.Sum(__ => __.match_result),
                Matches = (float)_.Count(),
            })
            .ToListAsync();

            (double sdMatches, double meanMatches, double maxMatches, double minMatches) = players.Deviation(_ => _.Matches);
            var high = meanMatches + sdMatches;
            
            var collection = players
            .Where(_ => _.Matches > high)
            .Select(_ => new
            {
                AccountId = _.AccountId,
                ProfileId = (long)(new SteamID((uint)_.AccountId, EUniverse.Public, EAccountType.Individual).ConvertToUInt64()),
                Wins = _.Wins,
                Matches = _.Matches,
                WinRate = _.Wins / _.Matches,
            })
            .OrderByDescending(_ => _.WinRate)
            .ThenByDescending(_ => _.Matches)
            .Take(100)
            .Select(_ => new
            {
                AccountId = _.AccountId,
                // Profile = apiClient.GetPlayerSummaries(_.ProfileId).Result,
                Wins = _.Wins,
                Matches = _.Matches,
                WinRate = _.Wins / _.Matches,
            })
            .ToList();

            var count = collection.Count();

            this.WriteResultsToFile("players.json", collection);
        }
    }
}
