namespace EasySplitProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class revisePayerWithSettle : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Payers", "settle", c => c.Boolean(nullable: false));
            DropColumn("dbo.Payers", "ReceiveAmount");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Payers", "ReceiveAmount", c => c.Int());
            DropColumn("dbo.Payers", "settle");
        }
    }
}
