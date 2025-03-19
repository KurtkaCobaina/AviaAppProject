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
    /// Логика взаимодействия для PaymentControl.xaml
    /// </summary>
    public partial class PaymentControl : Window
    {
        private Payments selectedPayment;
        private TRPOEntities context;

        public PaymentControl()
        {
            InitializeComponent();
            context = new TRPOEntities();
            LoadPayments();
        }

        private void LoadPayments()
        {
            var payments = context.Payments.ToList();
            PaymentsListBox.ItemsSource = payments;
        }

        private void PaymentsListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            selectedPayment = PaymentsListBox.SelectedItem as Payments;

            if (selectedPayment != null)
            {
                BookingIDTextBox.Text = selectedPayment.BookingID?.ToString() ?? string.Empty;
                PaymentDateTextBox.Text = selectedPayment.PaymentDate?.ToString("yyyy-MM-dd") ?? string.Empty;
                PaymentMethodTextBox.Text = selectedPayment.PaymentMethod ?? string.Empty;
                PaymentStatusTextBox.Text = selectedPayment.PaymentStatus ?? string.Empty;
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateFields())
            {
                MessageBox.Show("Заполните все поля.");
                return;
            }

            var newPayment = new Payments
            {
                BookingID = int.TryParse(BookingIDTextBox.Text, out int bookingId) ? (int?)bookingId : null,
                PaymentDate = DateTime.TryParse(PaymentDateTextBox.Text, out DateTime paymentDate) ? (DateTime?)paymentDate : null,
                PaymentMethod = PaymentMethodTextBox.Text,
                PaymentStatus = PaymentStatusTextBox.Text
            };

            context.Payments.Add(newPayment);
            context.SaveChanges();

            ClearFields();
            LoadPayments();
            MessageBox.Show("Платеж успешно добавлен!");
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedPayment == null)
            {
                MessageBox.Show("Выберите платеж для изменения.");
                return;
            }

            if (!ValidateFields())
            {
                MessageBox.Show("Заполните все поля.");
                return;
            }

            var paymentToUpdate = context.Payments.FirstOrDefault(p => p.PaymentID == selectedPayment.PaymentID);

            if (paymentToUpdate != null)
            {
                paymentToUpdate.BookingID = int.TryParse(BookingIDTextBox.Text, out int bookingId) ? (int?)bookingId : null;
                paymentToUpdate.PaymentDate = DateTime.TryParse(PaymentDateTextBox.Text, out DateTime paymentDate) ? (DateTime?)paymentDate : null;
                paymentToUpdate.PaymentMethod = PaymentMethodTextBox.Text;
                paymentToUpdate.PaymentStatus = PaymentStatusTextBox.Text;

                context.SaveChanges();
            }

            ClearFields();
            LoadPayments();
            MessageBox.Show("Платеж успешно изменен!");
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedPayment == null)
            {
                MessageBox.Show("Выберите платеж для удаления.");
                return;
            }

            var paymentToDelete = context.Payments.FirstOrDefault(p => p.PaymentID == selectedPayment.PaymentID);

            if (paymentToDelete != null)
            {
                context.Payments.Remove(paymentToDelete);
                context.SaveChanges();
            }

            ClearFields();
            LoadPayments();
            MessageBox.Show("Платеж успешно удален!");
        }

        private void ClearFields()
        {
            BookingIDTextBox.Clear();
            PaymentDateTextBox.Clear();
            PaymentMethodTextBox.Clear();
            PaymentStatusTextBox.Clear();

            selectedPayment = null;
        }

        private bool ValidateFields()
        {
            if (string.IsNullOrWhiteSpace(BookingIDTextBox.Text) ||
                string.IsNullOrWhiteSpace(PaymentDateTextBox.Text) ||
                string.IsNullOrWhiteSpace(PaymentMethodTextBox.Text) ||
                string.IsNullOrWhiteSpace(PaymentStatusTextBox.Text))
            {
                return false;
            }

            if (!int.TryParse(BookingIDTextBox.Text, out _))
            {
                MessageBox.Show("BookingID должен быть числом.");
                return false;
            }

            if (!DateTime.TryParse(PaymentDateTextBox.Text, out _))
            {
                MessageBox.Show("Дата платежа должна быть в формате yyyy-MM-dd.");
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
            using (var context = TRPOEntities.GetContext()) 
            {
               
                var payments = context.Payments.ToList();

                var sheet = wb.Worksheets.Add("Payments");

                
                sheet.Cell(1, 1).Value = "Payment ID";
                sheet.Cell(1, 2).Value = "Booking ID";
                sheet.Cell(1, 3).Value = "Payment Date";
                sheet.Cell(1, 4).Value = "Payment Method";
                sheet.Cell(1, 5).Value = "Payment Status";

                for (int i = 0; i < payments.Count; i++)
                {
                    var payment = payments[i];
                    sheet.Cell(i + 2, 1).Value = payment.PaymentID;
                    sheet.Cell(i + 2, 2).Value = payment.BookingID ?? 0; 
                    sheet.Cell(i + 2, 3).Value = payment.PaymentDate?.ToString("yyyy-MM-dd HH:mm") ?? "Unknown"; 
                    sheet.Cell(i + 2, 4).Value = payment.PaymentMethod ?? "Unknown"; 
                    sheet.Cell(i + 2, 5).Value = payment.PaymentStatus ?? "Unknown"; 
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
        private void SearchByPaymentStatusButton_Click(object sender, RoutedEventArgs e)
        {
            string searchPaymentStatus = SearchPaymentStatusTextBox.Text.Trim();

            if (string.IsNullOrEmpty(searchPaymentStatus))
            {
                LoadPayments(); // Если строка поиска пустая, загружаем все платежи
                return;
            }

            var filteredPayments = context.Payments
                .Where(p => p.PaymentStatus != null && p.PaymentStatus.Contains(searchPaymentStatus))
                .ToList();

            PaymentsListBox.ItemsSource = filteredPayments;
        }
    }
}