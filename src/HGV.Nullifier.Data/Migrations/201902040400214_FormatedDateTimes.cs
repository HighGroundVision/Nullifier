namespace HGV.Nullifier.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FormatedDateTimes : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AbilityDraftStats", "day_of_week", c => c.Int(nullable: false));
            AddColumn("dbo.AbilityDraftStats", "date", c => c.DateTime(nullable: false));
            AlterColumn("dbo.AbilityDraftStats", "duration", c => c.Double(nullable: false));
            DropColumn("dbo.AbilityDraftStats", "start_time");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AbilityDraftStats", "start_time", c => c.Long(nullable: false));
            AlterColumn("dbo.AbilityDraftStats", "duration", c => c.Int(nullable: false));
            DropColumn("dbo.AbilityDraftStats", "date");
            DropColumn("dbo.AbilityDraftStats", "day_of_week");
        }
    }
}
