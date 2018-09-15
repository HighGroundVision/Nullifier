namespace HGV.Nullifier.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ImportProject : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AbilityComboStats",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        ability1 = c.Int(nullable: false),
                        ability2 = c.Int(nullable: false),
                        picks = c.Int(nullable: false),
                        wins = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .Index(t => new { t.ability1, t.ability2 }, name: "IX_AbilityCombo");
            
            CreateTable(
                "dbo.AbilityHeroStats",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        ability = c.Int(nullable: false),
                        hero = c.Int(nullable: false),
                        picks = c.Int(nullable: false),
                        wins = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .Index(t => new { t.ability, t.hero }, name: "IX_AbilityAndHero");
            
            CreateTable(
                "dbo.AbilityStats",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        ability = c.Int(nullable: false),
                        picks = c.Int(nullable: false),
                        wins = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .Index(t => t.ability, unique: true);
            
            CreateTable(
                "dbo.DraftStats",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        key = c.String(maxLength: 16),
                        picks = c.Int(nullable: false),
                        wins = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .Index(t => t.key, unique: true);
            
            CreateTable(
                "dbo.GameModeStats",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        game_mode = c.Int(nullable: false),
                        picks = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .Index(t => t.game_mode, unique: true);
            
            CreateTable(
                "dbo.HeroStats",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        hero = c.Int(nullable: false),
                        picks = c.Int(nullable: false),
                        wins = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .Index(t => t.hero, unique: true);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.HeroStats", new[] { "hero" });
            DropIndex("dbo.GameModeStats", new[] { "game_mode" });
            DropIndex("dbo.DraftStats", new[] { "key" });
            DropIndex("dbo.AbilityStats", new[] { "ability" });
            DropIndex("dbo.AbilityHeroStats", "IX_AbilityAndHero");
            DropIndex("dbo.AbilityComboStats", "IX_AbilityCombo");
            DropTable("dbo.HeroStats");
            DropTable("dbo.GameModeStats");
            DropTable("dbo.DraftStats");
            DropTable("dbo.AbilityStats");
            DropTable("dbo.AbilityHeroStats");
            DropTable("dbo.AbilityComboStats");
        }
    }
}
