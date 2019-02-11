namespace HGV.Nullifier.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemovedName : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.GameModeStats", "name");
        }
        
        public override void Down()
        {
            AddColumn("dbo.GameModeStats", "name", c => c.String());
        }
    }
}
