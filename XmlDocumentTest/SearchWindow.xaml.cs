using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using XmlDocumentTest.Models;
using XmlDocumentTest.Helpers;

namespace XmlDocumentTest
{
    /// <summary>
    /// Interaction logic for SearchWindow.xaml
    /// </summary>
    public partial class SearchWindow : Window
    {
        public ApplicationDbContext Db { get; set; }

        public SearchWindow()
        {
            InitializeComponent();
            Db = new ApplicationDbContext();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            var sql = @"SELECT * FROM[dbo].[MainForms] WHERE Content.exist('" + QueryPath.Text.Replace("'", "''") + "') = 1";

            //var query = Db.MainForms.SqlQuery(sql);
            //IList<MainForm> forms = query.ToList();
            //IList<MainForm> forms = Db.MainForms.OrderBy(m => m.Id).Where(m => m.Id > 0).WhereXPath(m => m.XmlContent.FirstName.Content == "Test").ToList(); 
            IList<MainForm> forms = Db.MainForms.WhereXPath(m => m.XmlContent.FirstName.Content == "Test").ToList();
            SearchResult.ItemsSource = forms;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Db.Dispose();
        }

        private void SearchResult_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MainForm selected = (MainForm)SearchResult.SelectedItem;

            if (selected == null)
            {
                ViewXml.Text = "Nothing was found. Please stop readiong this.";
            }
            else
            {
                ViewXml.Text = selected.Content;
            }
        }

        private void QueryPath_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
