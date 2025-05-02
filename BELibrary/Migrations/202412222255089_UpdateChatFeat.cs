namespace BELibrary.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateChatFeat : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Message", "DeletedBy", c => c.Guid());
            AlterColumn("dbo.Message", "PinnedBy", c => c.Guid());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Message", "PinnedBy", c => c.Guid(nullable: false));
            AlterColumn("dbo.Message", "DeletedBy", c => c.Guid(nullable: false));
        }
    }
}
