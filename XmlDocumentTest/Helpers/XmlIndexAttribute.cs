using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlDocumentTest.Helpers.Migrations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class XmlIndexAttribute : Attribute
    {
        public string Table { get; set; }
        public string Column { get; set; }
        public string IndexName { get; set; }
        public bool IsPrimary { get; set; }
        public string PrimaryKeyName { get; set; }
        public bool IndexOnValue { get; set; }
        public bool IndexOnPath { get; set; }
        public bool IndexOnProperties { get; set; }

        public XmlIndexAttribute(string table, string column, string keyName)
        {
            Table = table;
            Column = column;
            IndexName = keyName;
            IsPrimary = false;
            PrimaryKeyName = null;
        }
    }
}
