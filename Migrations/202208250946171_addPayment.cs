namespace EasySplitProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addPayment : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PaymentBanks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PaymentId = c.Int(nullable: false),
                        AccountName = c.String(maxLength: 50),
                        BankCode = c.String(maxLength: 50),
                        Bank = c.String(maxLength: 50),
                        Account = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Payments", t => t.PaymentId, cascadeDelete: true)
                .Index(t => t.PaymentId);
            
            CreateTable(
                "dbo.Payments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        Method = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.PaymentCashes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PaymentId = c.Int(nullable: false),
                        Phone = c.String(maxLength: 50),
                        Method = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Payments", t => t.PaymentId, cascadeDelete: true)
                .Index(t => t.PaymentId);
            
            CreateTable(
                "dbo.PaymentLines",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PaymentId = c.Int(nullable: false),
                        Name = c.String(maxLength: 50),
                        LineID = c.String(maxLength: 50),
                        Phone = c.String(maxLength: 50),
                        QRCode = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Payments", t => t.PaymentId, cascadeDelete: true)
                .Index(t => t.PaymentId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Payments", "UserId", "dbo.Users");
            DropForeignKey("dbo.PaymentLines", "PaymentId", "dbo.Payments");
            DropForeignKey("dbo.PaymentCashes", "PaymentId", "dbo.Payments");
            DropForeignKey("dbo.PaymentBanks", "PaymentId", "dbo.Payments");
            DropIndex("dbo.PaymentLines", new[] { "PaymentId" });
            DropIndex("dbo.PaymentCashes", new[] { "PaymentId" });
            DropIndex("dbo.Payments", new[] { "UserId" });
            DropIndex("dbo.PaymentBanks", new[] { "PaymentId" });
            DropTable("dbo.PaymentLines");
            DropTable("dbo.PaymentCashes");
            DropTable("dbo.Payments");
            DropTable("dbo.PaymentBanks");
        }
    }
}
