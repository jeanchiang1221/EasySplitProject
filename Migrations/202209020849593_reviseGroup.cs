namespace EasySplitProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class reviseGroup : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Groups", "GroupGuid", c => c.String(maxLength: 50));
            AddColumn("dbo.Groups", "CreatDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Groups", "CreatDate");
            DropColumn("dbo.Groups", "GroupGuid");
        }
    }
}
