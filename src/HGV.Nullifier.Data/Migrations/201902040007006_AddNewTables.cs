namespace HGV.Nullifier.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddNewTables : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AbilityDraftStats",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        match_id = c.Long(nullable: false),
                        match_number = c.Long(nullable: false),
                        match_result = c.Boolean(nullable: false),
                        duration = c.Int(nullable: false),
                        start_time = c.Long(nullable: false),
                        player_slot = c.Int(nullable: false),
                        player_index = c.Int(nullable: false),
                        account_id = c.Long(nullable: false),
                        hero_id = c.Int(nullable: false),
                        kills = c.Int(nullable: false),
                        deaths = c.Int(nullable: false),
                        assists = c.Int(nullable: false),
                        last_hits = c.Int(nullable: false),
                        denies = c.Int(nullable: false),
                        gold = c.Int(nullable: false),
                        level = c.Int(nullable: false),
                        gold_per_min = c.Int(nullable: false),
                        xp_per_min = c.Int(nullable: false),
                        gold_spent = c.Int(nullable: false),
                        hero_damage = c.Int(nullable: false),
                        tower_damage = c.Int(nullable: false),
                        hero_healing = c.Int(nullable: false),
                        item_0 = c.Int(nullable: false),
                        item_1 = c.Int(nullable: false),
                        item_2 = c.Int(nullable: false),
                        item_3 = c.Int(nullable: false),
                        item_4 = c.Int(nullable: false),
                        item_5 = c.Int(nullable: false),
                        backpack_0 = c.Int(nullable: false),
                        backpack_1 = c.Int(nullable: false),
                        backpack_2 = c.Int(nullable: false),
                        ability_0 = c.Int(nullable: false),
                        ability_1 = c.Int(nullable: false),
                        ability_2 = c.Int(nullable: false),
                        ability_3 = c.Int(nullable: false),
                        ultimate_0 = c.Int(nullable: false),
                        ultimate_1 = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.CollectionStats",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.GameModeStats",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        mode = c.Int(nullable: false),
                        name = c.String(),
                        count = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .Index(t => t.mode, unique: true);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.GameModeStats", new[] { "mode" });
            DropTable("dbo.GameModeStats");
            DropTable("dbo.CollectionStats");
            DropTable("dbo.AbilityDraftStats");
        }
    }
}
