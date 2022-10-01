namespace EasySplitProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class reviseMember : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Members", "UserId", "dbo.Users");
            DropIndex("dbo.Members", new[] { "UserId" });
            AlterColumn("dbo.Members", "UserId", c => c.Int());
            CreateIndex("dbo.Members", "UserId");
            AddForeignKey("dbo.Members", "UserId", "dbo.Users", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Members", "UserId", "dbo.Users");
            DropIndex("dbo.Members", new[] { "UserId" });
            AlterColumn("dbo.Members", "UserId", c => c.Int(nullable: false));
            CreateIndex("dbo.Members", "UserId");
            AddForeignKey("dbo.Members", "UserId", "dbo.Users", "Id", cascadeDelete: true);
        }
    }
}
