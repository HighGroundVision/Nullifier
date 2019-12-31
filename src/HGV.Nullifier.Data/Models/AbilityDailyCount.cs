using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HGV.Nullifier.Data.Models
{
    public class AbilityDailyCount
    {
        [Key]
        public int Id { get; set; }

        [Index]
        public int Day { get; set; }

        public string AbilityId { get; set; }
        public string AbilityName { get; set; }
        public bool IsSkill { get; set; }

        // Strength
        public int MostKills { get; set; }

        // Win Rate
        public int Wins { get; set; }
        public int Losses { get; set; }
        public double WinRate { get; set; }

        // Frist Pick
        public int DraftOrder { get; set; }

        
    }
}
