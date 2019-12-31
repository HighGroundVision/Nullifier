namespace HGV.Nullifier.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ImportProject : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AbilityDailyCounts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Day = c.Int(nullable: false),
                        AbilityId = c.String(),
                        AbilityName = c.String(),
                        IsSkill = c.Boolean(nullable: false),
                        MostKills = c.Int(nullable: false),
                        Wins = c.Int(nullable: false),
                        Losses = c.Int(nullable: false),
                        WinRate = c.Double(nullable: false),
                        DraftOrder = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Day);
            
            CreateTable(
                "dbo.HeroDailyCounts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        HeroId = c.Int(nullable: false),
                        HeroName = c.String(),
                        Day = c.Int(nullable: false),
                        Wins = c.Int(nullable: false),
                        Losses = c.Int(nullable: false),
                        WinRate = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Day);
            
            CreateTable(
                "dbo.PlayerDailyCounts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AccountId = c.Long(nullable: false),
                        SteamId = c.Long(nullable: false),
                        Persona = c.String(),
                        Day = c.Int(nullable: false),
                        Wins = c.Int(nullable: false),
                        Losses = c.Int(nullable: false),
                        WinRate = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Day);
            
            CreateTable(
                "dbo.RegionDailyCounts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        RegionId = c.Int(nullable: false),
                        Day = c.Int(nullable: false),
                        Hour = c.Int(nullable: false),
                        Matches = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Day)
                .Index(t => t.Hour);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.RegionDailyCounts", new[] { "Hour" });
            DropIndex("dbo.RegionDailyCounts", new[] { "Day" });
            DropIndex("dbo.PlayerDailyCounts", new[] { "Day" });
            DropIndex("dbo.HeroDailyCounts", new[] { "Day" });
            DropIndex("dbo.AbilityDailyCounts", new[] { "Day" });
            DropTable("dbo.RegionDailyCounts");
            DropTable("dbo.PlayerDailyCounts");
            DropTable("dbo.HeroDailyCounts");
            DropTable("dbo.AbilityDailyCounts");
        }
    }
}
