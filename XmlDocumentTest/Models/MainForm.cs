using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using XmlDocumentTest.Helpers;

namespace XmlDocumentTest.Models
{
    public class MainForm
    {
        public int Id { get; set; }
        
        [Column(TypeName = "xml")]
        public string Content { get; set; }

        [NotMapped]
        private Form mXmlContent { get; set; }

        [XmlObject("Content")]
        [NotMapped]
        public Form XmlContent
        {
            get
            {
                if (mXmlContent == null)
                {
                    if (String.IsNullOrEmpty(Content)) {
                        mXmlContent = new Form();
                        Update();
                    }

                    using (StringReader reader = new StringReader(Content))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(Form));
                        mXmlContent = (Form)serializer.Deserialize(reader);
                    }
                }

                return mXmlContent;
            }
        }

        public void Invalidate()
        {
            mXmlContent = null;
        }

        public void Update()
        {
            using (StringWriter writer = new StringWriter())
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Form));
                serializer.Serialize(writer, mXmlContent);
                Content = writer.ToString();
            }
        }
    }
}
