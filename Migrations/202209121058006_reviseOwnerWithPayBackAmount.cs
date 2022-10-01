namespace EasySplitProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class reviseOwnerWithPayBackAmount : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Owners", "PayBackAmount", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Owners", "PayBackAmount");
        }
    }
}
