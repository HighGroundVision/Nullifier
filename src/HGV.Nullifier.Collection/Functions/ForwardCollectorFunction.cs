using HGV.Nullifier.Collection.Models.Diagnostics;
using HGV.Nullifier.Collection.Models.History;
using Humanizer;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HGV.Nullifier.Collection.Functions
{
    public class ForwardCollectorFunction
    {
        private readonly JsonSerializer serializer;
        private readonly string apiKey;
        private readonly HttpClient client;
        private readonly TimeSpan timeStep = TimeSpan.FromSeconds(3);

        public ForwardCollectorFunction(IHttpClientFactory factory)
        {
            this.serializer = new JsonSerializer();
            this.apiKey = Environment.GetEnvironmentVariable("SteamApiKey");

            client = factory.CreateClient();
            client.BaseAddress = new Uri("http://api.steampowered.com/IDOTA2Match_570/GetMatchHistoryBySequenceNum/v0001/");
        }

        [FunctionName("ForwardCollector")]
        public async Task Run(
            [TimerTrigger("0 */10 * * * *")]TimerInfo myTimer,
            [CosmosDB(
                databaseName: "hgv-nullifier",
                collectionName: "history",
                ConnectionStringSetting = "CosmosDBConnection",
                SqlQuery = "SELECT c.match_seq_num,c.start_time,c.duration FROM c ORDER BY c.match_seq_num DESC OFFSET 0 LIMIT 1")] 
            IEnumerable<MatchSummary> existing,
            [CosmosDB(
                databaseName: "hgv-nullifier",
                collectionName: "history",
                ConnectionStringSetting = "CosmosDBConnection")]
            IAsyncCollector<Match> collector,
            ILogger log)
        {
            if (myTimer.IsPastDue)
                return;

            var start = existing.Single();

            var data = new ForwardCollectorDiagnostics
            {
                Current = start.MatchSeqNum+1,
                StartTime = start.StartTime.GetValueOrDefault(),
                Duration = start.Duration.GetValueOrDefault()
            };

            var cts = new CancellationTokenSource();
            var timer = Task.Delay(TimeSpan.FromMinutes(9), cts.Token);
            var worker = DoWork(data, collector, log, cts.Token);
            var winner = await Task.WhenAny(timer, worker);
            if (winner == timer)
                cts.Cancel();

            log.LogInformation($"Delta: {data.Delta.Humanize(3)}");
            log.LogInformation($"Sleeping: {data.Sleeping.Humanize(3)}");
            log.LogInformation($"Matches Processed: {data.MatchesProcessed}");
            log.LogInformation($"Matches Collected: {data.MatchesCollected}");
        }

        private async Task DoWork(ForwardCollectorDiagnostics data, IAsyncCollector<Match> collector, ILogger log, CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var history = await GetMatchHistory(data, log, token);
                    var matches = history?.Result?.Matches;
                    if (matches == null || matches.Any() == false)
                    {
                        await Task.Delay(TimeSpan.FromMinutes(1), token);
                        continue;
                    }

                    data.Current = matches.Max(_ => _.MatchSeqNum) + 1;
                    data.MatchesProcessed += matches.Count;
                    var collection = matches.Where(_ => _.GameMode == 18).ToList();
                    data.MatchesCollected += collection.Count;
                    await StoreMatches(collector, collection, log, token);
                }
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }
        }

        private async Task<Reponse> GetMatchHistory(ForwardCollectorDiagnostics data, ILogger log, CancellationToken token)
        {

            await Task.Delay(timeStep, token);
            data.Sleeping += timeStep;

            var policy = Policy
                .Handle<HttpRequestException>()
                .WaitAndRetryForeverAsync(n => TimeSpan.FromSeconds(n * 30), (ex, n, time) => data.Sleeping += time);

            var history = await policy.ExecuteAsync<Reponse>(async t =>
            {
                var stream = await this.client.GetStreamAsync($"?key={apiKey}&start_at_match_seq_num={data.Current}");
                using var sr = new StreamReader(stream);
                using var jsonTextReader = new JsonTextReader(sr);
                return this.serializer.Deserialize<Reponse>(jsonTextReader);
            }, token);

            return history;
        }

        private static async Task StoreMatches(IAsyncCollector<Match> collector, IEnumerable<Match> matches, ILogger log, CancellationToken token)
        {
            foreach (var match in matches)
            {
                try
                {
                    await collector.AddAsync(match, token);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }
    }
}
