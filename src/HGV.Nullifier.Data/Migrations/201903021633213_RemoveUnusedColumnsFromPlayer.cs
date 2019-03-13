namespace HGV.Nullifier.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveUnusedColumnsFromPlayer : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.PlayerSummaries", "kills");
            DropColumn("dbo.PlayerSummaries", "deaths");
            DropColumn("dbo.PlayerSummaries", "assists");
            DropColumn("dbo.PlayerSummaries", "last_hits");
            DropColumn("dbo.PlayerSummaries", "denies");
            DropColumn("dbo.PlayerSummaries", "gold");
            DropColumn("dbo.PlayerSummaries", "level");
            DropColumn("dbo.PlayerSummaries", "gold_per_min");
            DropColumn("dbo.PlayerSummaries", "xp_per_min");
            DropColumn("dbo.PlayerSummaries", "gold_spent");
            DropColumn("dbo.PlayerSummaries", "hero_damage");
            DropColumn("dbo.PlayerSummaries", "tower_damage");
            DropColumn("dbo.PlayerSummaries", "hero_healing");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PlayerSummaries", "hero_healing", c => c.Int(nullable: false));
            AddColumn("dbo.PlayerSummaries", "tower_damage", c => c.Int(nullable: false));
            AddColumn("dbo.PlayerSummaries", "hero_damage", c => c.Int(nullable: false));
            AddColumn("dbo.PlayerSummaries", "gold_spent", c => c.Int(nullable: false));
            AddColumn("dbo.PlayerSummaries", "xp_per_min", c => c.Int(nullable: false));
            AddColumn("dbo.PlayerSummaries", "gold_per_min", c => c.Int(nullable: false));
            AddColumn("dbo.PlayerSummaries", "level", c => c.Int(nullable: false));
            AddColumn("dbo.PlayerSummaries", "gold", c => c.Int(nullable: false));
            AddColumn("dbo.PlayerSummaries", "denies", c => c.Int(nullable: false));
            AddColumn("dbo.PlayerSummaries", "last_hits", c => c.Int(nullable: false));
            AddColumn("dbo.PlayerSummaries", "assists", c => c.Int(nullable: false));
            AddColumn("dbo.PlayerSummaries", "deaths", c => c.Int(nullable: false));
            AddColumn("dbo.PlayerSummaries", "kills", c => c.Int(nullable: false));
        }
    }
}
