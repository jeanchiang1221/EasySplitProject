namespace EasySplitProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class reviseGroup1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Groups", "UserId", c => c.Int(nullable: false));
            CreateIndex("dbo.Groups", "UserId");
            AddForeignKey("dbo.Groups", "UserId", "dbo.Users", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Groups", "UserId", "dbo.Users");
            DropIndex("dbo.Groups", new[] { "UserId" });
            DropColumn("dbo.Groups", "UserId");
        }
    }
}
