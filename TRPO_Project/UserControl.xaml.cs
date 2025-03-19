using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using OfficeOpenXml;
using OfficeOpenXml.Core.ExcelPackage;
using System.IO;
using ClosedXML.Excel;
using Microsoft.Win32;
using Xceed.Document.NET;
using Xceed.Words.NET;

namespace TRPO_Project
{
    /// <summary>
    /// Логика взаимодействия для UserControl.xaml
    /// </summary>
    public partial class UserControl : Window
    {
        private Users selectedUser;
        private TRPOEntities context;

        public UserControl()
        {
            InitializeComponent();
            context = new TRPOEntities();
            LoadUsers();
        }
        private void LoadUsers()
        {
            var users = context.Users.ToList();
            UsersListBox.ItemsSource = users;
        }
        private void UsersListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            selectedUser = UsersListBox.SelectedItem as Users;

            if (selectedUser != null)
            {
                FirstNameTextBox.Text = selectedUser.FirstName;
                LastNameTextBox.Text = selectedUser.LastName;
                EmailTextBox.Text = selectedUser.Email;
                PhoneTextBox.Text = selectedUser.Phone;
                PasswordTextBox.Text = selectedUser.Password;
                DateOfBirthTextBox.Text = selectedUser.DateOfBirth?.ToString("yyyy-MM-dd");
                RoleTextBox.Text = selectedUser.Role;
            }
        }
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateFields())
            {
                MessageBox.Show("Заполните все поля.");
                return;
            }

            var newUser = new Users
            {
                FirstName = FirstNameTextBox.Text,
                LastName = LastNameTextBox.Text,
                Email = EmailTextBox.Text,
                Phone = PhoneTextBox.Text,
                Password = PasswordTextBox.Text,
                DateOfBirth = DateTime.TryParse(DateOfBirthTextBox.Text, out DateTime dob) ? dob : (DateTime?)null,
                Role = RoleTextBox.Text
            };

            context.Users.Add(newUser);
            context.SaveChanges();

            ClearFields();
            LoadUsers();
            MessageBox.Show("Пользователь успешно добавлен!");
        }
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedUser == null)
            {
                MessageBox.Show("Выберите пользователя для изменения.");
                return;
            }

            if (!ValidateFields())
            {
                MessageBox.Show("Заполните все поля.");
                return;
            }

            var userToUpdate = context.Users.FirstOrDefault(u => u.UserID == selectedUser.UserID);

            if (userToUpdate != null)
            {
                userToUpdate.FirstName = FirstNameTextBox.Text;
                userToUpdate.LastName = LastNameTextBox.Text;
                userToUpdate.Email = EmailTextBox.Text;
                userToUpdate.Phone = PhoneTextBox.Text;
                userToUpdate.Password = PasswordTextBox.Text;
                userToUpdate.DateOfBirth = DateTime.TryParse(DateOfBirthTextBox.Text, out DateTime dob) ? dob : (DateTime?)null;
                userToUpdate.Role = RoleTextBox.Text;

                context.SaveChanges();
            }

            ClearFields();
            LoadUsers();
            MessageBox.Show("Пользователь успешно изменен!");
        }
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedUser == null)
            {
                MessageBox.Show("Выберите пользователя для удаления.");
                return;
            }

            var userToDelete = context.Users.FirstOrDefault(u => u.UserID == selectedUser.UserID);

            if (userToDelete != null)
            {
                context.Users.Remove(userToDelete);
                context.SaveChanges();
            }

            ClearFields();
            LoadUsers();
            MessageBox.Show("Пользователь успешно удален!");
        }

        private void ClearFields()
        {
            FirstNameTextBox.Clear();
            LastNameTextBox.Clear();
            EmailTextBox.Clear();
            PhoneTextBox.Clear();
            PasswordTextBox.Clear();
            DateOfBirthTextBox.Clear();
            RoleTextBox.Clear();

            selectedUser = null;
        }
        private bool ValidateFields()
        {
            if (string.IsNullOrWhiteSpace(FirstNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(LastNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(EmailTextBox.Text) ||
                string.IsNullOrWhiteSpace(PhoneTextBox.Text) ||
                string.IsNullOrWhiteSpace(PasswordTextBox.Text) ||
                string.IsNullOrWhiteSpace(RoleTextBox.Text))
            {
                return false;
            }

            if (!DateTime.TryParse(DateOfBirthTextBox.Text, out _))
            {
                MessageBox.Show("Дата рождения должна быть в формате yyyy-MM-dd.");
                return false;
            }

            return true;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            context.Dispose();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var back = new WindowProfil();

            back.Show();
            this.Close();
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog ofd = new SaveFileDialog()
            {
                DefaultExt = "xlsx",
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                Title = "Выберите место для сохранения файла"
            };

            if (!(ofd.ShowDialog() == true))
                return;

            var wb = new XLWorkbook();
            using (var context = TRPOEntities.GetContext()) // Предполагается, что контекст данных называется TRPO_ProjectEntities
            {
                var users = context.Users.ToList();

                // Группировка пользователей по роли (Role)
                var groupedByRole = users.GroupBy(u => u.Role ?? "Unknown");

                foreach (var group in groupedByRole)
                {
                    // Создание листа для каждой роли
                    var roleSheet = wb.Worksheets.Add(group.Key);

                    // Заголовки столбцов
                    roleSheet.Cell(1, 1).Value = "ID";
                    roleSheet.Cell(1, 2).Value = "First Name";
                    roleSheet.Cell(1, 3).Value = "Last Name";
                    roleSheet.Cell(1, 4).Value = "Email";
                    roleSheet.Cell(1, 5).Value = "Phone";
                    roleSheet.Cell(1, 6).Value = "Date of Birth";
                    roleSheet.Cell(1, 7).Value = "Role";

                    // Заполнение данных
                    for (int i = 0; i < group.Count(); i++)
                    {
                        var user = group.ElementAt(i);
                        roleSheet.Cell(i + 2, 1).Value = user.UserID;
                        roleSheet.Cell(i + 2, 2).Value = user.FirstName;
                        roleSheet.Cell(i + 2, 3).Value = user.LastName;
                        roleSheet.Cell(i + 2, 4).Value = user.Email;
                        roleSheet.Cell(i + 2, 5).Value = user.Phone;
                        roleSheet.Cell(i + 2, 6).Value = user.DateOfBirth?.ToString("yyyy-MM-dd"); // Форматирование даты
                        roleSheet.Cell(i + 2, 7).Value = user.Role;
                    }
                }
            }

            try
            {
                // Сохранение файла
                wb.SaveAs(ofd.FileName);
                MessageBox.Show("Файл успешно экспортирован!", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchText = SearchTextBox.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(searchText))
            {
                LoadUsers(); // Если строка поиска пустая, загружаем всех пользователей
                return;
            }

            var filteredUsers = context.Users
                .Where(u => u.FirstName.ToLower().Contains(searchText) ||
                            u.LastName.ToLower().Contains(searchText) ||
                            u.Email.ToLower().Contains(searchText))
                .ToList();

            UsersListBox.ItemsSource = filteredUsers;
        }
    }
    }
