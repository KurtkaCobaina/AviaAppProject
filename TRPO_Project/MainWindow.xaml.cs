using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace TRPO_Project
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
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
            if (string.IsNullOrWhiteSpace(Username.Text) || string.IsNullOrWhiteSpace(Password.Password))
            {
                MessageBox.Show("Заполните все поля.");
                return;
            }

            using (TRPOEntities db = new TRPOEntities())
            {
                // Поиск пользователя по email
                var existingUser = db.Users.FirstOrDefault(u => u.Email == Username.Text);
                if (existingUser == null)
                {
                    MessageBox.Show("Пользователь с таким логином не существует.");
                    Username.Clear();
                    return;
                }

                // Хеширование введенного пароля
                string hashedPassword = GetHashString(Password.Password);

                // Проверка пароля
                if (existingUser.Password == hashedPassword)
                {
                    UserSession.CurrentUser = existingUser;
                    MessageBox.Show("Вход выполнен успешно!");

                    // Переход на страницу профиля
                    WindowProfil profil2 = new WindowProfil();
                    this.Close();
                    profil2.Show();
                }
                else
                {
                    MessageBox.Show("Неверный пароль.");
                    Password.Clear();
                }
            }
        }

        private void LabelRegistr_Click_1(object sender, RoutedEventArgs e)
        {
            // Открытие окна регистрации
            WindowRegistr windowRegistr = new WindowRegistr();
            this.Close();
            windowRegistr.Show();
        }
    }
}