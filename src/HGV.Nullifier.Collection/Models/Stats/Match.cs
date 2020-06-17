using System;
using System.Collections.Generic;
using System.Text;
using HGV.Nullifier.Collection.Models.History;
using Newtonsoft.Json;

namespace HGV.Nullifier.Collection.Models.Stats
{
    public class Match
    {
        // What
        [JsonProperty("match_id")]
        public long MatchId { get; set; }

        [JsonProperty("match_seq_num")]
        public long MatchSequenceNumber { get; set; }

        // When
        [JsonProperty("start_time", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? StartTime { get; set; }

        [JsonProperty("duration", NullValueHandling = NullValueHandling.Ignore)]
        public TimeSpan? Duration { get; set; }

        [JsonProperty("pre_game_duration", NullValueHandling = NullValueHandling.Ignore)]
        public TimeSpan? PreGameDuration { get; set; }

        [JsonProperty("first_blood_time", NullValueHandling = NullValueHandling.Ignore)]
        public TimeSpan? FirstBloodTime { get; set; }

        // Where
        [JsonProperty("cluster", NullValueHandling = NullValueHandling.Ignore)]
        public int? Cluster { get; set; }

        [JsonProperty("server", NullValueHandling = NullValueHandling.Ignore)]
        public int? Server { get; set; }

        [JsonProperty("region", NullValueHandling = NullValueHandling.Ignore)]
        public int? Region { get; set; }

        [JsonProperty("league_id", NullValueHandling = NullValueHandling.Ignore)]
        public long? LeagueId { get; set; }

        // Whom
        [JsonProperty("radiant_score", NullValueHandling = NullValueHandling.Ignore)]
        public int? RadiantScore { get; set; }

        [JsonProperty("radiant_objectives", NullValueHandling = NullValueHandling.Ignore)]
        public double? RadiantObjectives { get; set; }

        [JsonProperty("dire_score", NullValueHandling = NullValueHandling.Ignore)]
        public int? DireScore { get; set; }

        [JsonProperty("dire_objectives", NullValueHandling = NullValueHandling.Ignore)]
        public double? DireObjectives { get; set; }

        [JsonProperty("valid", NullValueHandling = NullValueHandling.Ignore)]
        public int? Valid { get; set; }
    }
}
