using Microsoft.Azure.Documents;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace HGV.Nullifier.Collection.Models.Stats
{
    public class PlayerRating
    {
        [JsonIgnore]
        public int PlayerSlot { get; set; } 

        [JsonIgnore]
        public int Team { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; } 

        [JsonProperty("account_id")]
        public long AccountId { get; set; } 

        [JsonProperty("timestamp")]
        public long LastTimestamp { get; set; }

        [JsonProperty("date")]
        public DateTime LastDate { get; set; }

        [JsonProperty("matches")]
        public int Matches { get; set; }

        [JsonProperty("wins")]
        public int Wins { get; set; }

        [JsonProperty("skill_including_anonymous")]
        public Skill SkillIncludingAnonymous { get; set; }

        [JsonProperty("skill_excluding_anonymous")]
        public Skill SkillExcludingAnonymous { get; set; }
    }

    public class Skill
    { 
        [JsonProperty("mean")]
        public double Mean { get; set; }

        [JsonProperty("std")]
        public double StandardDeviation { get; set; }

        [JsonProperty("rating")]
        public double Rating { get; set; }

        [JsonProperty("quality")]
        public double? Quality { get; set; } 
    }
}
