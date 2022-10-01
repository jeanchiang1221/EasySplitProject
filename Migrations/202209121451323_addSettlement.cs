namespace EasySplitProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addSettlement : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Settlements",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GroupId = c.Int(nullable: false),
                        MemberId = c.Int(nullable: false),
                        PayerShouldGet = c.Int(),
                        OwnerShouldPay = c.Int(),
                        PayGetFromOwnerAmount = c.Int(),
                        OwnerPaytoPayerAmount = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Groups", t => t.GroupId, cascadeDelete: true)
                .Index(t => t.GroupId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Settlements", "GroupId", "dbo.Groups");
            DropIndex("dbo.Settlements", new[] { "GroupId" });
            DropTable("dbo.Settlements");
        }
    }
}
