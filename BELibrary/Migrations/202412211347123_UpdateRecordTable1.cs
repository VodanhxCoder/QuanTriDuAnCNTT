namespace BELibrary.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateRecordTable1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Record", "Treatment", c => c.String());
            AddColumn("dbo.Record", "Diagnosis", c => c.String());
            DropColumn("dbo.PatientRecord", "Treatment");
            DropColumn("dbo.PatientRecord", "Diagnosis");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PatientRecord", "Diagnosis", c => c.String());
            AddColumn("dbo.PatientRecord", "Treatment", c => c.String());
            DropColumn("dbo.Record", "Diagnosis");
            DropColumn("dbo.Record", "Treatment");
        }
    }
}
