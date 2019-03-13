using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HGV.Nullifier.Common.Export
{
    public class Hero
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Key { get; set; }
        public string Image { get; set; }
        public string AttributePrimary { get; set; }
        public string AttackCapabilities { get; set; }
        public double Picks { get; set; }
        public int PicksDeviation { get; set; }
        public double Wins { get; set; }
        public int WinsDeviation { get; set; }
        public double WinRate { get; set; }
    }
}
