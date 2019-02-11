namespace HGV.Nullifier.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CleanUpAfterFristRun : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AbilityDraftStats", "team", c => c.Int(nullable: false));
            AddColumn("dbo.AbilityDraftStats", "draft_order", c => c.Int(nullable: false));
            DropColumn("dbo.AbilityDraftStats", "player_index");
            DropTable("dbo.CollectionStats");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.CollectionStats",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        last_match_found = c.Long(nullable: false),
                        last_match_processed = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.id);
            
            AddColumn("dbo.AbilityDraftStats", "player_index", c => c.Int(nullable: false));
            DropColumn("dbo.AbilityDraftStats", "draft_order");
            DropColumn("dbo.AbilityDraftStats", "team");
        }
    }
}
