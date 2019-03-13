using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HGV.Nullifier.Common.Export
{
    public class Ability
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Key { get; set; }
        public string Image { get; set; }
        public int HeroId { get; set; }
        public bool HasUpgrade { get; set; }
        public double Picks { get; set; }
        public double PicksPercentage { get; set; }
        public int PicksDeviation { get; set; }
        public double Wins { get; set; }
        public double WinsPercentage { get; set; }
        public int WinsDeviation { get; set; }
        public double WinRate { get; set; }
    }
}
