using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace HGV.Nullifier.Collection.Functions
{
    public class HistoryFeedFunction
    {
        [FunctionName("HistoryFeedFunction")]
        public void ChangeFeed([CosmosDBTrigger(
            databaseName: "hgv-nullifier",
            collectionName: "history",
            ConnectionStringSetting = "CosmosDBConnection",
            LeaseCollectionName = "history-leases")]IReadOnlyList<Models.History.Match> collection, 
        [CosmosDB(
            databaseName: "hgv-nullifier",
            collectionName: "matches",
            ConnectionStringSetting = "CosmosDBConnection")]
        IAsyncCollector<Models.Stats.Match> matchesCollector,
        [CosmosDB(
            databaseName: "hgv-nullifier",
            collectionName: "players",
            ConnectionStringSetting = "CosmosDBConnection")]
        IAsyncCollector<Models.Stats.Player> playersCollector,
        [CosmosDB(
            databaseName: "hgv-nullifier",
            collectionName: "heroes",
            ConnectionStringSetting = "CosmosDBConnection")]
        IAsyncCollector<Models.Stats.Hero> heroesCollector,
        [CosmosDB(
            databaseName: "hgv-nullifier",
            collectionName: "abilities",
            ConnectionStringSetting = "CosmosDBConnection")]
        IAsyncCollector<Models.Stats.Ability> abilitiesCollector,
        ILogger log)
        {
            if (collection == null)
                return;

            if (collection.Count > 0)
                return;

            //log.LogInformation("Documents modified " + input.Count);
            //log.LogInformation("First document Id " + input[0].Id);
        }
    }
}
