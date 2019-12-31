using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HGV.Nullifier.Data.Models
{
    public class RegionDailyCount
    {
        [Key]
        public int Id { get; set; }

        public int RegionId { get; set; }

        [Index]
        public int Day { get; set; }

        [Index]
        public int Hour { get; set; }

        public int Matches { get; set; }
    }
}
