namespace HGV.Nullifier.Data
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using HGV.Nullifier.Data.Models;
    using System.Data.Entity.ModelConfiguration.Conventions;

    public partial class DataContext : DbContext
    {
        public DataContext() : base("name=DataContext") {}

        public virtual DbSet<RegionDailyCount> RegionDailyCounts { get; set; }        
        public virtual DbSet<HeroDailyCount> HeroDailyCounts { get; set; }        
        public virtual DbSet<PlayerDailyCount> PlayerDailyCounts { get; set; }
        public virtual DbSet<AbilityDailyCount> AbilityDailyCounts { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
        }
    }
}
