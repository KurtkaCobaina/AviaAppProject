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
    /// Логика взаимодействия для AirportControl.xaml
    /// </summary>
    public partial class AirportControl : Window
    {
        private Airports selectedAirport;
        private TRPOEntities context;

        public AirportControl()
        {
            InitializeComponent();
            context = new TRPOEntities();
            LoadAirports();
        }

        private void LoadAirports()
        {
            var airports = context.Airports.ToList();
            AirportsListBox.ItemsSource = airports;
        }

        private void AirportsListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            selectedAirport = AirportsListBox.SelectedItem as Airports;

            if (selectedAirport != null)
            {
                NameTextBox.Text = selectedAirport.Name;
                CityTextBox.Text = selectedAirport.City;
                CountryTextBox.Text = selectedAirport.Country;
                CodeTextBox.Text = selectedAirport.Code;
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateFields())
            {
                MessageBox.Show("Заполните все поля.");
                return;
            }

            var newAirport = new Airports
            {
                Name = NameTextBox.Text,
                City = CityTextBox.Text,
                Country = CountryTextBox.Text,
                Code = CodeTextBox.Text
            };

            context.Airports.Add(newAirport);
            context.SaveChanges();

            ClearFields();
            LoadAirports();
            MessageBox.Show("Аэропорт успешно добавлен!");
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedAirport == null)
            {
                MessageBox.Show("Выберите аэропорт для изменения.");
                return;
            }

            if (!ValidateFields())
            {
                MessageBox.Show("Заполните все поля.");
                return;
            }

            var airportToUpdate = context.Airports.FirstOrDefault(a => a.AirportID == selectedAirport.AirportID);

            if (airportToUpdate != null)
            {
                airportToUpdate.Name = NameTextBox.Text;
                airportToUpdate.City = CityTextBox.Text;
                airportToUpdate.Country = CountryTextBox.Text;
                airportToUpdate.Code = CodeTextBox.Text;

                context.SaveChanges();
            }

            ClearFields();
            LoadAirports();
            MessageBox.Show("Аэропорт успешно изменен!");
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedAirport == null)
            {
                MessageBox.Show("Выберите аэропорт для удаления.");
                return;
            }

            var airportToDelete = context.Airports.FirstOrDefault(a => a.AirportID == selectedAirport.AirportID);

            if (airportToDelete != null)
            {
                context.Airports.Remove(airportToDelete);
                context.SaveChanges();
            }

            ClearFields();
            LoadAirports();
            MessageBox.Show("Аэропорт успешно удален!");
        }

        private void ClearFields()
        {
            NameTextBox.Clear();
            CityTextBox.Clear();
            CountryTextBox.Clear();
            CodeTextBox.Clear();

            selectedAirport = null;
        }

        private bool ValidateFields()
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text) ||
                string.IsNullOrWhiteSpace(CityTextBox.Text) ||
                string.IsNullOrWhiteSpace(CountryTextBox.Text) ||
                string.IsNullOrWhiteSpace(CodeTextBox.Text))
            {
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
                var airports = context.Airports.ToList();

                // Создание одного листа для всех аэропортов
                var sheet = wb.Worksheets.Add("Airports");

                // Заголовки столбцов
                sheet.Cell(1, 1).Value = "Airport ID";
                sheet.Cell(1, 2).Value = "Name";
                sheet.Cell(1, 3).Value = "City";
                sheet.Cell(1, 4).Value = "Country";
                sheet.Cell(1, 5).Value = "Code";

                // Заполнение данных
                for (int i = 0; i < airports.Count; i++)
                {
                    var airport = airports[i];
                    sheet.Cell(i + 2, 1).Value = airport.AirportID;
                    sheet.Cell(i + 2, 2).Value = airport.Name;
                    sheet.Cell(i + 2, 3).Value = airport.City;
                    sheet.Cell(i + 2, 4).Value = airport.Country;
                    sheet.Cell(i + 2, 5).Value = airport.Code;
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
