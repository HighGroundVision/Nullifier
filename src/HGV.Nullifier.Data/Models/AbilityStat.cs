namespace HGV.Nullifier.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class AbilityStat
    {
        [Key]
        public int id { get; set; }

        [Index(IsUnique = true)]
        public int ability { get; set; }

        public int picks { get; set; }

        public int wins { get; set; }

        [NotMapped]
        public float win_rate { get { return this.wins / (this.picks * 1.0f); } }
    }
}
