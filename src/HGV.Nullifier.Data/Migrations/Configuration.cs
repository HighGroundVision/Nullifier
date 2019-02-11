namespace HGV.Nullifier.Data.Migrations
{
    using HGV.Nullifier.Data.Models;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<HGV.Nullifier.Data.DataContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(HGV.Nullifier.Data.DataContext context)
        {
            //  This method will be called after migrating to the latest version.
            //  You can use the DbSet<T>.AddOrUpdate() helper extension method to avoid creating duplicate seed data.

            var client = new HGV.Basilius.MetaClient();
            var heroes = client.GetHeroes().Select(_ => new Hero() { id = _.Id, key = _.Key, name = _.Name }).ToArray();
            var abilities = client.GetSkills().Where(_ => _.Id != HGV.Basilius.Ability.GENERIC).Select(_ => new Ability() { id = _.Id, key = _.Key, name = _.Name, hero_id = _.HeroId }).ToArray();

            context.Heroes.AddOrUpdate(_ => _.id, heroes);
            context.Abilities.AddOrUpdate(_ => _.id, abilities);
        }
    }
}
