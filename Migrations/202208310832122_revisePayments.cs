namespace EasySplitProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class revisePayments : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.PaymentBanks", "PaymentId", "dbo.Payments");
            DropForeignKey("dbo.PaymentCashes", "PaymentId", "dbo.Payments");
            DropForeignKey("dbo.PaymentLines", "PaymentId", "dbo.Payments");
            DropIndex("dbo.PaymentBanks", new[] { "PaymentId" });
            DropIndex("dbo.PaymentCashes", new[] { "PaymentId" });
            DropIndex("dbo.PaymentLines", new[] { "PaymentId" });
            RenameColumn(table: "dbo.PaymentBanks", name: "PaymentId", newName: "Payment_Id");
            RenameColumn(table: "dbo.PaymentCashes", name: "PaymentId", newName: "Payment_Id");
            RenameColumn(table: "dbo.PaymentLines", name: "PaymentId", newName: "Payment_Id");
            AddColumn("dbo.PaymentBanks", "UserId", c => c.Int(nullable: false));
            AddColumn("dbo.PaymentCashes", "UserId", c => c.Int(nullable: false));
            AddColumn("dbo.PaymentLines", "UserId", c => c.Int(nullable: false));
            AlterColumn("dbo.PaymentBanks", "Payment_Id", c => c.Int());
            AlterColumn("dbo.PaymentCashes", "Payment_Id", c => c.Int());
            AlterColumn("dbo.PaymentLines", "Payment_Id", c => c.Int());
            CreateIndex("dbo.PaymentBanks", "UserId");
            CreateIndex("dbo.PaymentBanks", "Payment_Id");
            CreateIndex("dbo.PaymentCashes", "UserId");
            CreateIndex("dbo.PaymentCashes", "Payment_Id");
            CreateIndex("dbo.PaymentLines", "UserId");
            CreateIndex("dbo.PaymentLines", "Payment_Id");
            AddForeignKey("dbo.PaymentBanks", "UserId", "dbo.Users", "Id", cascadeDelete: true);
            AddForeignKey("dbo.PaymentCashes", "UserId", "dbo.Users", "Id", cascadeDelete: true);
            AddForeignKey("dbo.PaymentLines", "UserId", "dbo.Users", "Id", cascadeDelete: true);
            AddForeignKey("dbo.PaymentBanks", "Payment_Id", "dbo.Payments", "Id");
            AddForeignKey("dbo.PaymentCashes", "Payment_Id", "dbo.Payments", "Id");
            AddForeignKey("dbo.PaymentLines", "Payment_Id", "dbo.Payments", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PaymentLines", "Payment_Id", "dbo.Payments");
            DropForeignKey("dbo.PaymentCashes", "Payment_Id", "dbo.Payments");
            DropForeignKey("dbo.PaymentBanks", "Payment_Id", "dbo.Payments");
            DropForeignKey("dbo.PaymentLines", "UserId", "dbo.Users");
            DropForeignKey("dbo.PaymentCashes", "UserId", "dbo.Users");
            DropForeignKey("dbo.PaymentBanks", "UserId", "dbo.Users");
            DropIndex("dbo.PaymentLines", new[] { "Payment_Id" });
            DropIndex("dbo.PaymentLines", new[] { "UserId" });
            DropIndex("dbo.PaymentCashes", new[] { "Payment_Id" });
            DropIndex("dbo.PaymentCashes", new[] { "UserId" });
            DropIndex("dbo.PaymentBanks", new[] { "Payment_Id" });
            DropIndex("dbo.PaymentBanks", new[] { "UserId" });
            AlterColumn("dbo.PaymentLines", "Payment_Id", c => c.Int(nullable: false));
            AlterColumn("dbo.PaymentCashes", "Payment_Id", c => c.Int(nullable: false));
            AlterColumn("dbo.PaymentBanks", "Payment_Id", c => c.Int(nullable: false));
            DropColumn("dbo.PaymentLines", "UserId");
            DropColumn("dbo.PaymentCashes", "UserId");
            DropColumn("dbo.PaymentBanks", "UserId");
            RenameColumn(table: "dbo.PaymentLines", name: "Payment_Id", newName: "PaymentId");
            RenameColumn(table: "dbo.PaymentCashes", name: "Payment_Id", newName: "PaymentId");
            RenameColumn(table: "dbo.PaymentBanks", name: "Payment_Id", newName: "PaymentId");
            CreateIndex("dbo.PaymentLines", "PaymentId");
            CreateIndex("dbo.PaymentCashes", "PaymentId");
            CreateIndex("dbo.PaymentBanks", "PaymentId");
            AddForeignKey("dbo.PaymentLines", "PaymentId", "dbo.Payments", "Id", cascadeDelete: true);
            AddForeignKey("dbo.PaymentCashes", "PaymentId", "dbo.Payments", "Id", cascadeDelete: true);
            AddForeignKey("dbo.PaymentBanks", "PaymentId", "dbo.Payments", "Id", cascadeDelete: true);
        }
    }
}
