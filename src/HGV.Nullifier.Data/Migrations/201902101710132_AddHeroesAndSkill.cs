namespace HGV.Nullifier.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddHeroesAndSkill : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Abilities",
                c => new
                    {
                        id = c.Int(nullable: false),
                        name = c.String(),
                        key = c.String(),
                        hero_id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.Heroes",
                c => new
                    {
                        id = c.Int(nullable: false),
                        name = c.String(),
                        key = c.String(),
                    })
                .PrimaryKey(t => t.id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Heroes");
            DropTable("dbo.Abilities");
        }
    }
}
