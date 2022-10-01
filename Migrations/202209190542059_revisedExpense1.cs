namespace EasySplitProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class revisedExpense1 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Expenses", "Cost", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Expenses", "Cost", c => c.Int(nullable: false));
        }
    }
}
