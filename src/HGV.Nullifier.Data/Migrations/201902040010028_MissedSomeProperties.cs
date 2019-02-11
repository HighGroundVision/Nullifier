namespace HGV.Nullifier.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MissedSomeProperties : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CollectionStats", "last_match_found", c => c.Long(nullable: false));
            AddColumn("dbo.CollectionStats", "last_match_processed", c => c.Long(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.CollectionStats", "last_match_processed");
            DropColumn("dbo.CollectionStats", "last_match_found");
        }
    }
}
