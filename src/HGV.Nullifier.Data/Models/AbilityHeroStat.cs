namespace HGV.Nullifier.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class AbilityHeroStat
    {
        [Key]
        public int id { get; set; }

        [Index("IX_AbilityAndHero", 1)]
        public int ability { get; set; }

        [Index("IX_AbilityAndHero", 2)]
        public int hero { get; set; }

        public string names { get; set; }

        public bool is_same_hero { get; set; }

        public int picks { get; set; }

        public int wins { get; set; }

        [NotMapped]
        public float win_rate { get { return this.wins / (this.picks * 1.0f); } }
    }
}
