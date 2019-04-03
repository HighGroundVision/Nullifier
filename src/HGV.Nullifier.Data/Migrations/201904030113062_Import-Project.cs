namespace HGV.Nullifier.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ImportProject : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Abilities",
                c => new
                    {
                        id = c.Int(nullable: false),
                        name = c.String(),
                        key = c.String(),
                        hero_id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.Heroes",
                c => new
                    {
                        id = c.Int(nullable: false),
                        name = c.String(),
                        key = c.String(),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.Matches",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        match_id = c.Long(nullable: false),
                        match_number = c.Long(nullable: false),
                        league_id = c.Int(nullable: false),
                        duration = c.Double(nullable: false),
                        date = c.DateTime(nullable: false),
                        day_of_week = c.Int(nullable: false),
                        hour_of_day = c.Int(nullable: false),
                        cluster = c.Int(nullable: false),
                        region = c.Int(nullable: false),
                        victory_dire = c.Int(nullable: false),
                        victory_radiant = c.Int(nullable: false),
                        valid = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .Index(t => t.match_id)
                .Index(t => t.match_number);
            
            CreateTable(
                "dbo.Players",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        match_ref = c.Int(nullable: false),
                        victory = c.Int(nullable: false),
                        player_slot = c.Int(nullable: false),
                        draft_order = c.Int(nullable: false),
                        team = c.Int(nullable: false),
                        account_id = c.Long(nullable: false),
                        hero_id = c.Int(nullable: false),
                        kills = c.Int(nullable: false),
                        deaths = c.Int(nullable: false),
                        assists = c.Int(nullable: false),
                        last_hits = c.Int(nullable: false),
                        denies = c.Int(nullable: false),
                        gold = c.Int(nullable: false),
                        level = c.Int(nullable: false),
                        gold_per_min = c.Int(nullable: false),
                        xp_per_min = c.Int(nullable: false),
                        gold_spent = c.Int(nullable: false),
                        hero_damage = c.Int(nullable: false),
                        tower_damage = c.Int(nullable: false),
                        hero_healing = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .Index(t => t.match_ref)
                .Index(t => t.hero_id);
            
            CreateTable(
                "dbo.Regions",
                c => new
                    {
                        id = c.Int(nullable: false),
                        name = c.String(),
                        timezone = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.Skills",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        match_ref = c.Int(nullable: false),
                        player_ref = c.Int(nullable: false),
                        ability_id = c.Int(nullable: false),
                        is_skill = c.Int(nullable: false),
                        is_ulimate = c.Int(nullable: false),
                        is_taltent = c.Int(nullable: false),
                        is_hero_same = c.Int(nullable: false),
                        level = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .Index(t => t.match_ref)
                .Index(t => t.player_ref)
                .Index(t => t.ability_id);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.Skills", new[] { "ability_id" });
            DropIndex("dbo.Skills", new[] { "player_ref" });
            DropIndex("dbo.Skills", new[] { "match_ref" });
            DropIndex("dbo.Players", new[] { "hero_id" });
            DropIndex("dbo.Players", new[] { "match_ref" });
            DropIndex("dbo.Matches", new[] { "match_number" });
            DropIndex("dbo.Matches", new[] { "match_id" });
            DropTable("dbo.Skills");
            DropTable("dbo.Regions");
            DropTable("dbo.Players");
            DropTable("dbo.Matches");
            DropTable("dbo.Heroes");
            DropTable("dbo.Abilities");
        }
    }
}
