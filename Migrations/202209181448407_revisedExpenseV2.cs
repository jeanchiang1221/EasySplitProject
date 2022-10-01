namespace EasySplitProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class revisedExpenseV2 : DbMigration
    {
        public override void Up()
        {
         //   DropForeignKey("dbo.Expenses", "Category_Id", "dbo.Categories");
            DropIndex("dbo.Expenses", new[] { "Category_Id" });
            DropColumn("dbo.Expenses", "Category_Id");
            DropTable("dbo.Categories");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Categories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Expenses", "Category_Id", c => c.Int());
            CreateIndex("dbo.Expenses", "Category_Id");
           // AddForeignKey("dbo.Expenses", "Category_Id", "dbo.Categories", "Id");
        }
    }
}
