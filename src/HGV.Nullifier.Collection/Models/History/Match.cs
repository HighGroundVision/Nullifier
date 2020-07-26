using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace HGV.Nullifier.Collection.Models.History
{
    public class MatchSummary
    {
        [JsonProperty("match_seq_num")]
        public long MatchSeqNum { get; set; }

        [JsonProperty("start_time")]
        public long? StartTime { get; set; }

        [JsonProperty("duration")]
        public long? Duration { get; set; }
    }

    public partial class Match
    {
        [JsonProperty("match_id")]
        public long MatchId { get; set; }

        [JsonProperty("match_seq_num")]
        public long MatchSeqNum { get; set; }

        [JsonProperty("radiant_win", NullValueHandling = NullValueHandling.Ignore)]
        public bool? RadiantWin { get; set; }

        [JsonProperty("duration", NullValueHandling = NullValueHandling.Ignore)]
        public long? Duration { get; set; }

        [JsonProperty("pre_game_duration", NullValueHandling = NullValueHandling.Ignore)]
        public long? PreGameDuration { get; set; }

        [JsonProperty("start_time", NullValueHandling = NullValueHandling.Ignore)]
        public long? StartTime { get; set; }

        [JsonProperty("tower_status_radiant", NullValueHandling = NullValueHandling.Ignore)]
        public long? TowerStatusRadiant { get; set; }

        [JsonProperty("tower_status_dire", NullValueHandling = NullValueHandling.Ignore)]
        public long? TowerStatusDire { get; set; }

        [JsonProperty("barracks_status_radiant", NullValueHandling = NullValueHandling.Ignore)]
        public long? BarracksStatusRadiant { get; set; }

        [JsonProperty("barracks_status_dire", NullValueHandling = NullValueHandling.Ignore)]
        public long? BarracksStatusDire { get; set; }

        [JsonProperty("cluster", NullValueHandling = NullValueHandling.Ignore)]
        public int? Cluster { get; set; }

        [JsonProperty("first_blood_time", NullValueHandling = NullValueHandling.Ignore)]
        public long? FirstBloodTime { get; set; }

        [JsonProperty("lobby_type", NullValueHandling = NullValueHandling.Ignore)]
        public long? LobbyType { get; set; }

        [JsonProperty("human_players", NullValueHandling = NullValueHandling.Ignore)]
        public long? HumanPlayers { get; set; }

        [JsonProperty("leagueid", NullValueHandling = NullValueHandling.Ignore)]
        public long? LeagueId { get; set; }

        [JsonProperty("positive_votes", NullValueHandling = NullValueHandling.Ignore)]
        public long? PositiveVotes { get; set; }

        [JsonProperty("negative_votes", NullValueHandling = NullValueHandling.Ignore)]
        public long? NegativeVotes { get; set; }

        [JsonProperty("game_mode", NullValueHandling = NullValueHandling.Ignore)]
        public long? GameMode { get; set; }

        [JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
        public long? Flags { get; set; }

        [JsonProperty("engine", NullValueHandling = NullValueHandling.Ignore)]
        public long? Engine { get; set; }

        [JsonProperty("radiant_score", NullValueHandling = NullValueHandling.Ignore)]
        public int? RadiantScore { get; set; }

        [JsonProperty("dire_score", NullValueHandling = NullValueHandling.Ignore)]
        public int? DireScore { get; set; }

        [JsonProperty("players")]
        public List<Player> Players { get; set; }
    }
}
