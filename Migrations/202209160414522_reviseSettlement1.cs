namespace EasySplitProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class reviseSettlement1 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Settlements", "PaymentMethod", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Settlements", "PaymentMethod", c => c.String());
        }
    }
}
