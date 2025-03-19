using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace TRPO_Project
{
    /// <summary>
    /// Логика взаимодействия для WindowRegistr.xaml
    /// </summary>
    public partial class WindowRegistr : Window
    {
        private readonly Users _currentObject = new Users();
        private readonly TRPOEntities _bd = new TRPOEntities();

        public WindowRegistr()
        {
            InitializeComponent();
        }

        private string GetHashString(string s)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(s));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Проверка на пустые поля
            if (string.IsNullOrWhiteSpace(Name.Text) ||
                string.IsNullOrWhiteSpace(Surname.Text) ||
                string.IsNullOrWhiteSpace(Email.Text) ||
                string.IsNullOrWhiteSpace(PhoneNum.Text) ||
                string.IsNullOrWhiteSpace(BirthDate.Text) ||
                string.IsNullOrWhiteSpace(Password.Password))
            {
                MessageBox.Show("Указаны не все данные");
                return;
            }

            // Проверка формата email
            if (!Regex.IsMatch(Email.Text, @"^[A-Za-z][^@]+@[^@]+\.[^@]+$"))
            {
                MessageBox.Show("Пожалуйста, введите корректный E-mail.");
                return;
            }

            // Проверка формата номера телефона
            if (!Regex.IsMatch(PhoneNum.Text, @"^\+7\d{10}$"))
            {
                MessageBox.Show("Номер должен начинаться с +7 и после содержать 10 цифр");
                return;
            }

            // Проверка формата даты рождения
            if (!DateTime.TryParse(BirthDate.Text, out DateTime parsedDate))
            {
                MessageBox.Show("Пожалуйста, введите корректную дату рождения в формате дд.мм.гггг.");
                return;
            }

            // Проверка существования пользователя с таким email
            using (TRPOEntities db = new TRPOEntities())
            {
                var existingUser = db.Users.FirstOrDefault(u => u.Email == Email.Text);
                if (existingUser != null)
                {
                    MessageBox.Show("Пользователь с таким логином уже существует.");
                    Email.Clear();
                    return;
                }

                // Заполнение объекта пользователя
                _currentObject.FirstName = Name.Text;
                _currentObject.LastName = Surname.Text;
                _currentObject.Email = Email.Text;
                _currentObject.Phone = PhoneNum.Text;
                _currentObject.Password = GetHashString(Password.Password);
                _currentObject.DateOfBirth = parsedDate; // Присваиваем преобразованную дату
                _currentObject.Role = "Admin";

                // Добавление пользователя в базу данных
                TRPOEntities.GetContext().Users.Add(_currentObject);
                TRPOEntities.GetContext().SaveChanges();

                MessageBox.Show("Регистрация прошла успешно");

                // Переход на главную страницу
                MainWindow windowLogin = new MainWindow();
                windowLogin.Show();
                this.Close();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MainWindow windowLogin = new MainWindow();
            windowLogin.Show();
            this.Close();
        }
    }
}