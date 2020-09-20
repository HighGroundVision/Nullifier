using HGV.Nullifier.Collection.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace HGV.Nullifier.Collection.Functions
{
    public class SeedHistoryFunction
    {
        private readonly JsonSerializer serializer;
        private readonly string apiKey;
        private readonly HttpClient client;
        private readonly TimeSpan timeStep = TimeSpan.FromSeconds(3);

        public SeedHistoryFunction(IHttpClientFactory factory)
        {
            this.serializer = new JsonSerializer();
            this.apiKey = Environment.GetEnvironmentVariable("SteamApiKey");

            client = factory.CreateClient();
            client.BaseAddress = new Uri("http://api.steampowered.com/IDOTA2Match_570/GetMatchHistoryBySequenceNum/v0001/");
        }

        [Disable]
        [FunctionName("SeedHistory")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "seed/{id}")] HttpRequest req,
            long id,
            [CosmosDB(
                databaseName: "HGV-Nullifier",
                collectionName: "history",
                ConnectionStringSetting = "CosmosDBConnection")]
            IAsyncCollector<MatchHistory> collector,
            ILogger log)
        {
            var stream = await this.client.GetStreamAsync($"?key={apiKey}&start_at_match_seq_num={id}&matches_requested=1");
            using var sr = new StreamReader(stream);
            using var jsonTextReader = new JsonTextReader(sr);
            var reponse = this.serializer.Deserialize<MatchReponse>(jsonTextReader);
            var match = reponse.Result.Matches.Single();
            await collector.AddAsync(match);

            return new OkResult();
        }
    }
}
