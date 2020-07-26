using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace HGV.Nullifier.Collection.Models.History
{
    public partial class Reponse
    {
        [JsonProperty("result")]
        public Result Result { get; set; }
    }

    public partial class Result
    {
        [JsonProperty("status")]
        public long Status { get; set; }

        [JsonProperty("matches")]
        public List<Match> Matches { get; set; }
    }
}
