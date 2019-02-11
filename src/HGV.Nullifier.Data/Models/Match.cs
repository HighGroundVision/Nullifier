using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HGV.Nullifier.Data.Models
{
    public class MatchSummary
    {
        [Key]
        public int id { get; set; }

        [Index]
        public long match_number { get; set; }
        public long match_id { get; set; }
        public double duration { get; set; }
        public int day_of_week { get; set; }
        public DateTime date { get; set; }

        public ICollection<PlayerSummary> players { get; set; }
        public ICollection<SkillSummary> skills { get; set; }
    }
}
