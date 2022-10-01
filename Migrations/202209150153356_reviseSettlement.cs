namespace EasySplitProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class reviseSettlement : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Settlements", "OwnerMemberId", c => c.Int(nullable: false));
            AddColumn("dbo.Settlements", "OwnAmountresult", c => c.Double(nullable: false));
            AddColumn("dbo.Settlements", "PayerMemberId", c => c.Int(nullable: false));
            DropColumn("dbo.Settlements", "MemberId");
            DropColumn("dbo.Settlements", "PayerShouldGet");
            DropColumn("dbo.Settlements", "OwnerShouldPay");
            DropColumn("dbo.Settlements", "PayGetFromOwnerAmount");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Settlements", "PayGetFromOwnerAmount", c => c.Int());
            AddColumn("dbo.Settlements", "OwnerShouldPay", c => c.Int());
            AddColumn("dbo.Settlements", "PayerShouldGet", c => c.Int());
            AddColumn("dbo.Settlements", "MemberId", c => c.Int(nullable: false));
            DropColumn("dbo.Settlements", "PayerMemberId");
            DropColumn("dbo.Settlements", "OwnAmountresult");
            DropColumn("dbo.Settlements", "OwnerMemberId");
        }
    }
}
