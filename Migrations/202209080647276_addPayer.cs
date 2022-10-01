namespace EasySplitProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addPayer : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Payers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ExpenseId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        PayAmount = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Expenses", t => t.ExpenseId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.ExpenseId)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Payers", "UserId", "dbo.Users");
            DropForeignKey("dbo.Payers", "ExpenseId", "dbo.Expenses");
            DropIndex("dbo.Payers", new[] { "UserId" });
            DropIndex("dbo.Payers", new[] { "ExpenseId" });
            DropTable("dbo.Payers");
        }
    }
}
