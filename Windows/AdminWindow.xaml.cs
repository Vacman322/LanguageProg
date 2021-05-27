using LanguageProg.EF;
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
    /// Логика взаимодействия для AdminWindow.xaml
    /// </summary>
    public partial class AdminWindow : Window
    {
        private List<ClientsList> clients;
        List<string> countOfRecordsOnPage = new List<string> { "10", "50", "200", "Все" };
        int numberPage = 0;
        int maxRecordsCount;
        Dictionary<string, Func<List<ClientsList>, IEnumerable<ClientsList>>> sorting = new Dictionary<string, Func<List<ClientsList>, IEnumerable<ClientsList>>>
        {
            {"По ID", lst => lst.OrderBy(r => r.ID)},
            {"По фамилии", lst => lst.OrderBy(r => r.LastName)},
            {"По дате последнего посещения", lst => lst.OrderByDescending(r => r.LastEntry)},
            {"По количеству посещений", lst => lst.OrderByDescending(r => r.count)}
        };

        public AdminWindow()
        {
            InitializeComponent();

            Style = (Style)FindResource(typeof(Window));

            var genderList = DB.Context.Gender.Select(g => g.Name).ToList();
            genderList.Insert(0, "Все");
            GenderFilterComboBox.ItemsSource = genderList;
            CountOfRecordsComboBox.ItemsSource = countOfRecordsOnPage;
            SortingComboBox.ItemsSource = sorting.Keys;

            CountOfRecordsComboBox.SelectedIndex = 0;
            GenderFilterComboBox.SelectedIndex = 0;
            SortingComboBox.SelectedIndex = 0;
        }

        private int GetCountOfRecordsOnPage() =>
            GetStringCountOfRecordsOnPage() == "Все" ? -1 : int.Parse(GetStringCountOfRecordsOnPage());

        private string GetStringCountOfRecordsOnPage() =>
            (string)CountOfRecordsComboBox.SelectedItem;

        private void UpdateList(bool updateNumberPage = false)
        {
            DB.Context = new Context();
            if (updateNumberPage)
                numberPage = 0;

            if (GenderFilterComboBox.SelectedIndex == 0)
            {
                clients = DB.Context.ClientsList
                    .OrderBy(r => r.ID)
                    .ToList();
            }
            else
            {
                var gender = (string)GenderFilterComboBox.SelectedItem;
                clients = DB.Context.ClientsList
                    .Where(r => r.Name.Equals(gender))
                    .OrderBy(r => r.ID)
                    .ToList();
            }
            if (BirthDayCheckBox.IsChecked.HasValue && BirthDayCheckBox.IsChecked.Value)
            {
                clients = clients
                    .Where(r => r.DateOfBirth.Month == DateTime.Now.Month)
                    .ToList();
            }

            if (!string.IsNullOrEmpty(SearchTextBox.Text))
            {
                var str = SearchTextBox.Text;
                clients = clients
                    .Where(r => r.FirstName.Contains(str) ||
                    r.LastName.Contains(str) ||
                    r.Patronymic.Contains(str) ||
                    r.Email.Contains(str) ||
                    r.PhoneNumber.Contains(str))
                    .ToList();
            }

            maxRecordsCount = clients.Count;

            string sortingKay = string.IsNullOrEmpty((string)SortingComboBox.SelectedItem) ? "По ID" : (string)SortingComboBox.SelectedItem;
            if (GetStringCountOfRecordsOnPage().Equals("Все"))
                clients = sorting[sortingKay](clients)
                    .ToList();
            else
            {
                var countOfRecords = GetCountOfRecordsOnPage();
                clients = sorting[sortingKay](clients)
                    .Skip(numberPage * countOfRecords)
                    .Take(countOfRecords)
                    .ToList();
            }

            CountOfRecordsLable.Content = $"{clients.Count} из {maxRecordsCount}";
            AdminListView.ItemsSource = clients;
        }

        private void SelectionChangedCountOfRecordsComboBox(object sender, SelectionChangedEventArgs e)
        {
            UpdateList();
        }

        private int MaxPagesCount()
        {
            var countOfRecordsOnPage = GetCountOfRecordsOnPage();

            var result = maxRecordsCount / countOfRecordsOnPage;
            if (maxRecordsCount % countOfRecordsOnPage > 0)
                result++;

            return result;
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (MaxPagesCount() <= numberPage + 1)
                return;
            numberPage++;
            UpdateList();
        }

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            if (numberPage == 0)
                return;
            numberPage--;
            UpdateList();
        }

        private void SelectionChangedGenderFilterComboBox(object sender, SelectionChangedEventArgs e)
        {
            UpdateList(true);
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateList(true);
        }

        private void SortingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateList();
        }

        private void BirthDayCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            UpdateList(true);
        }

        private bool WarningWindowCall(string msg, string caption)
        {
            var mboxResult = MessageBox.Show(msg, caption, MessageBoxButton.YesNo, MessageBoxImage.Warning);
            return mboxResult == MessageBoxResult.Yes;
        }

        private void DelButton_Click(object sender, RoutedEventArgs e)
        {
            if (AdminListView.SelectedItem is null)
            {
                MessageBox.Show("Выберите запись для удаления");
                return;
            }

            var selected = (ClientsList)AdminListView.SelectedItem;
            if (selected.count > 0)
            {
                MessageBox.Show("Невозможно удалить запись т.к. у клиента есть информация о посещениях");
                return;
            }

            if (!WarningWindowCall("Вы точно хотите удалить запись?", "Удаление"))
                return;

            if (!string.IsNullOrEmpty(selected.Tags))
            {
                var TagClients = DB.Context.TagClient.Where(r => r.IDClient == selected.ID).ToList();
                foreach (var tagClient in TagClients)
                {
                    DB.Context.TagClient.Remove(tagClient);
                }
            }

            var selctedClient = DB.Context.Client.Where(r => r.ID == selected.ID).FirstOrDefault();
            DB.Context.Client.Remove(selctedClient);
            DB.Context.SaveChanges();
            UpdateList();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            new AddEditWindow().ShowDialog();
            UpdateList();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (AdminListView.SelectedItem is null)
            {
                MessageBox.Show("Выберите запись для редактирования");
                return;
            }

            var selected = (ClientsList)AdminListView.SelectedItem;
            new AddEditWindow(selected.ID).ShowDialog();
            UpdateList();
        }

        private void AdminListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TagsWrapPanel.Children.Clear();
            var selected = (ClientsList)AdminListView.SelectedItem;
            if (selected != null && !string.IsNullOrWhiteSpace(selected.Tags))
            {
                var id = selected.ID;
                var clientsTags = DB.Context.TagClient
                            .Where(r => r.IDClient == id)
                            .Select(r => r.Tag)
                            .ToList();
                foreach (var tag in clientsTags)
                {
                    var color = (Color)ColorConverter.ConvertFromString("#" + tag.Color);
                    var textBlock = new TextBlock()
                    {
                        Text = tag.Name,
                        Margin = new Thickness(2),
                        Padding = new Thickness(2),
                        Background = new SolidColorBrush(color),
                        Height = 25,
                        Foreground = (color.R + color.G + color.B) / 3 < 127 ? Brushes.White : Brushes.Black 
                    };

                    TagsWrapPanel.Children.Add(textBlock);
                }
            }
        }
    }
}
