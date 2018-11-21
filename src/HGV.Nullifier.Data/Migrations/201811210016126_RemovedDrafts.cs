namespace HGV.Nullifier.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemovedDrafts : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.DraftStats", new[] { "key" });
            DropTable("dbo.DraftStats");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.DraftStats",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        key = c.String(maxLength: 16),
                        is_same_hero = c.Boolean(nullable: false),
                        names = c.String(),
                        picks = c.Int(nullable: false),
                        wins = c.Int(nullable: false),
                        win_rate = c.Single(nullable: false),
                    })
                .PrimaryKey(t => t.id);
            
            CreateIndex("dbo.DraftStats", "key", unique: true);
        }
    }
}
