namespace BELibrary.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateRecordTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PatientRecord", "ReasonForHospitalization", c => c.String());
            AddColumn("dbo.PatientRecord", "PersonalMedicalHistory", c => c.String());
            AddColumn("dbo.PatientRecord", "FamilyMedicalHistory", c => c.String());
            AddColumn("dbo.PatientRecord", "Treatment", c => c.String());
            AddColumn("dbo.PatientRecord", "Diagnosis", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.PatientRecord", "Diagnosis");
            DropColumn("dbo.PatientRecord", "Treatment");
            DropColumn("dbo.PatientRecord", "FamilyMedicalHistory");
            DropColumn("dbo.PatientRecord", "PersonalMedicalHistory");
            DropColumn("dbo.PatientRecord", "ReasonForHospitalization");
        }
    }
}
