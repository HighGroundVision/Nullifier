namespace HGV.Nullifier.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class AbilityComboStat
    {
        [Key]
        public int id { get; set; }

        [Index("IX_AbilityCombo", 1)]
        public int ability1 { get; set; }

        [Index("IX_AbilityCombo", 2)]
        public int ability2 { get; set; }

        public string names { get; set; }

        public bool is_same_hero { get; set; }

        public int picks { get; set; }

        public int wins { get; set; }

        public float win_rate { get; set; }
    }
}
