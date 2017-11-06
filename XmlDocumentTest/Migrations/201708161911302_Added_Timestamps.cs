namespace XmlDocumentTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Added_Timestamps : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MainForms", "Created", c => c.DateTime(nullable: false, defaultValueSql: "GETUTCDATE()"));
            AddColumn("dbo.MainForms", "LastModified", c => c.DateTime(nullable: false, defaultValueSql: "GETUTCDATE()"));

            Sql("CREATE TRIGGER dbo.MainForm_UpdateLastModified ON dbo.MainForms AFTER UPDATE AS UPDATE dbo.MainForms SET LastModified = GETUTCDATE() WHERE Id IN (SELECT DISTINCT Id FROM Inserted)");
        }
        
        public override void Down()
        {
            Sql("DROP TRIGGER dbo.MainFrom_UpdateLastModified");
            DropColumn("dbo.MainForms", "LastModified");
            DropColumn("dbo.MainForms", "Created");
        }
    }
}
