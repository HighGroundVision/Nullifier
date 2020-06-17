using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace HGV.Nullifier.Collection.Models.Stats
{
    public partial class Hero
    {
        [JsonProperty("match_id")] 
        public long MatchId { get; set; }

        [JsonProperty("match_seq_num")] 
        public long MatchSequenceNumber { get; set; }

        [JsonProperty("start_time", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? StartTime { get; set; }

        [JsonProperty("duration", NullValueHandling = NullValueHandling.Ignore)]
        public TimeSpan? Duration { get; set; }

        [JsonProperty("pre_game_duration", NullValueHandling = NullValueHandling.Ignore)]
        public TimeSpan? PreGameDuration { get; set; }

        [JsonProperty("first_blood_time", NullValueHandling = NullValueHandling.Ignore)]
        public TimeSpan? FirstBloodTime { get; set; }

        [JsonProperty("cluster", NullValueHandling = NullValueHandling.Ignore)]
        public int? Cluster { get; set; }

        [JsonProperty("server", NullValueHandling = NullValueHandling.Ignore)]
        public int? Server { get; set; }

        [JsonProperty("region", NullValueHandling = NullValueHandling.Ignore)]
        public int? Region { get; set; }

        [JsonProperty("league_id", NullValueHandling = NullValueHandling.Ignore)]
        public long? LeagueId { get; set; }

        [JsonProperty("team_score", NullValueHandling = NullValueHandling.Ignore)]
        public int? TeamScore { get; set; }

        [JsonProperty("team_score_delta", NullValueHandling = NullValueHandling.Ignore)]
        public int? TeamScoreDelta { get; set; }

        [JsonProperty("objectives_lost", NullValueHandling = NullValueHandling.Ignore)]
        public double? ObjectivesLost { get; set; }

        [JsonProperty("objectives_taken", NullValueHandling = NullValueHandling.Ignore)]
        public double? ObjectivesTaken { get; set; }

        [JsonProperty("match_valid", NullValueHandling = NullValueHandling.Ignore)]
        public int? MatchValid { get; set; }

        [JsonProperty("victory", NullValueHandling = NullValueHandling.Ignore)]
        public int? Victory { get; set; }

        [JsonProperty("player_slot", NullValueHandling = NullValueHandling.Ignore)]
        public long? PlayerSlot { get; set; }

        [JsonProperty("player_order", NullValueHandling = NullValueHandling.Ignore)]
        public int? PlayerOrder { get; set; }

        [JsonProperty("account_id", NullValueHandling = NullValueHandling.Ignore)]
        public long? AccountId { get; set; }

        [JsonProperty("hero_id", NullValueHandling = NullValueHandling.Ignore)]
        public long? HeroId { get; set; }

        [JsonProperty("leaver_status", NullValueHandling = NullValueHandling.Ignore)]
        public int? LeaverStatus { get; set; }

        [JsonProperty("kills", NullValueHandling = NullValueHandling.Ignore)]
        public long? Kills { get; set; }

        [JsonProperty("deaths", NullValueHandling = NullValueHandling.Ignore)]
        public long? Deaths { get; set; }

        [JsonProperty("assists", NullValueHandling = NullValueHandling.Ignore)]
        public long? Assists { get; set; }

        [JsonProperty("kda", NullValueHandling = NullValueHandling.Ignore)]
        public double? KdaRatio { get; set; }

        [JsonProperty("level", NullValueHandling = NullValueHandling.Ignore)]
        public long? Level { get; set; }

        [JsonProperty("last_hits", NullValueHandling = NullValueHandling.Ignore)]
        public long? LastHits { get; set; }

        [JsonProperty("denies", NullValueHandling = NullValueHandling.Ignore)]
        public long? Denies { get; set; }

        [JsonProperty("gold_per_min", NullValueHandling = NullValueHandling.Ignore)]
        public long? GoldPerMin { get; set; }

        [JsonProperty("xp_per_min", NullValueHandling = NullValueHandling.Ignore)]
        public long? XpPerMin { get; set; }

        [JsonProperty("hero_damage", NullValueHandling = NullValueHandling.Ignore)]
        public long? HeroDamage { get; set; }

        [JsonProperty("tower_damage", NullValueHandling = NullValueHandling.Ignore)]
        public long? TowerDamage { get; set; }

        [JsonProperty("hero_healing", NullValueHandling = NullValueHandling.Ignore)]
        public long? HeroHealing { get; set; }

        [JsonProperty("gold", NullValueHandling = NullValueHandling.Ignore)]
        public long? Gold { get; set; }

        [JsonProperty("gold_spent", NullValueHandling = NullValueHandling.Ignore)]
        public long? GoldSpent { get; set; }

        [JsonProperty("scaled_hero_damage", NullValueHandling = NullValueHandling.Ignore)]
        public long? ScaledHeroDamage { get; set; }

        [JsonProperty("scaled_tower_damage", NullValueHandling = NullValueHandling.Ignore)]
        public long? ScaledTowerDamage { get; set; }

        [JsonProperty("scaled_hero_healing", NullValueHandling = NullValueHandling.Ignore)]
        public long? ScaledHeroHealing { get; set; }

        [JsonProperty("skills", NullValueHandling = NullValueHandling.Ignore)]
        public List<int> Skills { get; set; }

        [JsonProperty("items", NullValueHandling = NullValueHandling.Ignore)]
        public List<int> Items { get; set; }
    }
}
