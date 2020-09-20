using HGV.Nullifier.Collection.Models;
using HGV.Nullifier.Collection.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HGV.Nullifier.Collection.Functions
{
    public class BackwardCollectorFunction
    {
        private readonly ICollectionService colletionService;

        public BackwardCollectorFunction(ICollectionService colletionService)
        {
            this.colletionService = colletionService;
            this.colletionService.Config(Direction.Backward);
        }

        [FunctionName("BackwardCollector")]
        public async Task RunAsync(
            [TimerTrigger("0 */10 * * * *")]TimerInfo myTimer,
            [CosmosDB(
                databaseName: "HGV-Nullifier",
                collectionName: "history",
                ConnectionStringSetting = "CosmosDBConnection",
                SqlQuery = "SELECT c.match_seq_num,c.start_time,c.duration FROM c ORDER BY c.match_seq_num OFFSET 0 LIMIT 1")] 
            IEnumerable<MatchSummary> existing,
            [CosmosDB(
                databaseName: "HGV-Nullifier",
                collectionName: "history",
                ConnectionStringSetting = "CosmosDBConnection")]
            IAsyncCollector<MatchHistory> history,
            [CosmosDB(
                databaseName: "HGV-Nullifier",
                collectionName: "history-diagnostics",
                ConnectionStringSetting = "CosmosDBConnection")]
            IAsyncCollector<CollectorDiagnostics> collector,
            ILogger log)
        {
            if (myTimer.IsPastDue)
                return;

            await this.colletionService.Worker(existing, history, collector, log);
        }
    }
}
