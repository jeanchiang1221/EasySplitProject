namespace EasySplitProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addOwner : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Owners",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ExpenseId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        OwnAmount = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Expenses", t => t.ExpenseId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.ExpenseId)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Owners", "UserId", "dbo.Users");
            DropForeignKey("dbo.Owners", "ExpenseId", "dbo.Expenses");
            DropIndex("dbo.Owners", new[] { "UserId" });
            DropIndex("dbo.Owners", new[] { "ExpenseId" });
            DropTable("dbo.Owners");
        }
    }
}
