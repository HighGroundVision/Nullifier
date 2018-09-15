namespace HGV.Nullifier.Data
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class DataContext : DbContext
    {
        public DataContext()
            : base("name=DataContext")
        {
        }

        public virtual DbSet<DraftStat> DraftStat { get; set; }
        public virtual DbSet<AbilityHeroStat> AbilityHeroStats { get; set; }
        public virtual DbSet<AbilityStat> AbilityStats { get; set; }
        public virtual DbSet<AbilityComboStat> AbilityComboStats { get; set; }
        public virtual DbSet<GameModeStat> GameModeStats { get; set; }
        public virtual DbSet<HeroStat> HeroStats { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
