using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XmlDocumentTest.Helpers.Migrations;

namespace XmlDocumentTest.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
            : base("DefaultConnection")
        {
        }
        
        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            /*modelBuilder.Properties()
                .Where(x => x.GetCustomAttributes(false).OfType<XmlIndexAttribute>().Any())
                .Configure*/
        }

       public  DbSet<MainForm> MainForms { get; set; }
    }
}
