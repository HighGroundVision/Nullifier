namespace HGV.Nullifier.Data
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using HGV.Nullifier.Data.Models;

    public partial class DataContext : DbContext
    {
        public DataContext() : base("name=DataContext") {}

        public virtual DbSet<MatchSummary> Matches { get; set; }
        public virtual DbSet<PlayerSummary> Players { get; set; }
        public virtual DbSet<SkillSummary> Skills { get; set; }

        public virtual DbSet<Hero> Heroes { get; set; }
        public virtual DbSet<Ability> Abilities { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
