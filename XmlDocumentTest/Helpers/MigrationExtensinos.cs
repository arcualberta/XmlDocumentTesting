using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Data.Entity.Migrations.Infrastructure;
using System.Data.Entity.Migrations.Model;
using System.Data.Entity.SqlServer;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlDocumentTest.Helpers.Migrations
{
    public class CreateXmlIndexOperation : MigrationOperation
    {
        public string Table { get; set; }
        public string Column { get; set; }
        public string IndexName { get; set; }
        public bool IsPrimary { get; set; }
        public eIndexType IndexType { get; set; }
        public string PrimaryKeyName { get; set; }

        public enum eIndexType
        {
            VALUE,
            PATH,
            PROPERTY
        }

        public CreateXmlIndexOperation(string table, string column, string keyName, bool isPrimary = true, eIndexType indexType = eIndexType.VALUE, string primaryKeyName = null) 
            : base(null)
        {
            Table = table;
            Column = column;
            IndexName = keyName;
            IsPrimary = isPrimary;
            IndexType = indexType;
            PrimaryKeyName = primaryKeyName;
        }

        public override bool IsDestructiveChange
        {
            get { return false; }
        }

        public void WriteUpAsSql(TextWriter writer)
        {
            writer.WriteLine("CREATE {0} XML INDEX {1} ON {2} ( {3} )",
                IsPrimary ? "PRIMARY" : "",
                IndexName, Table, Column);

            if (!IsPrimary)
            {
                writer.WriteLine("USING XML INDEX {0} FOR {1}");
            }

            writer.WriteLine(";");
        }
    }

    public static class MigrationXmlExtensions
    {
        public static void CreateXmlIndex(this DbMigration migration, string table, string column, string keyName, bool isPrimary = true, CreateXmlIndexOperation.eIndexType indexType = CreateXmlIndexOperation.eIndexType.VALUE, string primaryKeyName = null)
        {
            ((IDbMigration)migration).AddOperation(new CreateXmlIndexOperation(table, column, keyName, isPrimary, indexType, primaryKeyName));
        }
    }

    public class XmlSqlServerMigrationSqlGenerator : SqlServerMigrationSqlGenerator
    {
        protected override void Generate(MigrationOperation migrationOperation)
        {
            if(migrationOperation != null)
            {
                Type type = migrationOperation.GetType();

                if (typeof(CreateXmlIndexOperation).IsAssignableFrom(type))
                {
                    using(var writer = Writer())
                    {

                        ((CreateXmlIndexOperation)migrationOperation).WriteUpAsSql(writer);
                        Statement(writer);
                    }
                    return;
                }
            }
            
            base.Generate(migrationOperation);
        }
    }
}
