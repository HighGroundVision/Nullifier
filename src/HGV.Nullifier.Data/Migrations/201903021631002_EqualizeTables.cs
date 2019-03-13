namespace HGV.Nullifier.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EqualizeTables : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MatchSummaries", "league_id", c => c.Int(nullable: false));
            AddColumn("dbo.MatchSummaries", "valid", c => c.Int(nullable: false));
            AddColumn("dbo.PlayerSummaries", "league_id", c => c.Int(nullable: false));
            AddColumn("dbo.SkillSummaries", "league_id", c => c.Int(nullable: false));
            AddColumn("dbo.SkillSummaries", "team", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SkillSummaries", "team");
            DropColumn("dbo.SkillSummaries", "league_id");
            DropColumn("dbo.PlayerSummaries", "league_id");
            DropColumn("dbo.MatchSummaries", "valid");
            DropColumn("dbo.MatchSummaries", "league_id");
        }
    }
}
