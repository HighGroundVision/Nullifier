namespace HGV.Nullifier.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedIndexToMatchData : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.AbilityDraftStats", "match_number");
        }
        
        public override void Down()
        {
            DropIndex("dbo.AbilityDraftStats", new[] { "match_number" });
        }
    }
}
