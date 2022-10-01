namespace EasySplitProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class reviseGroupWithRenameColumn : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.Groups", name: "UserId", newName: "CreatorId");
            RenameIndex(table: "dbo.Groups", name: "IX_UserId", newName: "IX_CreatorId");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.Groups", name: "IX_CreatorId", newName: "IX_UserId");
            RenameColumn(table: "dbo.Groups", name: "CreatorId", newName: "UserId");
        }
    }
}
