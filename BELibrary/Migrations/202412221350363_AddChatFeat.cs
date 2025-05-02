namespace BELibrary.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddChatFeat : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Group",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Type = c.String(),
                        Avatar = c.String(),
                        Name = c.String(),
                        Created = c.DateTime(nullable: false),
                        CreatedBy = c.Guid(nullable: false),
                        LastActive = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.GroupUser",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        GroupId = c.Guid(nullable: false),
                        UserId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Group", t => t.GroupId, cascadeDelete: true)
                .ForeignKey("dbo.Account", t => t.UserId, cascadeDelete: true)
                .Index(t => t.GroupId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Message",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Type = c.String(),
                        GroupId = c.Guid(nullable: false),
                        Content = c.String(),
                        Path = c.String(),
                        Created = c.DateTime(nullable: false),
                        CreatedBy = c.Guid(nullable: false),
                        IsDeleted = c.Boolean(),
                        DeletedBy = c.Guid(nullable: false),
                        DeletedDate = c.DateTime(),
                        IsPinned = c.Boolean(),
                        PinnedBy = c.Guid(nullable: false),
                        PinnedDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Group", t => t.GroupId, cascadeDelete: true)
                .ForeignKey("dbo.Account", t => t.CreatedBy, cascadeDelete: true)
                .Index(t => t.GroupId)
                .Index(t => t.CreatedBy);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Message", "CreatedBy", "dbo.Account");
            DropForeignKey("dbo.Message", "GroupId", "dbo.Group");
            DropForeignKey("dbo.GroupUser", "UserId", "dbo.Account");
            DropForeignKey("dbo.GroupUser", "GroupId", "dbo.Group");
            DropIndex("dbo.Message", new[] { "CreatedBy" });
            DropIndex("dbo.Message", new[] { "GroupId" });
            DropIndex("dbo.GroupUser", new[] { "UserId" });
            DropIndex("dbo.GroupUser", new[] { "GroupId" });
            DropTable("dbo.Message");
            DropTable("dbo.GroupUser");
            DropTable("dbo.Group");
        }
    }
}
