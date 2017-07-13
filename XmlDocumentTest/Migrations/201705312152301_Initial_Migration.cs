namespace XmlDocumentTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial_Migration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MainForms",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Content = c.String(storeType: "xml"),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.MainForms");
        }
    }
}
