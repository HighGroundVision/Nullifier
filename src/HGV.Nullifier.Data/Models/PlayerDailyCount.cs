using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HGV.Nullifier.Data.Models
{
    public class PlayerDailyCount
    {
        [Key]
        public int Id { get; set; }

        public long AccountId { get; set; }
        public long SteamId { get; set; }
        public string Persona { get; set; }

        [Index]
        public int Day { get; set; }

        public int Wins { get; set; }
        public int Losses { get; set; }
        public double WinRate { get; set; }
    }
}
