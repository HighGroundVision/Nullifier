using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace HGV.Nullifier.Collection.Models
{
    public partial class AccountReponse
    {
        [JsonProperty("result")]
        public AccountResult Result { get; set; }
    }
    
    public partial class AccountResult
    {
        [JsonProperty("status")]
        public long Status { get; set; }

        [JsonProperty("num_results")]
        public long? Results { get; set; }

        [JsonProperty("total_results")]
        public long? Total { get; set; }

        [JsonProperty("results_remaining")]
        public long? Remaining { get; set; }

        [JsonProperty("matches")]
        public List<MatchSummary> Matches { get; set; }
    }
}
