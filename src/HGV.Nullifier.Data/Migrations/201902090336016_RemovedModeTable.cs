namespace HGV.Nullifier.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemovedModeTable : DbMigration
    {
        public override void Up()
        {
            DropTable("dbo.GameModes");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.GameModes",
                c => new
                    {
                        mode = c.Int(nullable: false),
                        count = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.mode);
            
        }
    }
}
