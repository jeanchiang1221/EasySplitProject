namespace EasySplitProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class deleteUnnecessaryPayment : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.PaymentBanks", "Payment_Id", "dbo.Payments");
            DropForeignKey("dbo.PaymentCashes", "Payment_Id", "dbo.Payments");
            DropForeignKey("dbo.PaymentLines", "Payment_Id", "dbo.Payments");
            DropForeignKey("dbo.Payments", "UserId", "dbo.Users");
            DropIndex("dbo.PaymentBanks", new[] { "Payment_Id" });
            DropIndex("dbo.PaymentCashes", new[] { "Payment_Id" });
            DropIndex("dbo.PaymentLines", new[] { "Payment_Id" });
            DropIndex("dbo.Payments", new[] { "UserId" });
            DropColumn("dbo.PaymentBanks", "Payment_Id");
            DropColumn("dbo.PaymentCashes", "Payment_Id");
            DropColumn("dbo.PaymentLines", "Payment_Id");
            DropTable("dbo.Payments");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Payments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        Method = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.PaymentLines", "Payment_Id", c => c.Int());
            AddColumn("dbo.PaymentCashes", "Payment_Id", c => c.Int());
            AddColumn("dbo.PaymentBanks", "Payment_Id", c => c.Int());
            CreateIndex("dbo.Payments", "UserId");
            CreateIndex("dbo.PaymentLines", "Payment_Id");
            CreateIndex("dbo.PaymentCashes", "Payment_Id");
            CreateIndex("dbo.PaymentBanks", "Payment_Id");
            AddForeignKey("dbo.Payments", "UserId", "dbo.Users", "Id", cascadeDelete: true);
            AddForeignKey("dbo.PaymentLines", "Payment_Id", "dbo.Payments", "Id");
            AddForeignKey("dbo.PaymentCashes", "Payment_Id", "dbo.Payments", "Id");
            AddForeignKey("dbo.PaymentBanks", "Payment_Id", "dbo.Payments", "Id");
        }
    }
}
