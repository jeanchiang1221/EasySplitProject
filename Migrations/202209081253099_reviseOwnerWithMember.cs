namespace EasySplitProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class reviseOwnerWithMember : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Owners", "UserId", "dbo.Users");
            DropIndex("dbo.Owners", new[] { "UserId" });
            AddColumn("dbo.Owners", "MemberId", c => c.Int(nullable: false));
            DropColumn("dbo.Owners", "UserId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Owners", "UserId", c => c.Int(nullable: false));
            DropColumn("dbo.Owners", "MemberId");
            CreateIndex("dbo.Owners", "UserId");
            AddForeignKey("dbo.Owners", "UserId", "dbo.Users", "Id", cascadeDelete: true);
        }
    }
}
