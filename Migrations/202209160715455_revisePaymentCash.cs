namespace EasySplitProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class revisePaymentCash : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PaymentCashes", "Name", c => c.String(maxLength: 50));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PaymentCashes", "Name");
        }
    }
}
