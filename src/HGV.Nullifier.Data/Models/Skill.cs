using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HGV.Nullifier.Data.Models
{
    public class Skill
    {
        [Key]
        public int id { get; set; }

        [Index]
        public int match_ref { get; set; }

        [Index]
        public int player_ref { get; set; }

        // Skill
        [Index]
        public int ability_id { get; set; }

        public int is_skill { get; set; }
        public int is_ulimate { get; set; }
        public int is_taltent { get; set; }
        public int is_hero_same { get; set; }

        public int level { get; set; }
    }
}
