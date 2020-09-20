using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace HGV.Nullifier.Collection.Models
{
    public class MatchDiagnostics
    {
        [JsonProperty("key")]
        public string Key => Guid.NewGuid().ToString();

        [JsonProperty("processed")]
        public int MatchesProcessed { get; set; }

        [JsonProperty("insufficient_duration")]
        public int InsufficientDuration { get; set; }

        [JsonProperty("abandons")]
        public int Abandons { get; set; }

        [JsonProperty("omissions")]
        public int Omissions { get; set; }

        [JsonProperty("anonymous")]
        public int Anonymous { get; set; }
    }
}
