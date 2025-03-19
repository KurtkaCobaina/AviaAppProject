using ClosedXML.Excel;
using Microsoft.Win32;
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

namespace TRPO_Project
{
    /// <summary>
    /// Логика взаимодействия для BookingControl.xaml
    /// </summary>
    public partial class BookingControl : Window
    {
        private Bookings selectedBooking;
        private TRPOEntities context;

        public BookingControl()
        {
            InitializeComponent();
            context = new TRPOEntities();
            LoadBookings();
        }

        private void LoadBookings()
        {
            var bookings = context.Bookings
                .Include("Flights") // Используем строку вместо лямбда-выражения
                .ToList();
            BookingsListBox.ItemsSource = bookings;
        }

        private void BookingsListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            selectedBooking = BookingsListBox.SelectedItem as Bookings;

            if (selectedBooking != null)
            {
                UserIDTextBox.Text = selectedBooking.UserID?.ToString() ?? string.Empty;
                FlightIDTextBox.Text = selectedBooking.FlightID?.ToString() ?? string.Empty;
                BookingDateTextBox.Text = selectedBooking.BookingDate?.ToString("yyyy-MM-dd") ?? string.Empty;
                StatusTextBox.Text = selectedBooking.Status ?? string.Empty;
                AmountTextBox.Text = selectedBooking.Amount?.ToString() ?? string.Empty;
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateFields())
            {
                MessageBox.Show("Заполните все поля.");
                return;
            }

            var newBooking = new Bookings
            {
                UserID = int.TryParse(UserIDTextBox.Text, out int userId) ? (int?)userId : null,
                FlightID = int.TryParse(FlightIDTextBox.Text, out int flightId) ? (int?)flightId : null,
                BookingDate = DateTime.TryParse(BookingDateTextBox.Text, out DateTime bookingDate) ? (DateTime?)bookingDate : null,
                Status = StatusTextBox.Text,
                Amount = int.TryParse(AmountTextBox.Text, out int amount) ? (int?)amount : null
            };

            context.Bookings.Add(newBooking);
            context.SaveChanges();

            ClearFields();
            LoadBookings();
            MessageBox.Show("Бронирование успешно добавлено!");
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedBooking == null)
            {
                MessageBox.Show("Выберите бронирование для изменения.");
                return;
            }

            if (!ValidateFields())
            {
                MessageBox.Show("Заполните все поля.");
                return;
            }

            var bookingToUpdate = context.Bookings.FirstOrDefault(b => b.BookingID == selectedBooking.BookingID);

            if (bookingToUpdate != null)
            {
                bookingToUpdate.UserID = int.TryParse(UserIDTextBox.Text, out int userId) ? (int?)userId : null;
                bookingToUpdate.FlightID = int.TryParse(FlightIDTextBox.Text, out int flightId) ? (int?)flightId : null;
                bookingToUpdate.BookingDate = DateTime.TryParse(BookingDateTextBox.Text, out DateTime bookingDate) ? (DateTime?)bookingDate : null;
                bookingToUpdate.Status = StatusTextBox.Text;
                bookingToUpdate.Amount = int.TryParse(AmountTextBox.Text, out int amount) ? (int?)amount : null;

                context.SaveChanges();
            }

            ClearFields();
            LoadBookings();
            MessageBox.Show("Бронирование успешно изменено!");
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedBooking == null)
            {
                MessageBox.Show("Выберите бронирование для удаления.");
                return;
            }

            var bookingToDelete = context.Bookings.FirstOrDefault(b => b.BookingID == selectedBooking.BookingID);

            if (bookingToDelete != null)
            {
                context.Bookings.Remove(bookingToDelete);
                context.SaveChanges();
            }

            ClearFields();
            LoadBookings();
            MessageBox.Show("Бронирование успешно удалено!");
        }

        private void ClearFields()
        {
            UserIDTextBox.Clear();
            FlightIDTextBox.Clear();
            BookingDateTextBox.Clear();
            StatusTextBox.Clear();
            AmountTextBox.Clear();

            selectedBooking = null;
        }

        private bool ValidateFields()
        {
            if (string.IsNullOrWhiteSpace(UserIDTextBox.Text) ||
                string.IsNullOrWhiteSpace(FlightIDTextBox.Text) ||
                string.IsNullOrWhiteSpace(StatusTextBox.Text) ||
                string.IsNullOrWhiteSpace(AmountTextBox.Text))
            {
                return false;
            }

            if (!int.TryParse(UserIDTextBox.Text, out _) ||
                !int.TryParse(FlightIDTextBox.Text, out _) ||
                !int.TryParse(AmountTextBox.Text, out _))
            {
                MessageBox.Show("UserID, FlightID и Amount должны быть числами.");
                return false;
            }

            if (!DateTime.TryParse(BookingDateTextBox.Text, out _))
            {
                MessageBox.Show("Дата бронирования должна быть в формате yyyy-MM-dd.");
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
                // Загружаем бронирования вместе с данными о связанных пользователях и рейсах
                var bookings = context.Bookings
                    .Include("Users") // Подключаем данные о пользователе
                    .Include("Flights") // Подключаем данные о рейсе
                    .ToList();

                // Создание одного листа для всех бронирований
                var sheet = wb.Worksheets.Add("Bookings");

                // Заголовки столбцов
                sheet.Cell(1, 1).Value = "Booking ID";
                sheet.Cell(1, 2).Value = "User Name";
                sheet.Cell(1, 3).Value = "Flight Number";
                sheet.Cell(1, 4).Value = "Booking Date";
                sheet.Cell(1, 5).Value = "Status";
                sheet.Cell(1, 6).Value = "Amount";

                // Заполнение данных
                for (int i = 0; i < bookings.Count; i++)
                {
                    var booking = bookings[i];
                    sheet.Cell(i + 2, 1).Value = booking.BookingID;
                    sheet.Cell(i + 2, 2).Value = $"{booking.Users?.FirstName ?? "Unknown"} {booking.Users?.LastName ?? "Unknown"}"; // Имя и фамилия пользователя
                    sheet.Cell(i + 2, 3).Value = booking.Flights?.FlightNumber ?? "Unknown"; // Номер рейса
                    sheet.Cell(i + 2, 4).Value = booking.BookingDate?.ToString("yyyy-MM-dd HH:mm"); // Форматирование даты
                    sheet.Cell(i + 2, 5).Value = booking.Status;
                    sheet.Cell(i + 2, 6).Value = booking.Amount;
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
        private void SearchByFlightNumberButton_Click(object sender, RoutedEventArgs e)
        {
            string searchFlightNumber = SearchFlightNumberTextBox.Text.Trim();

            if (string.IsNullOrEmpty(searchFlightNumber))
            {
                LoadBookings(); // Если строка поиска пустая, загружаем все бронирования
                return;
            }

            var filteredBookings = context.Bookings
                .Include("Flights") // Подключаем данные о рейсах
                .Where(b => b.Flights != null && b.Flights.FlightNumber.Contains(searchFlightNumber))
                .ToList();

            BookingsListBox.ItemsSource = filteredBookings;
        }
    }
}