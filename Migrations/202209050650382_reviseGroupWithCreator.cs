namespace EasySplitProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class reviseGroupWithCreator : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Groups", "CreatorId", "dbo.Users");
            DropIndex("dbo.Groups", new[] { "CreatorId" });
            AddColumn("dbo.Groups", "CreatorAccount", c => c.String());
            DropColumn("dbo.Groups", "CreatorId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Groups", "CreatorId", c => c.Int(nullable: false));
            DropColumn("dbo.Groups", "CreatorAccount");
            CreateIndex("dbo.Groups", "CreatorId");
            AddForeignKey("dbo.Groups", "CreatorId", "dbo.Users", "Id", cascadeDelete: true);
        }
    }
}
