namespace HGV.Nullifier.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class TalentHeroStat
    {
        [Key]
        public int id { get; set; }

        [Index("IX_TalentAndHero", 1)]
        public int talent { get; set; }

        [Index("IX_TalentAndHero", 2)]
        public int hero { get; set; }

        public string names { get; set; }

        public int picks { get; set; }

        public int wins { get; set; }

        public float win_rate { get; set; }
    }
}
