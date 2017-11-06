using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using XmlDocumentTest.Models;
using XmlDocumentTest.Services;

namespace XmlDocumentTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainForm MainForm { get; set; }
        public ApplicationDbContext Db { get; set; }
        
        public MainWindow()
        {
            InitializeComponent();
            Db = new ApplicationDbContext();

            UpdateList();
            NewForm();
        }

        private void UpdateList()
        {
            IList<Tuple<int,string>> list = Db.MainForms.ToList().Select(m => new Tuple<int, string>(m.Id, m.Id + ". " + m.XmlContent.LastName.Content + ", " + m.XmlContent.FirstName.Content)).ToList();
            Entries.ItemsSource = list;
        }

        private void NewForm()
        {
            MainForm = new MainForm();
            Form test = MainForm.XmlContent;

            this.DataContext = MainForm;
        }

        private void SelectForm(int id)
        {
            MainForm = Db.MainForms.Find(id);
            this.DataContext = MainForm;
        }

        private void NewEntry_Click(object sender, RoutedEventArgs e)
        {
            NewForm();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            MainForm.Update();

            if(MainForm.Id > 0)
            {
                Db.Entry(MainForm).State = System.Data.Entity.EntityState.Modified;
            }
            else
            {
                Db.MainForms.Add(MainForm);
            }

            Db.SaveChanges();
            SolrService.RequestSolrUpdate();
        }

        private void Entries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Tuple<int, string> item = (Tuple<int, string>)Entries.SelectedItem;

            if(item == null)
            {
                NewForm();
            }
            else
            {
                SelectForm(item.Item1);
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchWindow search = new SearchWindow();
            search.ShowDialog();
        }
    }
}
