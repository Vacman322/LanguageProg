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
using System.Windows.Shapes;

namespace LanguageProg
{
    /// <summary>
    /// Interaction logic for CustomerVisitsWindow.xaml
    /// </summary>
    public partial class ClientVisitsWindow : Window
    {
        public ClientVisitsWindow(int idClient)
        {
            InitializeComponent();

            var visits = DB.Context.ClientService
                .Where(r => r.IDClient == idClient)
                .ToList();

            foreach (var visit in visits)
            {
                var border = new Border()
                {
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    Margin = new Thickness(5)
                };
                var sp = new StackPanel();
                VisitsStackPanel.Children.Add(border);
                border.Child = sp;
                sp.Children.Add(new TextBlock() { Text = visit.Service.Name });
                sp.Children.Add(new TextBlock() { Text = visit.StartDateTime.ToString("yyyy.mm.dd hh:mm") });
                sp.Children.Add(new TextBlock() { 
                    Text = "Всего файлов: " + 
                    DB.Context
                    .PhotoService.Where(r => r.IDService == visit.IDService)
                    .Count().ToString()
                });
            }
        }
    }
}
