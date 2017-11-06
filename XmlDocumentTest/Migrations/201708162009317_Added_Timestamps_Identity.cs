namespace XmlDocumentTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Added_Timestamps_Identity : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.MainForms");
            AlterColumn("dbo.MainForms", "Id", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.MainForms", "Created", c => c.DateTime(nullable: false));
            AddPrimaryKey("dbo.MainForms", "Id");
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.MainForms");
            AlterColumn("dbo.MainForms", "Created", c => c.DateTime(nullable: false));
            AlterColumn("dbo.MainForms", "Id", c => c.Int(nullable: false));
            AddPrimaryKey("dbo.MainForms", "Id");
        }
    }
}
