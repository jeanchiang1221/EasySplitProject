namespace EasySplitProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class reviseExpense : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ExpenseAlbums",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        ExpenseId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Expenses", t => t.ExpenseId, cascadeDelete: true)
                .Index(t => t.ExpenseId);
            
            AddColumn("dbo.Categories", "Expense_Id", c => c.Int());
            CreateIndex("dbo.Categories", "Expense_Id");
            AddForeignKey("dbo.Categories", "Expense_Id", "dbo.Expenses", "Id");
            DropColumn("dbo.Expenses", "CategoryId");
            DropColumn("dbo.Expenses", "PayerId");
            DropColumn("dbo.Expenses", "OwnerId");
            DropColumn("dbo.Expenses", "AlbumId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Expenses", "AlbumId", c => c.Int(nullable: false));
            AddColumn("dbo.Expenses", "OwnerId", c => c.Int(nullable: false));
            AddColumn("dbo.Expenses", "PayerId", c => c.Int(nullable: false));
            AddColumn("dbo.Expenses", "CategoryId", c => c.Int(nullable: false));
            DropForeignKey("dbo.ExpenseAlbums", "ExpenseId", "dbo.Expenses");
            DropForeignKey("dbo.Categories", "Expense_Id", "dbo.Expenses");
            DropIndex("dbo.ExpenseAlbums", new[] { "ExpenseId" });
            DropIndex("dbo.Categories", new[] { "Expense_Id" });
            DropColumn("dbo.Categories", "Expense_Id");
            DropTable("dbo.ExpenseAlbums");
        }
    }
}
