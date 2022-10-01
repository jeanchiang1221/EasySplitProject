namespace EasySplitProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class reviseOwnerV2 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Owners", "OwnAmount", c => c.Double(nullable: false));
            DropColumn("dbo.Owners", "settle");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Owners", "settle", c => c.Boolean(nullable: false));
            AlterColumn("dbo.Owners", "OwnAmount", c => c.Int(nullable: false));
        }
    }
}
