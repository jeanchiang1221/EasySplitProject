namespace EasySplitProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class reviseSettlementV2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Settlements", "CreatDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Settlements", "PaymentMethod", c => c.String());
            AddColumn("dbo.Settlements", "Memo", c => c.String(maxLength: 200));
            AddColumn("dbo.Settlements", "Image", c => c.String(maxLength: 200));
            AlterColumn("dbo.Settlements", "OwnerPaytoPayerAmount", c => c.Int(nullable: false));
            DropColumn("dbo.Settlements", "OwnAmountresult");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Settlements", "OwnAmountresult", c => c.Double(nullable: false));
            AlterColumn("dbo.Settlements", "OwnerPaytoPayerAmount", c => c.Int());
            DropColumn("dbo.Settlements", "Image");
            DropColumn("dbo.Settlements", "Memo");
            DropColumn("dbo.Settlements", "PaymentMethod");
            DropColumn("dbo.Settlements", "CreatDate");
        }
    }
}
