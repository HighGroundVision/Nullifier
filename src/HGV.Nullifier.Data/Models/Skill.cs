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

        [Index]
        public int hero_id { get; set; }

        [Index]
        public long match_id { get; set; }

        public long account_id { get; set; }
        public int draft_order { get; set; }

        public int match_result { get; set; }
        public int is_skill { get; set; }
        public int is_ulimate { get; set; }
        public int is_taltent { get; set; }
        public int is_self { get; set; }
    }
}
