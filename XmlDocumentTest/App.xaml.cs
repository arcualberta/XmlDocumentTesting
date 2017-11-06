using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using XmlDocumentTest.Models;
using XmlDocumentTest.Services;

namespace XmlDocumentTest
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (e.Args.Contains("GenerateIndex"))
            {
                SolrService.Init(this);
                //SolrService solrSrv = new SolrService(new ApplicationDbContext());
                //solrSrv.GenerateSolrIndexTemplate(Console.Out, new Type[] { typeof(MainForm) }, true);
                
            }
        }
    }
}
