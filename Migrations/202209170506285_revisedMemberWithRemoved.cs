namespace EasySplitProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class revisedMemberWithRemoved : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Members", "RemovedFromGroup", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Members", "RemovedFromGroup");
        }
    }
}
