namespace EasySplitProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class revisePayerwithoutFK : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Payers", "UserId", "dbo.Users");
            DropIndex("dbo.Payers", new[] { "UserId" });
            DropColumn("dbo.Payers", "UserId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Payers", "UserId", c => c.Int(nullable: false));
            CreateIndex("dbo.Payers", "UserId");
            AddForeignKey("dbo.Payers", "UserId", "dbo.Users", "Id", cascadeDelete: true);
        }
    }
}
