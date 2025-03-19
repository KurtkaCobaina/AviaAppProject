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
    /// Логика взаимодействия для FlightControl.xaml
    /// </summary>
    public partial class FlightControl : Window
    {
        private Flights selectedFlight;
        private TRPOEntities context;

        public FlightControl()
        {
            InitializeComponent();
            context = new TRPOEntities();
            LoadFlights();
        }

        private void LoadFlights()
        {
            var flights = context.Flights.ToList();
            FlightsListBox.ItemsSource = flights;
        }

        private void FlightsListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            selectedFlight = FlightsListBox.SelectedItem as Flights;

            if (selectedFlight != null)
            {
                FlightNumberTextBox.Text = selectedFlight.FlightNumber ?? string.Empty;
                DepartureAirportIDTextBox.Text = selectedFlight.DepartureAirportID?.ToString() ?? string.Empty;
                ArrivalAirportIDTextBox.Text = selectedFlight.ArrivalAirportID?.ToString() ?? string.Empty;
                DepartureTimeTextBox.Text = selectedFlight.DepartureTime?.ToString("yyyy-MM-dd HH:mm") ?? string.Empty;
                ArrivalTimeTextBox.Text = selectedFlight.ArrivalTime?.ToString("yyyy-MM-dd HH:mm") ?? string.Empty;
                PriceTextBox.Text = selectedFlight.Price?.ToString() ?? string.Empty;
                SeatsAvailableTextBox.Text = selectedFlight.SeatsAvailable?.ToString() ?? string.Empty;
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateFields())
            {
                MessageBox.Show("Заполните все поля.");
                return;
            }

            var newFlight = new Flights
            {
                FlightNumber = FlightNumberTextBox.Text,
                DepartureAirportID = int.TryParse(DepartureAirportIDTextBox.Text, out int departureAirportId) ? (int?)departureAirportId : null,
                ArrivalAirportID = int.TryParse(ArrivalAirportIDTextBox.Text, out int arrivalAirportId) ? (int?)arrivalAirportId : null,
                DepartureTime = DateTime.TryParse(DepartureTimeTextBox.Text, out DateTime departureTime) ? (DateTime?)departureTime : null,
                ArrivalTime = DateTime.TryParse(ArrivalTimeTextBox.Text, out DateTime arrivalTime) ? (DateTime?)arrivalTime : null,
                Price = int.TryParse(PriceTextBox.Text, out int price) ? (int?)price : null,
                SeatsAvailable = int.TryParse(SeatsAvailableTextBox.Text, out int seatsAvailable) ? (int?)seatsAvailable : null
            };

            context.Flights.Add(newFlight);
            context.SaveChanges();

            ClearFields();
            LoadFlights();
            MessageBox.Show("Рейс успешно добавлен!");
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedFlight == null)
            {
                MessageBox.Show("Выберите рейс для изменения.");
                return;
            }

            if (!ValidateFields())
            {
                MessageBox.Show("Заполните все поля.");
                return;
            }

            var flightToUpdate = context.Flights.FirstOrDefault(f => f.FlightID == selectedFlight.FlightID);

            if (flightToUpdate != null)
            {
                flightToUpdate.FlightNumber = FlightNumberTextBox.Text;
                flightToUpdate.DepartureAirportID = int.TryParse(DepartureAirportIDTextBox.Text, out int departureAirportId) ? (int?)departureAirportId : null;
                flightToUpdate.ArrivalAirportID = int.TryParse(ArrivalAirportIDTextBox.Text, out int arrivalAirportId) ? (int?)arrivalAirportId : null;
                flightToUpdate.DepartureTime = DateTime.TryParse(DepartureTimeTextBox.Text, out DateTime departureTime) ? (DateTime?)departureTime : null;
                flightToUpdate.ArrivalTime = DateTime.TryParse(ArrivalTimeTextBox.Text, out DateTime arrivalTime) ? (DateTime?)arrivalTime : null;
                flightToUpdate.Price = int.TryParse(PriceTextBox.Text, out int price) ? (int?)price : null;
                flightToUpdate.SeatsAvailable = int.TryParse(SeatsAvailableTextBox.Text, out int seatsAvailable) ? (int?)seatsAvailable : null;

                context.SaveChanges();
            }

            ClearFields();
            LoadFlights();
            MessageBox.Show("Рейс успешно изменен!");
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedFlight == null)
            {
                MessageBox.Show("Выберите рейс для удаления.");
                return;
            }

            var flightToDelete = context.Flights.FirstOrDefault(f => f.FlightID == selectedFlight.FlightID);

            if (flightToDelete != null)
            {
                context.Flights.Remove(flightToDelete);
                context.SaveChanges();
            }

            ClearFields();
            LoadFlights();
            MessageBox.Show("Рейс успешно удален!");
        }

        private void ClearFields()
        {
            FlightNumberTextBox.Clear();
            DepartureAirportIDTextBox.Clear();
            ArrivalAirportIDTextBox.Clear();
            DepartureTimeTextBox.Clear();
            ArrivalTimeTextBox.Clear();
            PriceTextBox.Clear();
            SeatsAvailableTextBox.Clear();

            selectedFlight = null;
        }

        private bool ValidateFields()
        {
            if (string.IsNullOrWhiteSpace(FlightNumberTextBox.Text) ||
                string.IsNullOrWhiteSpace(DepartureAirportIDTextBox.Text) ||
                string.IsNullOrWhiteSpace(ArrivalAirportIDTextBox.Text) ||
                string.IsNullOrWhiteSpace(DepartureTimeTextBox.Text) ||
                string.IsNullOrWhiteSpace(ArrivalTimeTextBox.Text) ||
                string.IsNullOrWhiteSpace(PriceTextBox.Text) ||
                string.IsNullOrWhiteSpace(SeatsAvailableTextBox.Text))
            {
                return false;
            }

            if (!int.TryParse(DepartureAirportIDTextBox.Text, out _) ||
                !int.TryParse(ArrivalAirportIDTextBox.Text, out _) ||
                !int.TryParse(PriceTextBox.Text, out _) ||
                !int.TryParse(SeatsAvailableTextBox.Text, out _))
            {
                MessageBox.Show("DepartureAirportID, ArrivalAirportID, Price и SeatsAvailable должны быть числами.");
                return false;
            }

            if (!DateTime.TryParse(DepartureTimeTextBox.Text, out _) ||
                !DateTime.TryParse(ArrivalTimeTextBox.Text, out _))
            {
                MessageBox.Show("Время отправления и прибытия должно быть в формате yyyy-MM-dd HH:mm.");
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
                // Загружаем рейсы вместе с данными о связанных аэропортах
                var flights = context.Flights
                    .Include("Airports") // Подключаем данные о вылетающем аэропорте
                    .Include("Airports1") // Подключаем данные о прилетающем аэропорте
                    .ToList();

                // Создание одного листа для всех рейсов
                var sheet = wb.Worksheets.Add("Flights");

                // Заголовки столбцов
                sheet.Cell(1, 1).Value = "Flight ID";
                sheet.Cell(1, 2).Value = "Flight Number";
                sheet.Cell(1, 3).Value = "Departure Airport";
                sheet.Cell(1, 4).Value = "Arrival Airport";
                sheet.Cell(1, 5).Value = "Departure Time";
                sheet.Cell(1, 6).Value = "Arrival Time";
                sheet.Cell(1, 7).Value = "Price";
                sheet.Cell(1, 8).Value = "Seats Available";

                // Заполнение данных
                for (int i = 0; i < flights.Count; i++)
                {
                    var flight = flights[i];
                    sheet.Cell(i + 2, 1).Value = flight.FlightID;
                    sheet.Cell(i + 2, 2).Value = flight.FlightNumber;
                    sheet.Cell(i + 2, 3).Value = flight.Airports?.Name ?? "Unknown"; // Название вылетающего аэропорта
                    sheet.Cell(i + 2, 4).Value = flight.Airports1?.Name ?? "Unknown"; // Название прилетающего аэропорта
                    sheet.Cell(i + 2, 5).Value = flight.DepartureTime?.ToString("yyyy-MM-dd HH:mm"); // Форматирование времени
                    sheet.Cell(i + 2, 6).Value = flight.ArrivalTime?.ToString("yyyy-MM-dd HH:mm"); // Форматирование времени
                    sheet.Cell(i + 2, 7).Value = flight.Price;
                    sheet.Cell(i + 2, 8).Value = flight.SeatsAvailable;
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
    }
}