using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace HGV.Nullifier.Collection.Models
{
    public class CollectorDiagnostics
    {
        [JsonProperty("key")]
        public string Key => Current.ToString();

        [JsonIgnore()]
        public long Timestamp { get; set; }

        [JsonProperty("played_on")]
        public DateTime Date => DateTimeOffset.FromUnixTimeSeconds(Timestamp).Date;

        [JsonProperty("processed_on")]
        private DateTime ProcessedOn = DateTime.UtcNow;

        [JsonIgnore()]
        public long Duration { get; set; }

        [JsonProperty("delta")]
        public TimeSpan Delta => ProcessedOn - DateTimeOffset.FromUnixTimeSeconds(Timestamp + Duration);

        [JsonProperty("start")]
        public long Start { get; set; }

        [JsonProperty("current")]
        public long Current { get; set; }

        [JsonProperty("matches_processed")]
        public long MatchesProcessed { get; set; }

        [JsonProperty("matches_collected")]
        public long MatchesCollected { get; set; }

        [JsonProperty("asleep")]
        public TimeSpan Sleeping { get; set; }

        [JsonProperty("errors")]
        public int Errors { get; set; }
    }
}
