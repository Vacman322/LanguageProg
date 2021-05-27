using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
using LanguageProg.EF;

namespace LanguageProg
{
    /// <summary>
    /// Interaction logic for AddEditWindow.xaml
    /// </summary>
    public partial class AddEditWindow : Window
    {
        Client client;
        bool isEdit = true;
        bool isPhotoEdit = false;
        string photoName;
        public AddEditWindow(int ClientId = -1)
        {
            InitializeComponent();
            GenderComboBox.ItemsSource = DB.Context.Gender
                .Select(r => r.ID)
                .ToList();
            if (ClientId != -1)
            {
                client = DB.Context.Client.Where(r => r.ID == ClientId).FirstOrDefault();
                IdTextBox.Text = client.ID.ToString();
                LastNameTextBox.Text = client.LastName;
                FirstNameTextBox.Text = client.FirstName;
                PatronymicTextBox.Text = client.Patronymic;
                EmailTextBox.Text = client.Email;
                PhoneTextBox.Text = client.PhoneNumber;
                BirthDayDatePicker.SelectedDate = client.DateOfBirth;
                GenderComboBox.SelectedItem = client.IDGender;
                if (!(client.Photo is null))
                {
                    var a = new Uri(FindDirectoryForSave() + "\\" + client.Photo);
                    ClientImage.Source = new BitmapImage(a);
                }
            }
            else
            {
                isEdit = false;
                client = new Client();
                IdLabel.Visibility = Visibility.Hidden;
                IdTextBox.Visibility = Visibility.Hidden;
            }

            var tags = DB.Context.Tag;
            var id = client.ID;
            var clientsTags = DB.Context.TagClient
                        .Where(r => r.IDClient == id)
                        .Select(r => r.IDTag)
                        .ToList();

            foreach (var tag in tags)
            {
                var cb = new CheckBox()
                {
                    Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#" + tag.Color)),
                    Content = tag.Name               
                };

                
                TagsWrapPanel.Children.Add(cb);
                if (isEdit)
                {

                    if (clientsTags.Contains(tag.ID))
                    {
                        cb.IsChecked = true;
                    }
                }
                cb.Checked += Cb_Checked;
                cb.Unchecked += Cb_Unchecked;
            }
        }

        private void Cb_Unchecked(object sender, RoutedEventArgs e)
        {
            var checkBox = (CheckBox)sender;
            string tagName = checkBox.Content.ToString();
            var tagId = DB.Context.Tag.FirstOrDefault(r => r.Name.Equals(tagName)).ID;
            var tagForDel = DB.Context.TagClient.FirstOrDefault(r => r.IDClient == client.ID && r.IDTag == tagId);
            DB.Context.TagClient.Remove(tagForDel);
        }

        private void Cb_Checked(object sender, RoutedEventArgs e)
        {
            var checkBox = (CheckBox)sender;
            string tagName = checkBox.Content.ToString();
            var tagId = DB.Context.Tag.FirstOrDefault(r => r.Name.Equals(tagName)).ID;
            var tagClient = new TagClient() { IDClient = client.ID, IDTag = tagId };
            DB.Context.TagClient.Add(tagClient);
        }

        public bool IsValidEmail(string emailaddress)
        {
            try
            {
                System.Net.Mail.MailAddress m = new System.Net.Mail.MailAddress(emailaddress);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
        private bool WarningWindowCall(string msg, string caption)
        {
            var mboxResult = MessageBox.Show(msg, caption, MessageBoxButton.YesNo, MessageBoxImage.Warning);
            return mboxResult == MessageBoxResult.Yes;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var textboxesTypes = this.GetType()
                .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(t => t.FieldType == typeof(TextBox));

            foreach (var textboxesType in textboxesTypes)
            {
                var tb = (TextBox)textboxesType.GetValue(this);
                if (!tb.IsVisible)
                    continue;
                if (string.IsNullOrWhiteSpace(tb.Text))
                {
                    MessageBox.Show("Заполните все поля");
                    return;
                }
            }
            
            if(GenderComboBox.SelectedItem is null || BirthDayDatePicker.SelectedDate is null)
            {
                MessageBox.Show("Заполните все поля");
                return;
            }

            var fio = string.Join(" ", LastNameTextBox.Text, FirstNameTextBox.Text, PatronymicTextBox.Text);
            if (!fio.Select(s => char.IsLetter(s) || s == ' ' || s == '-').Aggregate((b1, b2) => b1 && b2))
            {
                MessageBox.Show("ФИО могут содержать в себе только буквы и следующие символы: пробел и дефис");
                return;
            }

            if (LastNameTextBox.Text.Length > 50 ||
                FirstNameTextBox.Text.Length > 50 ||
                PatronymicTextBox.Text.Length > 50)
            {
                MessageBox.Show("Поля фамилии, имени и отчества не могут быть длиннее 50 символов.");
                return;
            }

            if (!IsValidEmail(EmailTextBox.Text))
            {
                MessageBox.Show("Не правильный email");
                return;
            }

            var validPhoneSybls = new HashSet<char>()
            {
                '+', '-', '(', ')', ' '
            };

            if (!PhoneTextBox.Text.Select(s => char.IsDigit(s) || validPhoneSybls.Contains(s)).Aggregate((b1, b2) => b1 && b2))
            {
                MessageBox.Show("телефона может содержать только цифры и следующие символы: плюс, минус, открывающая и закрывающая круглые скобки, знак пробела");
                return;
            }

            if (!WarningWindowCall("Вы точно хотите сохранить?", "Сохранение"))
                return;

            client.LastName = LastNameTextBox.Text;
            client.FirstName = FirstNameTextBox.Text;
            client.Patronymic = PatronymicTextBox.Text;
            client.Email = EmailTextBox.Text;
            client.PhoneNumber = PhoneTextBox.Text;
            client.DateOfBirth = BirthDayDatePicker.DisplayDate;
            client.IDGender = (string)GenderComboBox.SelectedItem;
            if(isPhotoEdit)
            {
                client.Photo = "Clients\\" + photoName;
            }

            if(isEdit)
            {
                DB.Context.SaveChanges();
            }
            else
            {
                client.DateOfRegistration = DateTime.Now;
                DB.Context.Client.Add(client);
                DB.Context.SaveChanges();
            }
            Close();
        }

        private string FindDirectoryForSave()
        {
            var currentDir = new DirectoryInfo(Environment.CurrentDirectory);
            var resDir = currentDir.Parent.Parent.GetDirectories()
                .FirstOrDefault(d => d.Name.Equals("Resources"));
            if (resDir is null)
                return Environment.CurrentDirectory;

            return resDir.FullName;
        }

        private void СhangeImgButoon_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new Microsoft.Win32.OpenFileDialog();
            fileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;";

            bool? result = fileDialog.ShowDialog();
            if(result.HasValue && result.Value)
            {
                var fileInfo = new FileInfo(fileDialog.FileName);
                if(fileInfo.Length > 2 * 1024 * 1024)
                {
                    MessageBox.Show("Размер фотографии не должен превышать 2 мегабайта.");
                    return;
                }
                ClientImage.Source = new BitmapImage(new Uri(fileDialog.FileName));
                

                fileInfo.CopyTo(FindDirectoryForSave() + "\\Clients\\" + fileInfo.Name, true);
                isPhotoEdit = true;
                photoName = fileInfo.Name;
            }
        }

        private void VisitsButton_Click(object sender, RoutedEventArgs e)
        {
            new ClientVisitsWindow(client.ID).ShowDialog();
        }
    }
}
