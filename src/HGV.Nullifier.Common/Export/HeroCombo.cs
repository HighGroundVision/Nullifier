using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HGV.Nullifier.Common.Export
{
    public class HeroCombo
    {
        public int AbilityId { get; set; }
        public string Name { get; set; }
        public string Key { get; set; }
        public string Image { get; set; }
        public bool HasUpgrade { get; set; }
        public double Picks { get; set; }
        public double Wins { get; set; }
        public double WinRate { get; set; }
        public double PicksRatio { get; set; }
        public double WinsRatio { get; set; }
        public List<string> Keywords { get; internal set; }
    }
}
