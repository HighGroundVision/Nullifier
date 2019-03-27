﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HGV.Nullifier.Data.Models
{
    public class SkillSummary
    {
        [Key]
        public int id { get; set; }

        public long match_id { get; set; }
        public int league_id { get; set; }
        public int match_result { get; set; }

        public int hero_id { get; set; }
        public long account_id { get; set; }
        public int draft_order { get; set; }
        public int team { get; set; }

        public int ability_id { get; set; }
        public int is_skill { get; set; }
        public int is_ulimate { get; set; }
        public int is_taltent { get; set; }

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
