using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace HGV.Nullifier.Collection.Models
{
    public partial class MatchPlayer
    {
        [JsonProperty("slot")]
        public int PlayerSlot { get; set; }

        [JsonProperty("team")]
        public int Team { get; set; }

        [JsonProperty("order")]
        public int DraftOrder { get; set; }

        [JsonProperty("standing")]
        public int Standing { get; set; }

        [JsonProperty("account_id")]
        public long AccountId { get; set; }

        [JsonProperty("anonymous")]
        public bool Anonymous { get; set; }

        [JsonProperty("hero_id")]
        public int HeroId { get; set; }

        [JsonProperty("level")]
        public int Level { get; set; }

        [JsonProperty("kills")]
        public int Kills { get; set; }

        [JsonProperty("deaths")]
        public int Deaths { get; set; }

        [JsonProperty("assists")]
        public int Assists { get; set; }

        [JsonProperty("victory")]
        public bool Victory { get; set; }

        [JsonProperty("score")]
        public int Score { get; set; }

        [JsonProperty("objectives_lost")]
        public double ObjectivesLost { get; set; }

        [JsonProperty("objectives_destroyed")]
        public double ObjectivesDestroyed { get; set; }

        [JsonProperty("abandoned")]
        public bool Abandoned { get; set; }

        [JsonProperty("abilities")]
        public List<int> Abilities { get; set; } = new List<int>();

        [JsonProperty("items")]
        public List<int> Items { get; set; } = new List<int>();
    }

    public partial class MatchGruad
    {
        [JsonProperty("has_insufficient_duration")]
        public bool HasInsufficientDuration { get; set; }

        [JsonProperty("has_abandon")]
        public bool HasAbandon { get; set; }

        [JsonProperty("has_omissions")]
        public bool HasOmissions { get; set; }

        [JsonProperty("has_anonymous")]
        public bool HasAnonymous { get; set; }
    }

    public partial class MatchDetails
    {
        [JsonProperty("id")]
        public string Id  => MatchId.ToString();

        [JsonProperty("key")]
        public string Key => Cluster.ToString();

        [JsonProperty("match_id")]
        public long MatchId { get; set; }

        [JsonProperty("match_seq_num")]
        public long MatchSeqNum { get; set; }

        [JsonProperty("league_id")]
        public long LeagueId { get; set; }

        [JsonProperty("cluster")]
        public int Cluster { get; set; }

        [JsonProperty("region")]
        public int Region { get; set; }

        [JsonProperty("area")]
        public int Area { get; set; }

        [JsonProperty("start")]
        public DateTime Start { get; set; }

        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }

        [JsonProperty("day")]
        public int Day { get; set; }

        [JsonProperty("hour")]
        public int Hour { get; set; }

        [JsonProperty("duration")]
        public TimeSpan Duration { get; set; }

        [JsonProperty("length")]
        public long Length { get; set; }

        [JsonProperty("end")]
        public DateTime End => Start + Duration;

        [JsonProperty("players")]
        public List<MatchPlayer> Players { get; set; } = new List<MatchPlayer> ();

        [JsonProperty("valid")]
        public MatchGruad Valid { get; set; } = new MatchGruad();
    }
}
