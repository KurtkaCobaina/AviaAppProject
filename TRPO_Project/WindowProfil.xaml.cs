
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace TRPO_Project
{
    /// <summary>
    /// Логика взаимодействия для WindowProfil.xaml
    /// </summary>
    public partial class WindowProfil : Window
    {

        public WindowProfil()
        {
            var currentUser = UserSession.CurrentUser;
            InitializeComponent();
            if (currentUser != null)
            {
                // Устанавливаем имя пользователя в текстовый бокс
                Name.Text = currentUser.FirstName;
                LastName.Text = currentUser.LastName;
                UserEmail.Text = currentUser.Email;
                UserPhone.Text = currentUser.Phone;
            }
            else
            {
                Name.Text = "Пользователь не найден";
            }
            if (currentUser.Role != "Admin")
            {
                BtnAirPorts.IsEnabled = false;
                BtnFlights.IsEnabled = false;
                BtnFlights.Visibility = Visibility.Collapsed;
                BtnAirPorts.Visibility = Visibility.Collapsed;
               



            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Name.Text) || string.IsNullOrWhiteSpace(LastName.Text) || string.IsNullOrWhiteSpace(UserEmail.Text) || string.IsNullOrWhiteSpace(UserPhone.Text))
            {
                MessageBox.Show("Указаны не все данные");
                return;
            }
            if (!Regex.IsMatch(UserEmail.Text, @"^[A-Za-z][^@]+@[^@]+\.[^@]+$"))
            {
                MessageBox.Show("Пожалуйста, введите корректный E-mail.");
                return;
            }
            if (!Regex.IsMatch(UserPhone.Text, @"^\+7\d{10}$"))
            {
                MessageBox.Show("Номер должен начинаться с +7 и после содержать 10 цифр");
            }
            else
            {
                var context = TRPOEntities.GetContext();

                Users user = UserSession.CurrentUser;
                user.FirstName = Name.Text;
                user.LastName = LastName.Text;
                user.Email = UserEmail.Text;
                user.Phone = UserPhone.Text;

                context.Users.AddOrUpdate(user);

                context.SaveChanges();
                MessageBox.Show("Изменения сохранились");
            }
        }

        private void Button_ClickEx(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Вы вышли из системы", "Попейте чай");
            UserSession.CurrentUser = null;
            this.Close();
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }

        private void Button_Click1(object sender, RoutedEventArgs e)
        {
            var fc = new FlightControl();
            fc.Show();
            this.Close();
        }

        private void Button_Click2(object sender, RoutedEventArgs e)
        {
            var ac = new AirportControl();
            ac.Show();
            this.Close();
        }

        private void ExportBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnUsers_Click(object sender, RoutedEventArgs e)
        {
            var wu = new UserControl();
            wu.Show();
            this.Close();
        }

        private void BtnBookings_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BookingControl();
            bc.Show();
            this.Close();
        }

        private void BtnPayments_Click(object sender, RoutedEventArgs e)
        {
            var pc = new PaymentControl();
            pc.Show();
            this.Close();
        }
    }
}
