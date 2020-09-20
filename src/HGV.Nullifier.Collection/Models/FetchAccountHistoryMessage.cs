using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace HGV.Nullifier.Collection.Models
{
    public class FetchAccountHistoryMessage
    {
        [JsonProperty("account_id")]
        public long AccountId { get; set; }

        [JsonProperty("steam_id")]
        public long SteamId => AccountId + 76561197960265728L;
    }
}
