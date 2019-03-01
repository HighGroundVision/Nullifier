using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HGV.Nullifier.Common.Export
{
    public class AbilityCombo
    {
        public int AbilityId { get; set; }
        public string Name { get; set; }
        public string Key { get; set; }
        public string Image { get; set; }
        public double Picks { get; set; }
        public double Wins { get; set; }
        public double WinRate { get; set; }
    }
}
