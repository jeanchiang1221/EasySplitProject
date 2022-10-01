namespace EasySplitProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addUser : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Account = c.String(nullable: false, maxLength: 50),
                        HashPassword = c.String(nullable: false, maxLength: 100),
                        Salt = c.String(maxLength: 50),
                        Image = c.String(maxLength: 200),
                        Name = c.String(maxLength: 50),
                        AccountState = c.Boolean(nullable: false),
                        CreatDate = c.DateTime(),
                        CheckMailCode = c.String(maxLength: 50),
                        MailCodeCreatDate = c.DateTime(nullable: false),
                        RefreshToken = c.String(maxLength: 50),
                        RefreshTokenCreatDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Users");
        }
    }
}
