namespace EasySplitProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class revisePayerV2 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Payers", "PayAmount", c => c.Double(nullable: false));
            DropColumn("dbo.Payers", "settle");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Payers", "settle", c => c.Boolean(nullable: false));
            AlterColumn("dbo.Payers", "PayAmount", c => c.Int(nullable: false));
        }
    }
}
