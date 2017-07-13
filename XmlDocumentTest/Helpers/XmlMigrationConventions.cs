using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlDocumentTest.Helpers.Migrations
{
    public class XmlIndexConvention : Convention
    {
        public XmlIndexConvention()
        {
            this.Properties()
                .Where(x => x.GetCustomAttributes(false).OfType<XmlIndexAttribute>().Any())
                .Configure(configuration =>
                {
                    XmlIndexAttribute attribute = configuration.ClrPropertyInfo.GetCustomAttributes(true).OfType<XmlIndexAttribute>().First();

                });
        }
    }
}
