namespace EasySplitProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class reviseExpenseWithCategory : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Categories", "Expense_Id", "dbo.Expenses");
            DropIndex("dbo.Categories", new[] { "Expense_Id" });
            AddColumn("dbo.Expenses", "CategoryId", c => c.Int(nullable: false));
            CreateIndex("dbo.Expenses", "CategoryId");
            AddForeignKey("dbo.Expenses", "CategoryId", "dbo.Categories", "Id", cascadeDelete: true);
            DropColumn("dbo.Categories", "Expense_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Categories", "Expense_Id", c => c.Int());
            DropForeignKey("dbo.Expenses", "CategoryId", "dbo.Categories");
            DropIndex("dbo.Expenses", new[] { "CategoryId" });
            DropColumn("dbo.Expenses", "CategoryId");
            CreateIndex("dbo.Categories", "Expense_Id");
            AddForeignKey("dbo.Categories", "Expense_Id", "dbo.Expenses", "Id");
        }
    }
}
