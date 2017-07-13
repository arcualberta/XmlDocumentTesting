using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace XmlDocumentTest.Helpers
{
    /**
     * Used to define an XmlObject for pathing. The system will also create the appropriate indexes for the database.
     **/
    [AttributeUsage(AttributeTargets.Property)]
    public class XmlObjectAttribute : Attribute
    {
        public string XmlColumn { get; set; }

        public XmlObjectAttribute(string xmlColumn = "Content" ) //TODO: Add indexing options
        {
            XmlColumn = xmlColumn;
        }
    }
  
}
