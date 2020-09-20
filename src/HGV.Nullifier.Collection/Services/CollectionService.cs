using HGV.Nullifier.Collection.Models;
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

namespace HGV.Nullifier.Collection.Services
{
    public enum Direction
    {
        Forward,
        Backward,
    }

    public interface ICollectionService
    {
        void Config(Direction direction);

        Task Worker(
            IEnumerable<MatchSummary> existing, 
            IAsyncCollector<MatchHistory> history, 
            IAsyncCollector<CollectorDiagnostics> collector, 
            ILogger log);
    }

    // Issues
    // Duel Direction
    // <--s-->
    // [-------------------------------]
    // max(match_seq_num) | min(match_seq_num)
        
    // Forward Collection lags needing to jump ahead [creating a gap]
    // Need to fill those gaps [from the jump]
    // <---](BatchA)[-------------------](BatchB)[-------------](BatchC)[------------------------>
    // max(match_seq_num) | batch [s:e:c] | batch [s:e:c] | min(match_seq_num)

    //TOOD: Fuck all this stuff above it will fail at some point...
    // build a system based on batches with 3 primary handlers [forward / jump / backward]
    // Remove all this stuff and start over based on that...

    public class CollectionService : ICollectionService
    {
        private readonly JsonSerializer serializer;
        private readonly HttpClient client;
        private readonly TimeSpan timeStep = TimeSpan.FromSeconds(1);
        private Direction? direction = null;

        public CollectionService(IHttpClientFactory factory)
        {
            this.serializer = new JsonSerializer();
            this.client = factory.CreateClient("match_history");
        }

        public void Config(Direction direction)
        {
           this.direction = direction;
        }

        public async Task Worker(
            IEnumerable<MatchSummary> existing, 
            IAsyncCollector<MatchHistory> history, 
            IAsyncCollector<CollectorDiagnostics> collector, 
            ILogger log)
        {
            var diagnostics = CreateDiagnostics(existing);

            var cts = new CancellationTokenSource();
            var timer = Task.Delay(TimeSpan.FromMinutes(9), cts.Token);
            var worker = DoWork(diagnostics, history, log, cts.Token);
            var winner = await Task.WhenAny(timer, worker);
            if (winner == timer)
                cts.Cancel();

            await LogDiagnostics(collector, diagnostics, log);
        }

        private CollectorDiagnostics CreateDiagnostics(IEnumerable<MatchSummary> existing)
        {
            if(existing.Count() != 1)
                throw new ArgumentOutOfRangeException(nameof(existing));

            var summary = existing.Single();
            var diagnostics = new CollectorDiagnostics
            {
                Timestamp = summary.StartTime.GetValueOrDefault(),
                Duration = summary.Duration.GetValueOrDefault()
            };

            if(this.direction == Direction.Forward)
                diagnostics.Start = summary.MatchSeqNum + 1;
            else if(this.direction == Direction.Backward)
                diagnostics.Start = summary.MatchSeqNum - 1000;
            else
                throw new ArgumentOutOfRangeException(nameof(direction));

            diagnostics.Current =  diagnostics.Start;

            return diagnostics;
        }

        private async Task DoWork(CollectorDiagnostics data, 
            IAsyncCollector<MatchHistory> collector, 
            ILogger log, 
            CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var history = await GetMatchHistory(data, log, token);
                    var matches = history?.Result?.Matches.EmptyIfNull();
                    var collection = matches.Where(_ => _.GameMode == 18).ToList();

                    data.Current = matches.Max(_ => _.MatchSeqNum) + 1;
                    data.MatchesProcessed += matches.Count();
                    data.MatchesCollected += collection.Count;

                    await StoreMatches(collector, collection, log, token);

                    if(this.direction == Direction.Forward && data.MatchesProcessed < 100)
                        return;
                    else if(this.direction == Direction.Backward && data.Current > data.Start)
                        return;
                }
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }
        }

        private async Task<MatchReponse> GetMatchHistory(
            CollectorDiagnostics data, 
            ILogger log, 
            CancellationToken token)
        {
            await Task.Delay(timeStep, token);
            data.Sleeping += timeStep;

            var policy = Policy
                .Handle<HttpRequestException>()
                .WaitAndRetryForeverAsync(n => TimeSpan.FromSeconds(n * 30), (ex, n, time) => { 
                    data.Sleeping += time;
                    data.Errors++;
                });

            var history = await policy.ExecuteAsync<MatchReponse>(async t =>
            {
                var stream = await this.client.GetStreamAsync($"?start_at_match_seq_num={data.Current}");
                using var streamReader = new StreamReader(stream);
                using var jsonTextReader = new JsonTextReader(streamReader);
                return this.serializer.Deserialize<MatchReponse>(jsonTextReader);
            }, token);

            return history;
        }

        private static async Task StoreMatches(
            IAsyncCollector<MatchHistory> collector, 
            IEnumerable<MatchHistory> matches, 
            ILogger log, 
            CancellationToken token)
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

        private static async Task LogDiagnostics(
            IAsyncCollector<CollectorDiagnostics> collector, 
            CollectorDiagnostics diagnostics, 
            ILogger log)
        {
            log.LogWarning($"Delta: {diagnostics.Delta.Humanize(3)}");
            log.LogWarning($"Matches Processed: {diagnostics.MatchesProcessed}");
            log.LogWarning($"Matches Collected: {diagnostics.MatchesCollected}");
            log.LogWarning($"Errors: {diagnostics.Errors}");
            log.LogWarning($"Sleeping: {diagnostics.Sleeping.Humanize(3)}");

            await collector.AddAsync(diagnostics);
        }

    }
}
