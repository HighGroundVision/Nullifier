using AutoMapper;
using HGV.Nullifier.Collection.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HGV.Nullifier.Collection.Functions
{
    public class HistoryFeedFunction
    {
        private readonly IMapper mapper;

        public HistoryFeedFunction(IMapper mapper)
        {
            this.mapper = mapper;
        }


        [FunctionName("HistoryFeed")]
        public async Task ChangeFeed([CosmosDBTrigger(
            databaseName: "HGV-Nullifier",
            collectionName: "history",
            ConnectionStringSetting = "CosmosDBConnection",
            LeaseCollectionName = "history-leases")] JArray feed,
        [CosmosDB(
            databaseName: "HGV-Nullifier",
            collectionName: "matches",
            ConnectionStringSetting = "CosmosDBConnection")] IAsyncCollector<MatchDetails> matches,
        [CosmosDB(
            databaseName: "HGV-Nullifier",
            collectionName: "matches-diagnostics",
            ConnectionStringSetting = "CosmosDBConnection")] IAsyncCollector<MatchDiagnostics> collector,
        ILogger log, CancellationToken token)
        {
            var diagnostics = new MatchDiagnostics();

            var history = feed.EmptyIfNull().Select(_ => _.ToObject<MatchHistory>()).ToList();
            foreach (var item in history)
            {
                var match = this.mapper.Map<MatchDetails>(item);
                await matches.AddAsync(match);

                UpdateDiagnostics(diagnostics, match);
            }

            await LogDiagnostics(collector, diagnostics, log);
        }

        private static void UpdateDiagnostics(MatchDiagnostics diagnostics, MatchDetails match)
        {
            diagnostics.MatchesProcessed++;

            if (match.Valid.HasInsufficientDuration)
                diagnostics.InsufficientDuration++;

            if (match.Valid.HasAbandon)
                diagnostics.Abandons++;

            if (match.Valid.HasOmissions)
                diagnostics.Omissions++;

            if (match.Valid.HasAnonymous)
                diagnostics.Anonymous++;
        }

        private static async Task LogDiagnostics(IAsyncCollector<MatchDiagnostics> collector, MatchDiagnostics diagnostics, ILogger log)
        {
            log.LogInformation($"Processed: {diagnostics.MatchesProcessed}");
            
            if(diagnostics.InsufficientDuration > 0)
                log.LogWarning($"Short: {diagnostics.InsufficientDuration}");

            if(diagnostics.Abandons > 0)
                log.LogWarning($"Abandons: {diagnostics.Abandons}");

            if(diagnostics.Omissions > 0)
                log.LogWarning($"Omissions: {diagnostics.Omissions}");
            
            if(diagnostics.Anonymous > 0)
                log.LogWarning($"Anonymous: {diagnostics.Anonymous}");

            await collector.AddAsync(diagnostics);
        }
    }
}
