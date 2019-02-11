using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HGV.Nullifier.Data.Models
{
    public class SkillSummary
    {
        [Key]
        public int id { get; set; }

        [Index]
        public int ability_id { get; set; }

        public int match_result { get; set; }
        public int is_skill { get; set; }
        public int is_ulimate { get; set; }
        public int is_taltent { get; set; }

        public PlayerSummary player { get; set; }
        public MatchSummary match { get; set; }
    }
}
