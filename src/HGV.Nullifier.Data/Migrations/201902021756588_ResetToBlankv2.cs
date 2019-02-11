namespace HGV.Nullifier.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ResetToBlankv2 : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.AbilityComboStats", "IX_AbilityCombo");
            DropIndex("dbo.AbilityHeroStats", "IX_AbilityAndHero");
            DropIndex("dbo.AbilityStats", new[] { "ability" });
            DropIndex("dbo.GameModeStats", new[] { "mode" });
            DropIndex("dbo.HeroStats", new[] { "hero" });
            DropIndex("dbo.TalentHeroStats", "IX_TalentAndHero");
            DropTable("dbo.AbilityComboStats");
            DropTable("dbo.AbilityHeroStats");
            DropTable("dbo.AbilityStats");
            DropTable("dbo.GameModeStats");
            DropTable("dbo.HeroStats");
            DropTable("dbo.TalentHeroStats");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.TalentHeroStats",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        talent = c.Int(nullable: false),
                        hero = c.Int(nullable: false),
                        names = c.String(),
                        picks = c.Int(nullable: false),
                        wins = c.Int(nullable: false),
                        win_rate = c.Single(nullable: false),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.HeroStats",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        hero = c.Int(nullable: false),
                        name = c.String(),
                        picks = c.Int(nullable: false),
                        wins = c.Int(nullable: false),
                        win_rate = c.Single(nullable: false),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.GameModeStats",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        mode = c.Int(nullable: false),
                        name = c.String(),
                        picks = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.AbilityStats",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        ability = c.Int(nullable: false),
                        name = c.String(),
                        picks = c.Int(nullable: false),
                        wins = c.Int(nullable: false),
                        win_rate = c.Single(nullable: false),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.AbilityHeroStats",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        ability = c.Int(nullable: false),
                        hero = c.Int(nullable: false),
                        names = c.String(),
                        is_same_hero = c.Boolean(nullable: false),
                        picks = c.Int(nullable: false),
                        wins = c.Int(nullable: false),
                        win_rate = c.Single(nullable: false),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.AbilityComboStats",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        ability1 = c.Int(nullable: false),
                        ability2 = c.Int(nullable: false),
                        names = c.String(),
                        is_same_hero = c.Boolean(nullable: false),
                        picks = c.Int(nullable: false),
                        wins = c.Int(nullable: false),
                        win_rate = c.Single(nullable: false),
                    })
                .PrimaryKey(t => t.id);
            
            CreateIndex("dbo.TalentHeroStats", new[] { "talent", "hero" }, name: "IX_TalentAndHero");
            CreateIndex("dbo.HeroStats", "hero", unique: true);
            CreateIndex("dbo.GameModeStats", "mode", unique: true);
            CreateIndex("dbo.AbilityStats", "ability", unique: true);
            CreateIndex("dbo.AbilityHeroStats", new[] { "ability", "hero" }, name: "IX_AbilityAndHero");
            CreateIndex("dbo.AbilityComboStats", new[] { "ability1", "ability2" }, name: "IX_AbilityCombo");
        }
    }
}
