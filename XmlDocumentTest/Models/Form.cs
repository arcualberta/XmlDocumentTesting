using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace XmlDocumentTest.Models
{
    public class Form
    {
        public Field FirstName { get; set; }
        public Field LastName { get; set; }
        
        public List<Field> Fields { get; set; }

        public Form()
        {
            FirstName = new Field() { TestId = Guid.NewGuid().ToString() };
            LastName = new Field();
            Fields = new List<Field>();
        }
    }

    public class Field
    {
        [XmlText]
        public string Content { get; set; }

        [XmlAttribute]
        public string TestId { get; set; }
    }

    public class TextArea
    {
        [XmlText]
        public string Content { get; set; }
    }
}
