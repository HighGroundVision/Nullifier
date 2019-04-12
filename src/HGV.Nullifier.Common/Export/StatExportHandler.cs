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
using System.Linq.Expressions;

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

            // Page - Draft Pool
            // handler.ExportDraftPool();

            // Page - Schedule
            // handler.ExportSchedule();

            // Page - Heroes
            // handler.ExportHeroesSearch();
            // handler.ExportHeroesChart();
            // handler.ExportHeroesTypes();

            // Page - Abilities
            // handler.ExportSummaryAbilities();
            // handler.ExportSummaryCombos();
            // handler.ExportAbilitiesGroups();

            // Page - Leaderboard
            // handler.ExportAccounts();

            // Page - Hero
            handler.ExportHeroDetails();
            
            // Page - Ability
            // Summary
            // Attributes
            // Heroes Chart
            // Combos - Skills
            // Combos - Ultimates
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

        public void ExportSummaryAbilities()
        {
            var skills = this.metaClient.GetSkills();

            var query = this.context.Skills
                .Where(_ => _.is_skill == 1 || _.is_ulimate == 1)
                .Join(this.context.Players, _ => _.player_ref, _ => _.id, (skill, player) => new { skill, player })
                .Join(this.context.Matches.Where(_ => _.valid == true), _ => _.skill.match_ref, _ => _.id, (lhs, rhs) => new { skill = lhs.skill, player = lhs.player, match = rhs })
                .Select(_ => new
                {
                     _.skill.ability_id,
                    from_same_source = _.skill.is_hero_same,
                    _.skill.is_skill,
                    _.skill.is_ulimate,
                    ability_level = _.skill.level,
                    _.player.hero_id,
                    hero_level = _.player.level,
                    _.player.draft_order,
                    _.player.level,
                    _.player.team,
                    _.player.kills,
                    _.player.assists,
                    _.player.deaths,
                    damage = _.player.hero_damage + _.player.tower_damage,
                    healing = _.player.hero_healing,
                    kda = (_.player.kills + (_.player.assists / 3.0f)) - _.player.deaths,
                    wins = _.player.victory,
                    _.match.region,
                    _.match.date,
                    _.match.day_of_week,
                    _.match.hour_of_day,
                    _.match.duration,
                })
                .ToList();

            var groups = query.GroupBy(_ => _.ability_id).ToList();

            // 1. Top Abilities by wins
            var byWins = groups
                .Select(_ => new
                {
                    id = _.Key,
                    wins = _.Sum(x => x.wins)
                })
                .OrderByDescending(_ => _.wins)
                .Take(3)
                .Join(skills, _ => _.id, _ => _.Id, (lhs, rhs) => new
                {
                    id = rhs.Id,
                    name = rhs.Name,
                    image = rhs.Image,
                    lhs.wins
                })
                .ToList();

            // 2. Top Abilities by kills
            var byKills = groups
                .Select(_ => new
                {
                    id = _.Key,
                    kills = _.Sum(x => x.kills)
                })
                .OrderByDescending(_ => _.kills)
                .Take(3)
                .Join(skills, _ => _.id, _ => _.Id, (lhs, rhs) => new
                {
                    id = rhs.Id,
                    name = rhs.Name,
                    image = rhs.Image,
                    lhs.kills
                })
                .ToList();

            // 3. Top Abilities by kda
            var byKDA = groups
               .Select(_ => new
               {
                   id = _.Key,
                   kda = ((_.Sum(x => x.kills) + (_.Sum(x => x.assists) / 3.0f)) - _.Sum(x => x.deaths)) / _.Count(),
               })
               .OrderByDescending(_ => _.kda)
               .Take(3)
                .Join(skills, _ => _.id, _ => _.Id, (lhs, rhs) => new
                {
                    id = rhs.Id,
                    name = rhs.Name,
                    image = rhs.Image,
                    lhs.kda
                })
               .ToList();

            // 4. Top Abilities by winrate
            var byWinRate = groups
                .Select(_ => new
                {
                    id = _.Key,
                    winrate = (float)_.Sum(x => x.wins) / (float)_.Count()
                })
                .OrderByDescending(_ => _.winrate)
                .Take(3)
                .Join(skills, _ => _.id, _ => _.Id, (lhs, rhs) => new
                {
                    id = rhs.Id,
                    name = rhs.Name,
                    image = rhs.Image,
                    lhs.winrate
                })
                .ToList();

            // 5. Top Abilities by picks
            var byPicks = groups
                .Select(_ => new
                {
                    id = _.Key,
                    picks = _.Count()
                })
                .OrderByDescending(_ => _.picks)
                .Take(3)
                .Join(skills, _ => _.id, _ => _.Id, (lhs, rhs) => new
                {
                    id = rhs.Id,
                    name = rhs.Name,
                    image = rhs.Image,
                    lhs.picks
                })
                .ToList();

            // But All of these can have major diferences between each heroes
            // 6. Top Abilities aggregated by each hero [Test1.sql]
            var byRank = query
                .GroupBy(_ => new { _.ability_id, _.hero_id })
                .Select(_ => new
                {
                    ability_id = _.Key.ability_id,
                    hero_id = _.Key.hero_id,
                    winrate = (float)_.Sum(x => x.wins) / (float)_.Count(),
                    kda = ((_.Sum(x => x.kills) + (_.Sum(x => x.assists) / 3.0f)) - _.Sum(x => x.deaths)) / _.Count(),
                })
                .GroupBy(_ => _.hero_id)
                .Select(_ => _.OrderByDescending(x => x.kda).FirstOrDefault())
                .GroupBy(_ => _.ability_id)
                .Select(_ => new
                {
                    id = _.Key,
                    count = _.Sum(x => x.ability_id)
                })
                .OrderByDescending(_ => _.count)
                .Take(3)
                .Join(skills, _ => _.id, _ => _.Id, (lhs, rhs) => new
                {
                    id = rhs.Id,
                    name = rhs.Name,
                    image = rhs.Image,
                })
                .ToList();

            var data = new
            {
                wins = byWins,
                picks = byPicks,
                kills = byKills,
                kda = byKDA,
                winrate = byWinRate,
                rank = byRank,
            };

            this.WriteResultsToFile("summary-abilities.json", data);
        }

        public void ExportSummaryCombos()
        {
            var skills = this.metaClient.GetSkills();

            var abilities = this.context.Skills.Where(_ => _.is_skill == 1);
            var ulimates = this.context.Skills.Where(_ => _.is_ulimate == 1);
            var matches = this.context.Matches.Where(_ => _.valid == true);
            var players = this.context.Players;

            // Abilities

            var queryAbilities = abilities
                .Join(abilities, _ => _.player_ref, _ => _.player_ref, (lhs, rhs) => new
                {
                    lhs_ability_id = lhs.ability_id > rhs.ability_id ? lhs.ability_id : rhs.ability_id,
                    rhs_ability_id = lhs.ability_id > rhs.ability_id ? rhs.ability_id : lhs.ability_id,
                    player_ref = lhs.player_ref,
                    match_ref = lhs.match_ref
                })
                .Where(_ => _.lhs_ability_id != _.rhs_ability_id)
                .Distinct()
                .Join(players, _ => _.player_ref, _ => _.id, (combo, player) => new { combo, player })
                .Join(matches, _ => _.combo.match_ref, _ => _.id, (lhs, rhs) => new { combo = lhs.combo, player = lhs.player, match = rhs })
                .Select(_ => new
                {
                    lhs = _.combo.lhs_ability_id,
                    rhs = _.combo.rhs_ability_id,
                    _.player.hero_id,
                    _.player.kills,
                    _.player.deaths,
                    _.player.assists,
                    _.player.victory
                })
                .GroupBy(_ => new { _.lhs, _.rhs })
                .Select(_ => new
                {
                    _.Key.lhs,
                    _.Key.rhs,
                    picks = _.Count(),
                    wins = _.Sum(x => x.victory),
                    kills = _.Sum(x => x.kills),
                    winrate = (float)_.Sum(x => x.victory) / (float)_.Count(),
                    kda = ((_.Sum(x => x.kills) + (_.Sum(x => x.assists) / 3.0f)) - _.Sum(x => x.deaths)) / _.Count(),
                })
                .ToList();

            var abilitiesByWins = queryAbilities
                .OrderByDescending(_ => _.wins)
                .Take(3)
                .Join(skills, _ => _.lhs, _ => _.Id, (lhs, rhs) => new
                {
                    ability1 = rhs,
                    identity2 = lhs.rhs,
                    kda = lhs.kda,
                    wins = lhs.wins,
                    picks = lhs.picks,
                    winrate = lhs.winrate,
                    kills = lhs.kills,
                })
                .Join(skills, _ => _.identity2, _ => _.Id, (lhs, rhs) => new
                {
                    ability1 = new
                    {
                        id = lhs.ability1.Id,
                        name = lhs.ability1.Name,
                        image = lhs.ability1.Image,
                    },
                    ability2 = new
                    {
                        id = rhs.Id,
                        name = rhs.Name,
                        image = rhs.Image
                    },
                    lhs.wins,
                })
                .ToList();

            var abilitiesBykills = queryAbilities
               .OrderByDescending(_ => _.kills)
               .Take(3)
               .Join(skills, _ => _.lhs, _ => _.Id, (lhs, rhs) => new
               {
                   ability1 = rhs,
                   identity2 = lhs.rhs,
                   lhs.kills,
               })
               .Join(skills, _ => _.identity2, _ => _.Id, (lhs, rhs) => new
               {
                   ability1 = new
                   {
                       id = lhs.ability1.Id,
                       name = lhs.ability1.Name,
                       image = lhs.ability1.Image,
                   },
                   ability2 = new
                   {
                       id = rhs.Id,
                       name = rhs.Name,
                       image = rhs.Image
                   },
                   lhs.kills
               })
               .ToList();

            var abilitiesByPicks = queryAbilities
               .OrderByDescending(_ => _.picks)
               .Take(3)
               .Join(skills, _ => _.lhs, _ => _.Id, (lhs, rhs) => new
               {
                   ability1 = rhs,
                   identity2 = lhs.rhs,
                   kda = lhs.kda,
                   picks = lhs.picks,
                   winrate = lhs.winrate,
               })
               .Join(skills, _ => _.identity2, _ => _.Id, (lhs, rhs) => new
               {
                   ability1 = new
                   {
                       id = lhs.ability1.Id,
                       name = lhs.ability1.Name,
                       image = lhs.ability1.Image,
                   },
                   ability2 = new
                   {
                       id = rhs.Id,
                       name = rhs.Name,
                       image = rhs.Image
                   },
                   picks = lhs.picks,
               })
               .ToList();

            var abilitiesByKda = queryAbilities
                .OrderByDescending(_ => _.kda)
                .Take(3)
                .Join(skills, _ => _.lhs, _ => _.Id, (lhs, rhs) => new
                {
                    ability1 = rhs,
                    identity2 = lhs.rhs,
                    kda = lhs.kda,
                    picks = lhs.picks,
                    winrate = lhs.winrate,
                })
                .Join(skills, _ => _.identity2, _ => _.Id, (lhs, rhs) => new
                {
                    ability1 = new
                    {
                        id = lhs.ability1.Id,
                        name = lhs.ability1.Name,
                        image = lhs.ability1.Image,
                    },
                    ability2 = new
                    {
                        id = rhs.Id,
                        name = rhs.Name,
                        image = rhs.Image
                    },
                    kda = lhs.kda,
                })
                .ToList();

            var abilityByWinrate = queryAbilities
                .Where(_ => _.winrate < 1)
                .OrderByDescending(_ => _.winrate)
                .Take(3)
                .Join(skills, _ => _.lhs, _ => _.Id, (lhs, rhs) => new
                {
                    ability1 = rhs,
                    identity2 = lhs.rhs,
                    kda = lhs.kda,
                    winrate = lhs.winrate,
                    picks = lhs.picks,
                })
                .Join(skills, _ => _.identity2, _ => _.Id, (lhs, rhs) => new
                {
                    ability1 = new
                    {
                        id = lhs.ability1.Id,
                        name = lhs.ability1.Name,
                        image = lhs.ability1.Image,
                    },
                    ability2 = new
                    {
                        id = rhs.Id,
                        name = rhs.Name,
                        image = rhs.Image
                    },
                    winrate = lhs.winrate,
                })
                .ToList();


            // Ulimates

            var queryUlimates = abilities
                .Join(ulimates, _ => _.player_ref, _ => _.player_ref, (lhs, rhs) => new
                {
                    lhs_ability_id = lhs.ability_id > rhs.ability_id ? lhs.ability_id : rhs.ability_id,
                    rhs_ability_id = lhs.ability_id > rhs.ability_id ? rhs.ability_id : lhs.ability_id,
                    player_ref = lhs.player_ref,
                    match_ref = lhs.match_ref
                })
                .Where(_ => _.lhs_ability_id != _.rhs_ability_id)
                .Distinct()
                .Join(players, _ => _.player_ref, _ => _.id, (combo, player) => new { combo, player })
                .Join(matches, _ => _.combo.match_ref, _ => _.id, (lhs, rhs) => new { combo = lhs.combo, player = lhs.player, match = rhs })
                .Select(_ => new
                {
                    lhs = _.combo.lhs_ability_id,
                    rhs = _.combo.rhs_ability_id,
                    _.player.hero_id,
                    _.player.kills,
                    _.player.deaths,
                    _.player.assists,
                    _.player.victory
                })
                .GroupBy(_ => new { _.lhs, _.rhs })
                .Select(_ => new
                {
                    _.Key.lhs,
                    _.Key.rhs,
                    wins = _.Sum(x => x.victory),
                    kills = _.Sum(x => x.kills),
                    picks = _.Count(),
                    winrate = (float)_.Sum(x => x.victory) / (float)_.Count(),
                    kda = ((_.Sum(x => x.kills) + (_.Sum(x => x.assists) / 3.0f)) - _.Sum(x => x.deaths)) / _.Count(),
                })
                .ToList();


            var ulimatesByWins = queryUlimates
               .OrderByDescending(_ => _.wins)
               .Take(3)
               .Join(skills, _ => _.lhs, _ => _.Id, (lhs, rhs) => new
               {
                   ability1 = rhs,
                   identity2 = lhs.rhs,
                   wins = lhs.wins,
               })
               .Join(skills, _ => _.identity2, _ => _.Id, (lhs, rhs) => new
               {
                   ability1 = new
                   {
                       id = lhs.ability1.Id,
                       name = lhs.ability1.Name,
                       image = lhs.ability1.Image,
                   },
                   ability2 = new
                   {
                       id = rhs.Id,
                       name = rhs.Name,
                       image = rhs.Image
                   },
                   lhs.wins,
               })
               .ToList();

            var ulimateBykills = queryUlimates
               .OrderByDescending(_ => _.kills)
               .Take(3)
               .Join(skills, _ => _.lhs, _ => _.Id, (lhs, rhs) => new
               {
                   ability1 = rhs,
                   identity2 = lhs.rhs,
                   kils = lhs.kills,
               })
               .Join(skills, _ => _.identity2, _ => _.Id, (lhs, rhs) => new
               {
                   ability1 = new
                   {
                       id = lhs.ability1.Id,
                       name = lhs.ability1.Name,
                       image = lhs.ability1.Image,
                   },
                   ability2 = new
                   {
                       id = rhs.Id,
                       name = rhs.Name,
                       image = rhs.Image
                   },
                   lhs.kils
               })
               .ToList();

            var ulimateByPicks = queryUlimates
               .OrderByDescending(_ => _.picks)
               .Take(3)
               .Join(skills, _ => _.lhs, _ => _.Id, (lhs, rhs) => new
               {
                   ability1 = rhs,
                   identity2 = lhs.rhs,
                   picks = lhs.picks,
               })
               .Join(skills, _ => _.identity2, _ => _.Id, (lhs, rhs) => new
               {
                   ability1 = new
                   {
                       id = lhs.ability1.Id,
                       name = lhs.ability1.Name,
                       image = lhs.ability1.Image,
                   },
                   ability2 = new
                   {
                       id = rhs.Id,
                       name = rhs.Name,
                       image = rhs.Image
                   },
                   lhs.picks
               })
               .ToList();

            var ulimatesByKda = queryUlimates
                .OrderByDescending(_ => _.kda)
                .Take(3)
                .Join(skills, _ => _.lhs, _ => _.Id, (lhs, rhs) => new
                {
                    ability1 = rhs,
                    identity2 = lhs.rhs,
                    kda = lhs.kda,
                })
                .Join(skills, _ => _.identity2, _ => _.Id, (lhs, rhs) => new
                {
                    ability1 = new
                    {
                        id = lhs.ability1.Id,
                        name = lhs.ability1.Name,
                        image = lhs.ability1.Image,
                    },
                    ability2 = new
                    {
                        id = rhs.Id,
                        name = rhs.Name,
                        image = rhs.Image
                    },
                    lhs.kda,
                })
                .ToList();

            var ulimatesByWinrate = queryUlimates
                .Where(_ => _.winrate < 1)
                .OrderByDescending(_ => _.winrate)
                .Take(3)
                .Join(skills, _ => _.lhs, _ => _.Id, (lhs, rhs) => new
                {
                    ability1 = rhs,
                    identity2 = lhs.rhs,
                    lhs.winrate,
                })
                .Join(skills, _ => _.identity2, _ => _.Id, (lhs, rhs) => new
                {
                    ability1 = new
                    {
                        id = lhs.ability1.Id,
                        name = lhs.ability1.Name,
                        image = lhs.ability1.Image,
                    },
                    ability2 = new
                    {
                        id = rhs.Id,
                        name = rhs.Name,
                        image = rhs.Image
                    },
                    winrate = lhs.winrate,
                })
                .ToList();

            var data = new
            {
                abilities = new
                {
                    wins = abilitiesByWins,
                    picks = abilitiesByPicks,
                    kills = abilitiesBykills,
                    kda = abilitiesByKda,
                    winrate = abilityByWinrate
                },
                ulimates = new
                {
                    wins = ulimatesByWins,
                    picks = ulimateByPicks,
                    kills = ulimateBykills,
                    kda = ulimatesByKda,
                    winrate = ulimatesByWinrate
                }
            };

            this.WriteResultsToFile("summary-combos.json", data);
        }

        public void ExportSchedule()
        {
            var query = this.context.Matches.Where(_ => _.valid == true);

            // Range
            float validMatches = (float)query.Count();
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
                Dire = direVictories / validMatches,
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

        public void ExportHeroesSearch()
        {
            var heroes = this.metaClient.GetADHeroes();

            var collection = heroes
                .Select(_ => new 
                {
                    Id = _.Id,
                    Name = _.Name,
                    Image = _.ImageBanner,
                    Icon = _.ImageIcon,
                })
                .ToList();

            this.WriteResultsToFile("heroes-search.json", collection);
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

        private HeroAttribute GetHeroAttribute<T, K>(IList<T> collection, T item, Expression<Func<T, K>> keySelector)
        {
            var selector = keySelector.Compile();

            var ranks = collection.AsQueryable().GroupBy(keySelector).Select(_ => Convert.ToDouble(_.Key)).OrderBy(_ => _).ToList();
            var value = Convert.ToDouble(selector(item));
            var rank = (ranks.IndexOf(value) + 1.0) / ranks.Count();

            return new HeroAttribute() { Rank = rank, Value = value };
        }

        public void ExportHeroDetails()
        {
            // TOOD:
            // Summary
            // Attributes
            // Abilities
            // Talents
            // Combos - Skills
            // Combos - Ultimates
            // Ability Groups

            var matches = this.context.Matches.Where(_ => _.valid == true);
            var players = this.context.Players
                .Join(matches, _ => _.match_ref, _ => _.id, (lhs, rhs) => new { player = lhs, match = rhs })
                .Select(_ => new
                {
                    _.player.hero_id,
                    _.player.kills,
                    _.player.assists,
                    _.player.deaths,
                    wins = _.player.victory,
                })
                .GroupBy(_ => _.hero_id)
                .ToList();

            var abilityQuery = this.context.Skills
                    .Where(_ => _.is_skill == 1 || _.is_ulimate == 1)
                    .Join(this.context.Players, _ => _.player_ref, _ => _.id, (lhs, rhs) => new
                    {
                        id = lhs.ability_id,
                        victory = rhs.victory,
                        kills = rhs.kills,
                        deaths = rhs.deaths,
                        assists = rhs.assists
                    })
                    .GroupBy(_ => _.id)
                    .ToList();

            var talentQuery = this.context.Skills
                   .Where(_ => _.is_taltent == 1)
                   .Join(this.context.Players, _ => _.player_ref, _ => _.id, (lhs, rhs) => new
                   {
                       id = lhs.ability_id,
                       victory = rhs.victory,
                       kills = rhs.kills,
                       deaths = rhs.deaths,
                       assists = rhs.assists
                   })
                    .GroupBy(_ => _.id)
                    .ToList();

            var talentLevels = new Dictionary<int, int>() {
                {0, 10},
                {1, 10},
                {2, 15},
                {3, 15},
                {4, 20},
                {5, 20},
                {6, 25},
                {7, 25},
            };

            var heroes = this.metaClient.GetADHeroes();
            foreach (var item in heroes)
            {
                // Summary
                var group = players.Where(_ => _.Key == item.Id).FirstOrDefault();
                var byWins = group.Sum(_ => _.wins);
                var byKills = group.Sum(_ => _.kills);
                var byKda = ((group.Sum(_ => _.kills) + (group.Sum(_ => _.assists) / 3.0f)) - group.Sum(_ => _.deaths)) / group.Count();
                var byWinRate = (float)group.Sum(_ => _.wins) / (float)group.Count();
                var byPicks = group.Count();

                // Abilities
                var abilityContains = item.Abilities.Select(x => x.Id).ToList();
                var abilityData = abilityQuery.Where(_ => abilityContains.Contains(_.Key));
                var abilities  = item.Abilities.Join(abilityData, _ => _.Id, _ => _.Key, (rhs, lhs) => new
                    {
                        Id = rhs.Id,
                        Key = rhs.Key,
                        Name = rhs.Name,
                        Image = rhs.Image,
                        Wins = lhs.Sum(x => x.victory),
                        Kills = lhs.Sum(x => x.kills),
                        Kda = ((lhs.Sum(x => x.kills) + (lhs.Sum(x => x.assists) / 3.0f)) - lhs.Sum(x => x.deaths)) / lhs.Count(),
                        WinRate = (float)lhs.Sum(x => x.victory) / (float)lhs.Count(),
                        Picks = lhs.Count(),
                    })
                    .ToList();

                // Talents
                var talentContains = item.Talents.Select(x => x.Id).ToList();
                var talentData = talentQuery.Where(_ => talentContains.Contains(_.Key));
                var talentCount = 0;
                var talents = item.Talents.Join(talentData, _ => _.Id, _ => _.Key, (rhs, lhs) => new
                    {
                        Id = rhs.Id,
                        Level = talentLevels[talentCount++],
                        Key = rhs.Key,
                        Name = rhs.Name,
                        Wins = lhs.Sum(x => x.victory),
                        Kills = lhs.Sum(x => x.kills),
                        Kda = ((lhs.Sum(x => x.kills) + (lhs.Sum(x => x.assists) / 3.0f)) - lhs.Sum(x => x.deaths)) / lhs.Count(),
                        WinRate = (float)lhs.Sum(x => x.victory) / (float)lhs.Count(),
                        Picks = lhs.Count(),
                    })
                    .GroupBy(_ => _.Level)
                    .Select(_ => new
                    {
                        Level = _.Key,
                        lhs = _.FirstOrDefault(),
                        rhs = _.LastOrDefault()
                    })
                    .ToList();

                var data = new
                {
                    Summary = new {
                        Wins = byWins,
                        Kills = byKills,
                        Kda = byKda,
                        Winrate = byWinRate,
                        Picks = byPicks,
                    },
                    Attributes = new {
                        AttributeBaseStrength = GetHeroAttribute(heroes, item, _ => _.AttributeBaseStrength),
                        AttributeStrengthGain = GetHeroAttribute(heroes, item, _ => _.AttributeStrengthGain),
                        AttributeBaseAgility = GetHeroAttribute(heroes, item, _ => _.AttributeBaseAgility),
                        AttributeAgilityGain = GetHeroAttribute(heroes, item, _ => _.AttributeAgilityGain),
                        AttributeBaseIntelligence = GetHeroAttribute(heroes, item, _ => _.AttributeBaseIntelligence),
                        AttributeIntelligenceGain = GetHeroAttribute(heroes, item, _ => _.AttributeIntelligenceGain),
                        StatusHealth = GetHeroAttribute(heroes, item, _ => _.StatusHealth),
                        StatusHealthRegen = GetHeroAttribute(heroes, item, _ => _.StatusHealthRegen),
                        StatusMana = GetHeroAttribute(heroes, item, _ => _.StatusMana),
                        StatusManaRegen = GetHeroAttribute(heroes, item, _ => _.StatusManaRegen),
                        AttackRange = GetHeroAttribute(heroes, item, _ => _.AttackRange),
                        AttackDamageMin = GetHeroAttribute(heroes, item, _ => _.AttackDamageMin),
                        AttackDamageMax = GetHeroAttribute(heroes, item, _ => _.AttackDamageMax),
                        MovementSpeed = GetHeroAttribute(heroes, item, _ => _.MovementSpeed),
                        MovementTurnRate = GetHeroAttribute(heroes, item, _ => _.MovementTurnRate),
                        VisionDaytimeRange = GetHeroAttribute(heroes, item, _ => _.VisionDaytimeRange),
                        VisionNighttimeRange = GetHeroAttribute(heroes, item, _ => _.VisionNighttimeRange)
                    },
                    Abilities = abilities,
                    Talents = talents,
                    ComboSkills = new List<object>(),
                    ComboUltimates = new List<object>(),
                    AbilityGroups = new List<object>(),
                };

                this.WriteResultsToFile($"hero.{item.Id}.json", data);
            }
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

            var averageMatches = this.context.Players
                .Where(_ => _.account_id != CATCH_ALL_ACCOUNT_ID)
                .Join(matches, _ => _.match_ref, _ => _.id, (lhs, rhs) => new
                {
                    AccountId = lhs.account_id,
                })
                .GroupBy(_ => _.AccountId)
                .Average(_ => _.Count());

            averageMatches = Math.Ceiling(averageMatches);

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
                })
                .ToList();

            var collection = players
                .Where(_ => _.Matches > averageMatches)
                .ToList();

            var chunks = collection
                .GroupBy(_ => _.ProfileId)
                .Select(_ => _.Key)
                .ToList()
                .Split(100);

            var count = chunks.Count;
            Console.WriteLine($"Downloading Profiles in {count} chunks, will take ~{(count / 60)} mins");

            var profiles = new List<Daedalus.GetPlayerSummaries.Player>();
            int i = 0;
            while (i < count)
            {
                try
                {
                    var chunk = chunks[i];
                    var results = apiClient.GetPlayersSummary(chunk).Result;
                    profiles.AddRange(results);

                    i++;

                    Console.WriteLine($"Processing chuck: {i}");

                    Task.Delay(TimeSpan.FromSeconds(1)).Wait();                  
                }
                catch (Exception)
                {
                    Task.Delay(TimeSpan.FromSeconds(10)).Wait();
                }
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

            var dataPlayers = new
            {
                averageMatches = averageMatches,
                players = accounts,
            };

            this.WriteResultsToFile("leaderboard-collection.json", dataPlayers);

            var regions = accounts
                .GroupBy(_ => _.Region)
                .Select(_ => new
                {
                    region = _.Key,
                    players = _.OrderByDescending(x => x.WinRate).ThenByDescending(x => x.Matches).Take(3).ToList(),
                })
                .OrderBy(_ => _.region)
                .ToDictionary(_ => _.region, _ => _.players);

            var dataRegions = new
            {
                averageMatches = averageMatches,
                regions = regions,
            };

            this.WriteResultsToFile("leaderboard-regions.json", dataRegions);
        }
    }
}
