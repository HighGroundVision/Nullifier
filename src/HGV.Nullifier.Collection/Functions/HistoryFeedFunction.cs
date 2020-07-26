using HGV.Divine;
using HGV.Nullifier.Collection.Models.History;
using HGV.Nullifier.Collection.Models.Stats;
using HGV.Nullifier.Collection.Services;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HGV.Nullifier.Collection.Functions
{
    public class HistoryFeedFunction
    {
        private const long ANONYMOUS_ACCOUNT_ID = 4294967295;

        private readonly IObjectiveService ObjectiveService;
        private readonly ITeamService TeamService;

        private readonly Uri PlayerRecordsCollection;
        private readonly Uri PlayerRatingsCollection;

        public HistoryFeedFunction(
            IObjectiveService objectiveService, 
            ITeamService teamService
        )
        {
            this.ObjectiveService = objectiveService;
            this.TeamService = teamService;

            this.PlayerRecordsCollection = UriFactory.CreateDocumentCollectionUri("hgv-nullifier", "player-records");
            this.PlayerRatingsCollection = UriFactory.CreateDocumentCollectionUri("hgv-nullifier", "player-ratings");

        }

        [FunctionName("HistoryFeed")]
        public async Task ChangeFeed([CosmosDBTrigger(
            databaseName: "hgv-nullifier",
            collectionName: "history",
            ConnectionStringSetting = "CosmosDBConnection",
            LeaseCollectionName = "history-leases",
            CreateLeaseCollectionIfNotExists=true)]
        IReadOnlyList<Document> feed,
        [CosmosDB(ConnectionStringSetting = "CosmosDBConnection")] DocumentClient client,
        ILogger log)
        {
            if (feed == null || feed.Count == 0)
                return;

            foreach (var doc in feed)
            {
                var history = JsonConvert.DeserializeObject<Match>(doc.ToString());

                var records = CreateRecords(history);
                var ratings = GetExitingRatings(client, records);

                UpdateRatingIncluding(ratings, history, log);
                UpdateRatingExcluding(ratings, history, log);
                UpdateRecords(ratings, records);
                
                await CreateRecords(client, records);
                await CreateOrReplaceRatings(client, ratings);
            }
        }

        private List<PlayerRecord> CreateRecords(Match history)
        {
            var records = new List<PlayerRecord>();
            foreach (var player in history.Players)
            {
                var record = new PlayerRecord();
                record.MatchId = history.MatchId;
                record.Cluster = history.Cluster.GetValueOrDefault();
                record.Timestamp = history.StartTime.GetValueOrDefault();
                record.Date = DateTimeOffset.FromUnixTimeSeconds(record.Timestamp).UtcDateTime;
                record.Length = history.Duration.GetValueOrDefault();
                record.Duration = DateTimeOffset.FromUnixTimeSeconds(record.Length).TimeOfDay;

                record.PlayerSlot = player.PlayerSlot.GetValueOrDefault();
                record.Team = this.TeamService.GetTeam(record.PlayerSlot);
                switch (record.Team)
                {
                    case TeamNames.Radiant:
                        record.TeamScore = history.RadiantScore.GetValueOrDefault();
                        record.ObjectivesLost = this.ObjectiveService.CalculateLost(history.TowerStatusRadiant, history.BarracksStatusRadiant);
                        record.ObjectivesTaken = this.ObjectiveService.CalculateLost(history.TowerStatusDire, history.BarracksStatusDire);
                        break;
                    case TeamNames.Dire:
                        record.TeamScore = history.DireScore.GetValueOrDefault();
                        record.ObjectivesLost = this.ObjectiveService.CalculateLost(history.TowerStatusDire, history.BarracksStatusDire);
                        record.ObjectivesTaken = this.ObjectiveService.CalculateLost(history.TowerStatusRadiant, history.BarracksStatusRadiant);
                        break;
                    default:
                        break;
                }

                record.Victory = this.TeamService.Victor(record.Team, history.RadiantWin);
                record.DraftOrder = this.TeamService.GetDraftOrder(record.PlayerSlot);

                record.AccountId = player.AccountId.GetValueOrDefault();
                record.Anonymous = player.AccountId == ANONYMOUS_ACCOUNT_ID;

                record.HeroId = player.HeroId.GetValueOrDefault();
                record.Kills = player.Kills.GetValueOrDefault();
                record.Deaths = player.Deaths.GetValueOrDefault();
                record.Assists = player.Assists.GetValueOrDefault();
                record.LastHits = player.Denies.GetValueOrDefault();

                record.Items = Enumerable.Empty<int?>()
                    .Concat(new int?[] { player.Item0, player.Item1, player.Item2, player.Item3, player.Item4, player.Item5 })
                    .Concat(new int?[] { player.Backpack0, player.Backpack1, player.Backpack2 })
                    .Concat(new int?[] { player.ItemNeutral })
                    .Where(_ => _.HasValue)
                    .Select(_ => _.Value)
                    .Where(v => v != 0)
                    .ToList();

                record.Skills = player.AbilityUpgrades
                    .EmptyIfNull()
                    .Select(_ => _.Ability)
                    .Distinct()
                    .ToList();

                records.Add(record);
            }

            return records;
        }

        private List<PlayerRating> GetExitingRatings(DocumentClient client, List<PlayerRecord> playerRecords)
        {
            var options = new FeedOptions() { EnableCrossPartitionQuery = true };
            var accounts = playerRecords.Select(_ => _.AccountId);
            var sql = $"SELECT * FROM c WHERE c.account_id IN ({string.Join(",", accounts)})";
            var query = client.CreateDocumentQuery<PlayerRating>(this.PlayerRatingsCollection, sql, options).ToList();

            var collection = playerRecords
                .GroupJoin(query, _ => _.AccountId, _ => _.AccountId, (lhs, rhs) => new { Player = lhs, Rating = rhs })
                .SelectMany(_ => _.Rating.DefaultIfEmpty(), (x, c) => 
                {
                    if(c == null)
                    {
                        return new PlayerRating() 
                        { 
                            AccountId = x.Player.AccountId,
                            PlayerSlot = x.Player.PlayerSlot, 
                            Team = x.Player.Team,
                            Matches = 1,
                            Wins = x.Player.Victory ? 1 : 0,
                            LastTimestamp = x.Player.Timestamp,
                            LastDate = x.Player.Date,
                            SkillIncludingAnonymous = new Skill(),
                            SkillExcludingAnonymous = new Skill(),
                        };
                    }
                    else
                    {
                        c.PlayerSlot = x.Player.PlayerSlot;
                        c.Team = x.Player.Team;
                        c.Matches++;
                        c.Wins += x.Player.Victory ? 1 : 0;
                        c.LastTimestamp = x.Player.Timestamp;
                        c.LastDate = x.Player.Date;
                        return c;
                    }
                })
                .ToList();

            return collection;
        }

        private void UpdateRatingIncluding(List<PlayerRating> playerRatings, Match history, ILogger log)
        {
            try
            {
                var gameInfo = GameInfo.DefaultGameInfo;
                var teamStandings = this.TeamService.GetStandings(history.RadiantWin);

                Func<PlayerRating, Rating> converter = (x) => 
                {
                    if(string.IsNullOrEmpty(x.Id))
                        return gameInfo.DefaultRating;
                    else
                        return new Rating(x.SkillIncludingAnonymous.Mean, x.SkillIncludingAnonymous.StandardDeviation);
                };

                var players = playerRatings
                    .GroupBy(_ => _.Team)
                    .OrderBy(_ => _.Key)
                    .Select(_ => _.Aggregate(new Team<int>(), (t, i) => t.AddPlayer(i.PlayerSlot, converter(i))))
                    .ToArray();

                var teams = Teams.Concat(players);
                var quality = TrueSkillCalculator.CalculateMatchQuality(gameInfo, teams);
                var ratings = TrueSkillCalculator.CalculateNewRatings(gameInfo, teams, teamStandings);
                var collection = playerRatings
                    .Join(ratings, _ => _.PlayerSlot, _ => _.Key, (lhs, rhs) => new { PlayerRating = lhs, Rating = rhs })
                    .ToList();

                foreach (var item in collection)
                {
                    item.PlayerRating.SkillIncludingAnonymous.Quality = quality;
                    item.PlayerRating.SkillIncludingAnonymous.Mean = item.Rating.Value.Mean;
                    item.PlayerRating.SkillIncludingAnonymous.StandardDeviation = item.Rating.Value.StandardDeviation;
                    item.PlayerRating.SkillIncludingAnonymous.Rating = item.Rating.Value.ConservativeRating;
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }
        }

        private void UpdateRatingExcluding(List<PlayerRating> playerRatings, Match history, ILogger log)
        {
            try
            {
                var gameInfo = GameInfo.DefaultGameInfo;
                var teamStandings = this.TeamService.GetStandings(history.RadiantWin);

                Func<PlayerRating, Rating> converter = (x) => 
                {
                    if(string.IsNullOrEmpty(x.Id))
                        return gameInfo.DefaultRating;
                    else
                        return new Rating(x.SkillExcludingAnonymous.Mean, x.SkillExcludingAnonymous.StandardDeviation);
                };

                var players = playerRatings
                    .Where(_ => _.AccountId != ANONYMOUS_ACCOUNT_ID)
                    .GroupBy(_ => _.Team)
                    .OrderBy(_ => _.Key)
                    .Select(_ => _.Aggregate(new Team<int>(), (t, i) => t.AddPlayer(i.PlayerSlot, converter(i))))
                    .ToArray();

                if(players.Length != 2)
                    return;

                var teams = Teams.Concat(players);
                var quality = TrueSkillCalculator.CalculateMatchQuality(gameInfo, teams);
                var ratings = TrueSkillCalculator.CalculateNewRatings(gameInfo, teams, teamStandings);
                var collection = playerRatings
                    .Join(ratings, _ => _.PlayerSlot, _ => _.Key, (lhs, rhs) => new { PlayerRating = lhs, Rating = rhs })
                    .ToList();

                foreach (var item in collection)
                {
                    item.PlayerRating.SkillExcludingAnonymous.Quality = quality;
                    item.PlayerRating.SkillExcludingAnonymous.Mean = item.Rating.Value.Mean;
                    item.PlayerRating.SkillExcludingAnonymous.StandardDeviation = item.Rating.Value.StandardDeviation;
                    item.PlayerRating.SkillExcludingAnonymous.Rating = item.Rating.Value.ConservativeRating;
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }
        }

        private void UpdateRecords(List<PlayerRating> playerRatings, List<PlayerRecord> playerRecords)
        {
            var collection = playerRecords
                .Join(playerRatings, _ => _.PlayerSlot, _ => _.PlayerSlot, (lhs, rhs) => new { Player = lhs, Rating = rhs })
                .ToList();

            foreach (var item in collection)
            {
                if(item.Player.Anonymous)
                {
                    item.Player.SkillExcludingAnonymous = null;
                    item.Player.SkillIncludingAnonymous = null;
                }
                else
                {
                    item.Player.SkillExcludingAnonymous = item.Rating.SkillExcludingAnonymous;
                    item.Player.SkillIncludingAnonymous = item.Rating.SkillIncludingAnonymous;
                }   
            }
        }

        private async Task CreateRecords(DocumentClient client, List<PlayerRecord> records)
        {
            foreach (var item in records)
            {
                await client.CreateDocumentAsync(this.PlayerRecordsCollection, item);
            }
        }

        private async Task CreateOrReplaceRatings(DocumentClient client, List<PlayerRating> ratings)
        {
            foreach (var item in ratings)
            {
                if(item.AccountId == ANONYMOUS_ACCOUNT_ID)
                    continue;

                if(string.IsNullOrEmpty(item.Id))
                {
                     await client.CreateDocumentAsync(this.PlayerRatingsCollection, item);
                }
                else
                {
                    var uri = UriFactory.CreateDocumentUri("hgv-nullifier", "player-ratings", item.Id);
                    await client.ReplaceDocumentAsync(uri, item);
                }
            }
        }

        /*
        // Where (Geo)
        // -> World Map (Servers)

        // When (Temporal)
        // -> Calendar Heatmap (Daily/Hourly)

        // Leaderboards
        // -> Total Matches
        // -> Win Rate
        // -> Skill Rating

        // Search
        // -> When (< 6 months ago) [Fixed]
        // -> Where [Server/Region] => Cluster
        // -> Hero [Name] [Str/Agi/Int] [Melee/Range] => Hero Id
        // -> Abilities [Name] [Skill, Ultimate, Talent] => Ability Id
        // -> Items [Shop,Drop] => Item Id

        // Live Draft [Capture] [Search w/ Filters]
        // -> Pool / Hero
        // -> Hero Picks
        // -> Combo Picks
        // -> Deny Picks
        // -> Personal Picks [Signed In]

        // Profile [Search w/ Account]
        // -> Record
        // -> History
        // -> Combatants
        */
    }
}
