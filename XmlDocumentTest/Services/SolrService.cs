using System;
using System.Data.Entity;
using System.Data.Entity.Core.Mapping;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using XmlDocumentTest.Models;
using XmlDocumentTest.Helpers;
using System.Xml.Serialization;
using SolrNet.Attributes;
using SolrNet;
using System.Collections.Specialized;
using Hangfire;
using Owin;
using Microsoft.Practices.ServiceLocation;
using SolrNet.Impl;

namespace XmlDocumentTest.Services
{
    public class SolrService
    {
        private ApplicationDbContext Db { get; set; }

        // Used to control my update system
        private static System.Object mSolrLock = new System.Object();
        private static bool mSolrUpdateRequired = true;
        private static SolrConnection mSolr { get; set; }

        public static void Init(System.Windows.Application app)
        {
            // Initialize Solr
            string server = System.Configuration.ConfigurationManager.AppSettings["SolrServer"];
            mSolr = new SolrConnection(server);
            Startup.Init<SolrIndex>(mSolr);

            // Setup Hangfire
            GlobalConfiguration.Configuration.UseSqlServerStorage("HangfireConnection");

            var hangfireServer = new BackgroundJobServer();
            BackgroundJob.Enqueue(() => Console.WriteLine("No OWIN"));

            RecurringJob.AddOrUpdate("solr-delta-index", () => UpdateSolrIndex(false), "*/2 * * * *");
            RecurringJob.AddOrUpdate("solr-full-index", () => UpdateSolrIndex(true), "0 0 * * *");
        }

        public static void RequestSolrUpdate()
        {
            lock (mSolrLock)
            {
                mSolrUpdateRequired = true;
            }
        }

        public static void UpdateSolrIndex(bool fullImport)
        {
            Console.WriteLine("Updating Solr Index");

            bool trigger = fullImport;

            lock (mSolrLock)
            {
                if (mSolrUpdateRequired)
                {
                    trigger = true;
                    mSolrUpdateRequired = false;
                }
            }

            if (trigger)
            {
                IDictionary <string, string> data = new Dictionary<string, string>()
                {
                    { "command", "delta-import" },
                    { "clean", "false" }
                };

                if (fullImport)
                {
                    data["command"] = "full-import";
                    data["clean"] = "true";
                }

                //mSolr.PostStream("/dataimport", Newtonsoft.Json.JsonConvert.SerializeObject(data));
            }
        }

        public SolrService(ApplicationDbContext db)
        {
            Db = db;
        }

        public void GenerateSolrIndexTemplate(TextWriter writer, Type[] models, bool mapChildren = false)
        {
            XDocument document = GenerateSolrIndexDocument(models, mapChildren);
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = false;
            settings.Indent = true;

            using (XmlWriter xw = XmlWriter.Create(writer, settings))
            {
                document.WriteTo(xw);
            }
        }

        private XDocument GenerateSolrIndexDocument(Type[] models, bool mapChildren)
        {
            XElement schema = new XElement("dataConfig");
            XDocument document = new XDocument(schema);

            // Initial settings
            schema.SetAttributeValue("name", "xml-document-test-schema");
            schema.SetAttributeValue("version", "1.6"); // Using solr version 1.6 schema syntax

            // Create the DataSource
            // TODO Get Connection details
            XElement dataSource = new XElement("dataSource");
            dataSource.SetAttributeValue("name", "d1");
            dataSource.SetAttributeValue("type", "JdbcDataSource");
            dataSource.SetAttributeValue("driver", "com.sqlserver.jdbc.Driver"); //TODO: find out.
            dataSource.SetAttributeValue("url", "jdbc:sqlserver://localhost/dbname");
            dataSource.SetAttributeValue("user", "user");
            dataSource.SetAttributeValue("password", "password");
            schema.Add(dataSource);

            dataSource = new XElement("dataSource");
            dataSource.SetAttributeValue("name", "d2");
            dataSource.SetAttributeValue("type", "FieldReaderDataSource");
            schema.Add(dataSource);


            // Create the fields
            XElement docElement = new XElement("document");
            docElement.SetAttributeValue("name", "entries");
            schema.Add(docElement);

            foreach (Type model in models)
            {
                docElement.Add(GenerateFieldDataForModel(model.Name, model, mapChildren));
            }
            
            return document;
        }

        private XElement GenerateFieldDataForModel(string name, Type model, bool mapChildren, string whereClause = null)
        {
            XElement entity = new XElement("entity");
            entity.SetAttributeValue("processor", "SqlEntityProcessor");
            entity.SetAttributeValue("dataSource", "d1");

            // Obtain any XML objects
            var properties = model.GetProperties().Where(p => p.GetCustomAttributes<XmlObjectAttribute>().Any());

            // Obtain the table data
            // Method obtained from: https://romiller.com/2015/08/05/ef6-1-get-mapping-between-properties-and-columns/
            var metadata = ((IObjectContextAdapter)Db).ObjectContext.MetadataWorkspace;
            var objectItemCollection = ((ObjectItemCollection)metadata.GetItemCollection(DataSpace.OSpace));

            var entityType = metadata.GetItems<EntityType>(DataSpace.OSpace)
                .Single(e => objectItemCollection.GetClrType(e) == model);

            var entitySet = metadata.GetItems<EntityContainer>(DataSpace.CSpace)
                .Single().EntitySets.Single(s => s.ElementType.Name == entityType.Name);

            var mapping = metadata.GetItems<EntityContainerMapping>(DataSpace.CSSpace)
                .Single().EntitySetMappings.Single(s => s.EntitySet == entitySet);

            var tableEntitySet = mapping.EntityTypeMappings.Single()
                .Fragments.Single().StoreEntitySet;

            string tableName = (string)tableEntitySet.MetadataProperties["Table"].Value ?? tableEntitySet.Name;
            var propertyMappings = mapping.EntityTypeMappings.Single().Fragments.Single().PropertyMappings;

            // Add the table to the query
            entity.SetAttributeValue("name", name);

            StringBuilder selectClause = new StringBuilder("select * from ");
            selectClause.Append(tableName);

            if (!string.IsNullOrEmpty(whereClause))
            {
                //TODO: Get details
                /*selectClause.Append(" where ");
                selectClause.Append("")*/
            }

            entity.SetAttributeValue("query", selectClause.ToString());

            // Add the properties
            foreach(var property in propertyMappings)
            {
                // Check if it's an XML object
                var associatedProperties = properties.Where(p => p.GetCustomAttributes<XmlObjectAttribute>().Where(a => a.XmlColumn == property.Property.Name).Any());

                if(associatedProperties.Count() > 0)
                {
                    foreach(var p in associatedProperties)
                    {
                        XElement childEntity = GenerateFieldDataForXmlModel(p.PropertyType);

                        childEntity.SetAttributeValue("name", p.Name);
                        childEntity.SetAttributeValue("dataSource", "d2");
                        childEntity.SetAttributeValue("dataField", string.Format("{0}.{1}", name, ((ScalarPropertyMapping)property).Column.Name));
                        childEntity.SetAttributeValue("forEach", string.Format("/{0}", p.Name));

                        entity.Add(childEntity);
                    }
                }
                else
                {
                    XElement field = new XElement("field");
                    field.SetAttributeValue("column", ((ScalarPropertyMapping)property).Column.Name);
                    field.SetAttributeValue("name", property.Property.Name);

                    entity.Add(field);
                }
            }

            return entity;
        }

        private XElement GenerateFieldDataForXmlModel(Type model)
        {
            XElement entity = new XElement("entity");
            entity.SetAttributeValue("processor", "XPathEntityProcessor");
            entity.SetAttributeValue("transformer", "RegexTransformer,DateFormatTransformer");

            string baseXPath = string.Format("/{0}", model.Name);

            AddXPathFields(entity, model, baseXPath);

            return entity;
        }

        private void AddXPathFields(XElement entity, Type model, string baseXPath, string namePath = "")
        {
            PropertyInfo[] properties = model.GetProperties(BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance);

            foreach(PropertyInfo property in properties)
            {
                if (property.PropertyType.IsGenericType)
                {
                    Type type = property.PropertyType.GetGenericArguments()[0];
                    AddXPathFields(entity, type, string.Format("{0}/{1}/{2}", baseXPath, property.Name, type.Name), string.Format("{0}{1}-{2}-", namePath, property.Name, type.Name));
                }
                else if (!property.PropertyType.IsPrimitive && !property.PropertyType.IsAssignableFrom(typeof(string)))
                {
                    AddXPathFields(entity, property.PropertyType, string.Format("{0}/{1}", baseXPath, property.Name), string.Format("{0}{1}-", namePath, property.Name));
                }
                else
                {
                    XElement field = new XElement("field");
                    

                    if (property.GetCustomAttributes<XmlAttributeAttribute>().Any())
                    {
                        field.SetAttributeValue("column", string.Format("{0}{1}", namePath, property.Name));
                        field.SetAttributeValue("xpath", string.Format("{0}@{1}", baseXPath, property.Name));
                    }
                    else
                    {
                        field.SetAttributeValue("column", namePath);
                        field.SetAttributeValue("xpath", baseXPath);
                    }

                    entity.Add(field);
                }
            }
        }
    }

    class SolrIndex
    {
        [SolrUniqueKey("Id")]
        public int Id { get; set; }

        [SolrField("FirstName")]
        public ICollection<string> FirstName { get; set; }

        [SolrField("FirstName-TestId")]
        public ICollection<string> FirstNameId { get; set; }

        [SolrField("LastName")]
        public ICollection<string> LastName { get; set; }

        [SolrField("LastName-TestId")]
        public ICollection<string> LastNameId { get; set; }

        [SolrField("Fields-Field")]
        public ICollection<string> Fields { get; set; }

        [SolrField("Fields-Field-TestId")]
        public ICollection<string> FieldsId { get; set; }
    }
}
