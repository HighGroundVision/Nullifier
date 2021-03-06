﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HGV.Nullifier.Data.Models
{
    public class Player
    {
        [Key]
        public int id { get; set; }

        // Match
        [Index]
        public int match_ref { get; set; }
        public int victory { get; set; }

        // Player
        public int player_slot { get; set; }
        public int draft_order { get; set; }
        public int team { get; set; }
        public long account_id { get; set; }
        public string persona { get; set; }

        // Hero
        [Index]
        public int hero_id { get; set; }
        public int kills { get; set; }
        public int deaths { get; set; }
        public int assists { get; set; }
        public int last_hits { get; set; }
        public int denies { get; set; }
        public int gold { get; set; }
        public int level { get; set; }
        public int gold_per_min { get; set; }
        public int xp_per_min { get; set; }
        public int gold_spent { get; set; }
        public int hero_damage { get; set; }
        public int tower_damage { get; set; }
        public int hero_healing { get; set; }
    }
}
