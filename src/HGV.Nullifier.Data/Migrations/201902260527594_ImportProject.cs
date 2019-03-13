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
                "dbo.MatchSummaries",
                c => new
                    {
                        id = c.Long(nullable: false),
                        match_number = c.Long(nullable: false),
                        duration = c.Double(nullable: false),
                        day_of_week = c.Int(nullable: false),
                        date = c.DateTime(nullable: false),
                        victory_dire = c.Int(nullable: false),
                        victory_radiant = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .Index(t => t.match_number);
            
            CreateTable(
                "dbo.PlayerSummaries",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        hero_id = c.Int(nullable: false),
                        match_id = c.Long(nullable: false),
                        match_result = c.Int(nullable: false),
                        player_slot = c.Int(nullable: false),
                        draft_order = c.Int(nullable: false),
                        team = c.Int(nullable: false),
                        account_id = c.Long(nullable: false),
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
                .Index(t => t.hero_id)
                .Index(t => t.match_id);
            
            CreateTable(
                "dbo.SkillSummaries",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        ability_id = c.Int(nullable: false),
                        hero_id = c.Int(nullable: false),
                        match_id = c.Long(nullable: false),
                        account_id = c.Long(nullable: false),
                        draft_order = c.Int(nullable: false),
                        match_result = c.Int(nullable: false),
                        is_skill = c.Int(nullable: false),
                        is_ulimate = c.Int(nullable: false),
                        is_taltent = c.Int(nullable: false),
                        is_self = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .Index(t => t.ability_id)
                .Index(t => t.hero_id)
                .Index(t => t.match_id);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.SkillSummaries", new[] { "match_id" });
            DropIndex("dbo.SkillSummaries", new[] { "hero_id" });
            DropIndex("dbo.SkillSummaries", new[] { "ability_id" });
            DropIndex("dbo.PlayerSummaries", new[] { "match_id" });
            DropIndex("dbo.PlayerSummaries", new[] { "hero_id" });
            DropIndex("dbo.MatchSummaries", new[] { "match_number" });
            DropTable("dbo.SkillSummaries");
            DropTable("dbo.PlayerSummaries");
            DropTable("dbo.MatchSummaries");
            DropTable("dbo.Heroes");
            DropTable("dbo.Abilities");
        }
    }
}
