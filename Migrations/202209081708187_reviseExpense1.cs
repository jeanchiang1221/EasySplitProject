namespace EasySplitProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class reviseExpense1 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Expenses", "CategoryId", "dbo.Categories");
            DropIndex("dbo.Expenses", new[] { "CategoryId" });
            AlterColumn("dbo.Expenses", "CategoryId", c => c.Int());
            AlterColumn("dbo.Expenses", "CreatDate", c => c.DateTime());
            AlterColumn("dbo.Expenses", "Memo", c => c.String(maxLength: 50));
            CreateIndex("dbo.Expenses", "CategoryId");
            AddForeignKey("dbo.Expenses", "CategoryId", "dbo.Categories", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Expenses", "CategoryId", "dbo.Categories");
            DropIndex("dbo.Expenses", new[] { "CategoryId" });
            AlterColumn("dbo.Expenses", "Memo", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.Expenses", "CreatDate", c => c.DateTime(nullable: false));
            AlterColumn("dbo.Expenses", "CategoryId", c => c.Int(nullable: false));
            CreateIndex("dbo.Expenses", "CategoryId");
            AddForeignKey("dbo.Expenses", "CategoryId", "dbo.Categories", "Id", cascadeDelete: true);
        }
    }
}
