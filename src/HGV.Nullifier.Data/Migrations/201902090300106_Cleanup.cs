namespace HGV.Nullifier.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Cleanup : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.GameModes", new[] { "mode" });
            DropPrimaryKey("dbo.GameModes");
            AddPrimaryKey("dbo.GameModes", "mode");
            DropColumn("dbo.GameModes", "id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.GameModes", "id", c => c.Int(nullable: false, identity: true));
            DropPrimaryKey("dbo.GameModes");
            AddPrimaryKey("dbo.GameModes", "id");
            CreateIndex("dbo.GameModes", "mode", unique: true);
        }
    }
}
