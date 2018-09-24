namespace HGV.Nullifier.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class DraftStat
    {
        [Key]
        public int id { get; set; }

        [Index(IsUnique = true)]
        [StringLength(16)]
        public string key { get; set; }

        public bool is_same_hero { get; set; }

        public string names { get; set; }

        public int picks { get; set; }

        public int wins { get; set; }

        public float win_rate { get; set; }
    }
}
