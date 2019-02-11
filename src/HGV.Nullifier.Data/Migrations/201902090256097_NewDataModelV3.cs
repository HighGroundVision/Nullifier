namespace HGV.Nullifier.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NewDataModelV3 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.GameModes",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        mode = c.Int(nullable: false),
                        count = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .Index(t => t.mode, unique: true);
            
            CreateTable(
                "dbo.MatchSummaries",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        match_number = c.Long(nullable: false),
                        match_id = c.Long(nullable: false),
                        duration = c.Double(nullable: false),
                        day_of_week = c.Int(nullable: false),
                        date = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .Index(t => t.match_number);
            
            CreateTable(
                "dbo.PlayerSummaries",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        hero_id = c.Int(nullable: false),
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
                        match_id = c.Int(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.MatchSummaries", t => t.match_id)
                .Index(t => t.hero_id)
                .Index(t => t.match_id);
            
            CreateTable(
                "dbo.SkillSummaries",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        ability_id = c.Int(nullable: false),
                        match_result = c.Int(nullable: false),
                        is_skill = c.Int(nullable: false),
                        is_ulimate = c.Int(nullable: false),
                        is_taltent = c.Int(nullable: false),
                        match_id = c.Int(),
                        player_id = c.Int(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.MatchSummaries", t => t.match_id)
                .ForeignKey("dbo.PlayerSummaries", t => t.player_id)
                .Index(t => t.ability_id)
                .Index(t => t.match_id)
                .Index(t => t.player_id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SkillSummaries", "player_id", "dbo.PlayerSummaries");
            DropForeignKey("dbo.SkillSummaries", "match_id", "dbo.MatchSummaries");
            DropForeignKey("dbo.PlayerSummaries", "match_id", "dbo.MatchSummaries");
            DropIndex("dbo.SkillSummaries", new[] { "player_id" });
            DropIndex("dbo.SkillSummaries", new[] { "match_id" });
            DropIndex("dbo.SkillSummaries", new[] { "ability_id" });
            DropIndex("dbo.PlayerSummaries", new[] { "match_id" });
            DropIndex("dbo.PlayerSummaries", new[] { "hero_id" });
            DropIndex("dbo.MatchSummaries", new[] { "match_number" });
            DropIndex("dbo.GameModes", new[] { "mode" });
            DropTable("dbo.SkillSummaries");
            DropTable("dbo.PlayerSummaries");
            DropTable("dbo.MatchSummaries");
            DropTable("dbo.GameModes");
        }
    }
}
