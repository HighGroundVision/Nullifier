using Dynamitey.DynamicObjects;
using HGV.Nullifier.Collection.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace HGV.Nullifier.Collection.Functions
{
    public class AccountCollectorFunction
    { 
        private readonly JsonSerializer serializer;
        private readonly HttpClient client;

        public AccountCollectorFunction(IHttpClientFactory factory)
        {
            this.serializer = new JsonSerializer();
            this.client = factory.CreateClient("account_history");
        }

        [Disable]
        [FunctionName("AccountCollectorFunction")]
        public async Task Run([QueueTrigger("hgv-account-history")]FetchAccountHistoryMessage item, ILogger log)
        {
            // {account_id:13029812}
            
            var collection = new List<long>();
            var current = 0L;
            while(true)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));

                var matches = await GetHistory(item.AccountId, current);
                collection.AddRange(matches);
                var next = matches.Min();
                if(next == current)
                    break;

                current = next;
            }

            var history = collection.Distinct().ToList();
            log.LogInformation($"player: {item.AccountId} has {history.Count} matches");
        }

        private async Task<List<long>> GetHistory(long accountId, long current)
        {
            var policy = Policy
               .Handle<HttpRequestException>()
               .WaitAndRetryForeverAsync(n => TimeSpan.FromSeconds(n * 30));

            var reponse = await policy.ExecuteAsync<AccountReponse>(async () =>
            {
                var stream = await this.client.GetStreamAsync($"?account_id={accountId}&start_at_match_id={current}");
                using var streamReader = new StreamReader(stream);
                using var jsonTextReader = new JsonTextReader(streamReader);
                return this.serializer.Deserialize<AccountReponse>(jsonTextReader);
            });

            var matches = reponse?.Result?.Matches?.EmptyIfNull().Select(_ => _.MatchId).ToList();
            return matches;
        }
    }
}
