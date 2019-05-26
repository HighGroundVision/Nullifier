namespace HGV.Nullifier.Data.Migrations
{
    using HGV.Nullifier.Data.Models;
    using System;
    using System.Collections.Generic;
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
            var heroes = client.GetHeroes()
                .Select(_ => new Hero()
                {
                    id = _.Id,
                    key = _.Key,
                    name = _.Name
                })
                .ToArray();

            var abilities = client.GetSkills()
                .Where(_ => _.Id != HGV.Basilius.Ability.GENERIC)
                .Select(_ => new Ability()
                {
                    id = _.Id,
                    key = _.Key,
                    name = _.Name,
                    hero_id = _.HeroId
                }).ToArray();

            var timezones = new Dictionary<int, int>()
            {
                { 0, 0 },   // UNKNOWN
                { 1, -5 },  // US WEST
                { 2, -8 },  // US EAST
                { 3, +2 },  // EUROPE
                { 5, +8 },  // SINGAPORE
                { 6, +4 },  // DUBAI
                { 7, +10 }, // AUSTRALIA
                { 8, +2 },  // STOCKHOLM
                { 9, +2 },  // AUSTRIA
                { 10, -3 }, // BRAZIL
                { 11, +2 }, // SOUTHAFRICA
                { 12, +8 }, // PW TELECOM SHANGHAI
                { 13, +8 }, // PW UNICOM
                { 14, -3 }, // CHILE
                { 15, -5 }, // PERU
                { 16, +5 }, // INDIA
                { 17, +8 }, // PW TELECOM GUANGDONG
                { 18, +8 }, // PW TELECOM ZHEJIANG
                { 19, +9 }, // JAPAN
                { 20, +8 }, // PW TELECOM WUHAN
                { 25, +8 }, // PW UNICOM TIANJIN
            };
            var regions = client.GetRegions()
                .Select(_ => new {  id = _.Key, name = _.Value } )
                .Join(timezones, _ => _.id, _ => _.Key, (lhs, rhs) => new Region()
                {
                    id = lhs.id,
                    name = lhs.name,
                    timezone = rhs.Value
                }).ToArray();

            context.Heroes.AddOrUpdate(_ => _.id, heroes);
            context.Abilities.AddOrUpdate(_ => _.id, abilities);
            context.Regions.AddOrUpdate(_ => _.id, regions);
        }
    }
}
