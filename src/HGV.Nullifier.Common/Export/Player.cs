using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HGV.Nullifier.Common.Export
{
    public class Player
    {
        public long AccountId { get; set; }
        public long ProfileId { get; set; }

        public int Rank { get; set; }
        public double Matches { get; set; }
        public double Wins { get; set; }
        public double WinRate { get; set; }

        public string Avatar { get; set; }
        public string Name { get; set; }
        public string ProfileUrl { get; set; }
    }
}
