namespace HGV.Nullifier.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TalentHeroStats : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TalentHeroStats",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        talent = c.Int(nullable: false),
                        hero = c.Int(nullable: false),
                        names = c.String(),
                        picks = c.Int(nullable: false),
                        wins = c.Int(nullable: false),
                        win_rate = c.Single(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .Index(t => new { t.talent, t.hero }, name: "IX_TalentAndHero");
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.TalentHeroStats", "IX_TalentAndHero");
            DropTable("dbo.TalentHeroStats");
        }
    }
}
