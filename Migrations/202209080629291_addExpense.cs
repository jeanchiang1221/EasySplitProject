namespace EasySplitProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addExpense : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Expenses",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GroupId = c.Int(nullable: false),
                        CategoryId = c.Int(nullable: false),
                        Item = c.String(nullable: false, maxLength: 50),
                        Cost = c.Int(nullable: false),
                        CreatDate = c.DateTime(nullable: false),
                        PayerId = c.Int(nullable: false),
                        OwnerId = c.Int(nullable: false),
                        Memo = c.String(nullable: false, maxLength: 50),
                        AlbumId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Groups", t => t.GroupId, cascadeDelete: true)
                .Index(t => t.GroupId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Expenses", "GroupId", "dbo.Groups");
            DropIndex("dbo.Expenses", new[] { "GroupId" });
            DropTable("dbo.Expenses");
        }
    }
}
