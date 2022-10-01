namespace EasySplitProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class revisedExpense : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.Expenses", name: "CategoryId", newName: "Category_Id");
            RenameIndex(table: "dbo.Expenses", name: "IX_CategoryId", newName: "IX_Category_Id");
            AddColumn("dbo.Expenses", "ExpenseType", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Expenses", "ExpenseType");
            RenameIndex(table: "dbo.Expenses", name: "IX_Category_Id", newName: "IX_CategoryId");
            RenameColumn(table: "dbo.Expenses", name: "Category_Id", newName: "CategoryId");
        }
    }
}
