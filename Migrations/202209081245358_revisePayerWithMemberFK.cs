namespace EasySplitProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class revisePayerWithMemberFK : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Payers", "MemberId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Payers", "MemberId");
        }
    }
}
