namespace EasySplitProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class revisePayerWithReceiveAmount : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Payers", "ReceiveAmount", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Payers", "ReceiveAmount");
        }
    }
}
