using Microsoft.Azure.Documents;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace HGV.Nullifier.Collection.Models.Stats
{
    public class PlayerRecord
    {
        // What
        [JsonProperty("match_id")]
        public long MatchId { get; set; }

        // When
        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }

        [JsonProperty("date")]
        public DateTime Date { get; set; }

        [JsonProperty("length")]
        public long Length { get; set; }

        [JsonProperty("duration")]
        public TimeSpan Duration { get; set; }

        // Where
        [JsonProperty("cluster")]
        public int Cluster { get; set; } // <= PK

         // Team
        [JsonProperty("team")]
        public int Team { get; set; }

        [JsonProperty("team_score")]
        public int TeamScore { get; set; }

        [JsonProperty("objectives_lost")]
        public double ObjectivesLost { get; set; }

        [JsonProperty("objectives_taken")]
        public double ObjectivesTaken { get; set; }

        [JsonProperty("victory")]
        public bool Victory { get; set; }

        // Account
        [JsonProperty("account_id")]
        public long AccountId { get; set; } // <= PK

        [JsonProperty("anonymous")]
        public bool Anonymous { get; set; }

        // Ranking
        [JsonProperty("skill_including_anonymous", NullValueHandling = NullValueHandling.Ignore)]
        public Skill SkillIncludingAnonymous { get; set; }

        [JsonProperty("skill_excluding_anonymous", NullValueHandling = NullValueHandling.Ignore)]
        public Skill SkillExcludingAnonymous { get; set; }

        // Hero
        [JsonProperty("draft_order")]
        public int DraftOrder { get; set; }

        [JsonProperty("player_slot")]
        public int PlayerSlot { get; set; }

        [JsonProperty("hero_id")]
        public int HeroId { get; set; }

        // Stats
        [JsonProperty("kills")]
        public int Kills { get; set; }

        [JsonProperty("deaths")]
        public int Deaths { get; set; }

        [JsonProperty("assists")]
        public int Assists { get; set; }

        [JsonProperty("last_hits")]
        public int LastHits { get; set; }

        [JsonProperty("denies")]
        public int Denies { get; set; }
        
        // Items
        [JsonProperty("items", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<int> Items { get; set; } = new List<int>();

        // Abilities & Talent
        [JsonProperty("skills", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<int> Skills { get; set; } = new List<int>();
    }
}
