namespace EasySplitProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class reviseOwnerWithSettle : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Owners", "settle", c => c.Boolean(nullable: false));
            DropColumn("dbo.Owners", "PayBackAmount");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Owners", "PayBackAmount", c => c.Int());
            DropColumn("dbo.Owners", "settle");
        }
    }
}
