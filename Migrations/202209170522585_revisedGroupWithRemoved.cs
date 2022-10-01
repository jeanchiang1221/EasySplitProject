namespace EasySplitProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class revisedGroupWithRemoved : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Groups", "Removed", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Groups", "Removed");
        }
    }
}
