using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HGV.Nullifier.Collection.Models;
using HGV.Nullifier.Collection.Models.History;
using Humanizer;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;

namespace HGV.Nullifier.Collection.Functions
{
    public class ForwardCollectorFunction
    {
        private readonly JsonSerializer serializer;
        private readonly string apiKey;
        private readonly HttpClient client;

        public ForwardCollectorFunction(IHttpClientFactory factory)
        {
            this.serializer = new JsonSerializer();
            this.apiKey = Environment.GetEnvironmentVariable("SteamApiKey");

            client = factory.CreateClient();
            client.BaseAddress = new Uri("http://api.steampowered.com");
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

            var data = new DiagnosticData
            {
                Current = start.MatchSeqNum+1,
                StartTime = start.StartTime.GetValueOrDefault(),
                Duration = start.Duration.GetValueOrDefault()
            };

            log.LogWarning($"Delta: {data.Delta.Humanize(3)}");

            var cts = new CancellationTokenSource();
            var timer = Task.Delay(TimeSpan.FromMinutes(9), cts.Token);
            var worker = DoWork(data, collector, log, cts.Token);
            var winner = await Task.WhenAny(timer, worker);
            if (winner == timer)
                cts.Cancel();

            log.LogWarning($"In Error: {data.InError.Humanize(3)}");
            log.LogWarning($"Matches Processed: {data.MatchesProcessed}");
            log.LogWarning($"Matches Collected: {data.MatchesCollected}");
        }

        private async Task DoWork(DiagnosticData data, IAsyncCollector<Match> collector, ILogger log, CancellationToken token)
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

        private async Task<MatchHistory> GetMatchHistory(DiagnosticData data, ILogger log, CancellationToken token)
        {
            var policy = Policy
                .Handle<HttpRequestException>()
                .WaitAndRetryForeverAsync(
                    n => TimeSpan.FromSeconds(Math.Pow(2, n))
                    , (ex, n, time) =>
                    {
                        data.InError += time;
                        Debug.WriteLine($"Error with Steam API (Too Many Requests) Retrying in {time.Humanize(3)}");
                    });

            var history = await policy.ExecuteAsync<MatchHistory>(async t =>
            {
                var url =
                    $"/IDOTA2Match_570/GetMatchHistoryBySequenceNum/v0001/?key={apiKey}&start_at_match_seq_num={data.Current}";
                var stream = await this.client.GetStreamAsync(url);
                using var sr = new StreamReader(stream);
                using var jsonTextReader = new JsonTextReader(sr);
                return this.serializer.Deserialize<MatchHistory>(jsonTextReader);
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
