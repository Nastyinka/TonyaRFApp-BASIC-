using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;

namespace TonyaRFApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string connectionString =
            "Server=.\\SQLEXPRESS;Database=BeautyClinicDB;Trusted_Connection=True;TrustServerCertificate=True;";

        private int selectedClientId = -1;
        private int selectedAppointmentId = -1;
        private int selectedTreatmentId = -1;


        public MainWindow()
        {
            InitializeComponent();
            LoadClients();
            LoadClientComboBox();
            LoadTreatmentComboBox();
            LoadAppointments();
            LoadTreatments();
        }
        private void LoadClients()              
        {
            using (SqlConnection connection = new SqlConnection(connectionString))  //Create connection
            {
                connection.Open();
                string query = "SELECT * FROM Clients";
                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                DataTable table = new DataTable();
                adapter.Fill(table);
                dgClients.ItemsSource = table.DefaultView;
            }
        }
        private void LoadTreatments()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT TreatmentID, TreatmentName, Price, DurationMinutes FROM Treatments";
                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                DataTable table = new DataTable();
                adapter.Fill(table);
                dgTreatments.ItemsSource = table.DefaultView;
            }
        }
        private void SearchClients(string searchText)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
                    SELECT *
                    FROM Clients
                    WHERE
                    FirstName LIKE @Search
                    OR Surname LIKE @Search
                    OR PhoneNumber LIKE @Search";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Search", "%" + searchText + "%");
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                DataTable table = new DataTable();
                adapter.Fill(table);
                dgClients.ItemsSource = table.DefaultView;


            }
        }

        private void AddClient_Click(object sender, RoutedEventArgs e)          //Add client click method
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
                INSERT INTO Clients
                (
                    FirstName,
                    Surname,
                    DOB,
                    Address,
                    PhoneNumber,
                    HasAllergies,
                    AllergyDetails
                )
                VALUES
                (
                    @FirstName,
                    @Surname,
                    @DOB,
                    @Address,
                    @Phone,
                    @HasAllergies,
                    @AllergyDetails
                )";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@FirstName", txtFirstName.Text);
                command.Parameters.AddWithValue("@Surname", txtSurname.Text);
                command.Parameters.AddWithValue("@DOB", dpDOB.SelectedDate ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Address", txtAddress.Text);
                command.Parameters.AddWithValue("@Phone", txtPhone.Text);
                command.Parameters.AddWithValue("@HasAllergies", chkAllergies.IsChecked == true);
                command.Parameters.AddWithValue("@AllergyDetails", txtAllergyDetails.Text);

                command.ExecuteNonQuery();
            }

            LoadClients();
            LoadClientComboBox();
        }
        private void AddTreatment_Click(object sender, RoutedEventArgs e)          //Add treatment click method
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                if (!decimal.TryParse(txtPrice.Text, out decimal price))
                {
                    MessageBox.Show("Please enter a valid price (e.g. 45.00)");
                    return;
                }

                string query = @"
                INSERT INTO Clients
                (
                    TreatmentName,
                    Price,
                    DurationMinutes,
                )
                VALUES
                (
                    @TreatmentName,
                    @Price,
                    @DurationMinutes,
                )";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@FirstName", txtFirstName.Text);
                command.Parameters.AddWithValue("@Price", txtPrice.Text);
                command.Parameters.AddWithValue("@DurationMinutes", txtDurationMinutes.Text);

                command.ExecuteNonQuery();
            }

            LoadTreatments();
        }
        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchClients(txtSearch.Text);
        }

        private void dgClients_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgClients.SelectedItem == null)
                return;

            if (!(dgClients.SelectedItem is DataRowView drv))
                return;
            DataRow row =
                drv.Row;
            int v = row.Field<int>("ClientID");
            selectedClientId = v;


            txtFirstName.Text = row.Field<string>("FirstName");
            txtSurname.Text = row.Field<string>("Surname");
            dpDOB.SelectedDate = row.Field<DateTime?>("DOB");
            txtAddress.Text = row.Field<string>("Address");
            txtPhone.Text = row.Field<string>("PhoneNumber");
            chkAllergies.IsChecked = row.Field<bool?>("HasAllergies") ?? false;
            txtAllergyDetails.Text = row.Field<string>("AllergyDetails");
        }
        private void dgAppointments_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgAppointments.SelectedItem == null)
                return;

            if (!(dgAppointments.SelectedItem is DataRowView drv))
                return;

            DataRow row = drv.Row;
            selectedAppointmentId = row.Field<int>("AppointmentsID");
            cbClients.SelectedValue = row.Field<int>("ClientID");
            cbTreatments.SelectedValue = row.Field<int>("TreatmentID");

            dpAppointmentDate.SelectedDate = row.Field<DateTime?>("AppointmentDate");
            var appointmentTime = row.Field<TimeSpan?>("AppointmentTime");
            txtAppointmentTime.Text = appointmentTime?.ToString(@"hh\:mm") ?? string.Empty;
            txtAppointmentNotes.Text = row.Field<string>("Notes") ?? "N/A";
        }
        private void dgTreatments_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgTreatments.SelectedItem == null)
                return;

            if (!(dgTreatments.SelectedItem is DataRowView drv))
                return;

            DataRow row =
                drv.Row;
            int v = row.Field<int>("TreatmentID");
            selectedTreatmentId = v;


            txtTreatmentName.Text = row.Field<string>("TreatmentName");
            
            txtPrice.Text = row.Field<string>("Price");
            txtAddress.Text = row.Field<string>("DurationMinutes");
        }
        private void UpdateClient_Click(object sender, RoutedEventArgs e) // Update client info method
        {
            if (selectedClientId == -1)
            {
                MessageBox.Show("Please select a client.");
                return;
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = @"
                    UPDATE Clients
                    SET
                        FirstName = @FirstName,
                        Surname = @Surname,
                        DOB = @DOB,
                        Address = @Address,
                        PhoneNumber = @Phone,
                        HasAllergies = @HasAllergies,
                        AllergyDetails = @AllergyDetails
                    WHERE ClientID = @ClientID";

                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@FirstName", txtFirstName.Text);
                command.Parameters.AddWithValue("@Surname", txtSurname.Text);
                command.Parameters.AddWithValue("@DOB", dpDOB.SelectedDate ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Address", txtAddress.Text);
                command.Parameters.AddWithValue("@Phone", txtPhone.Text);
                command.Parameters.AddWithValue("@HasAllergies", chkAllergies.IsChecked == true);
                command.Parameters.AddWithValue("@AllergyDetails", txtAllergyDetails.Text);

                command.Parameters.AddWithValue("@ClientID", selectedClientId);

                command.ExecuteNonQuery();
            }

            LoadClients();
            LoadClientComboBox();

            MessageBox.Show("Client Updated Successfully");
        }
        private void DeleteClient_Click(Object sender, RoutedEventArgs e)       //Delete client click method
        {
            if(selectedClientId == -1)
            {
                MessageBox.Show("Please select a client.");
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Are you sure you want to delete this client?",
                "Confirm Delete",
                MessageBoxButton.YesNo);

            if (result != MessageBoxResult.Yes)
                return;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "DELETE FROM Clients WHERE ClientID = @ClientID";
                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@ClientID", selectedClientId);

                command.ExecuteNonQuery();
            }
            LoadClients();
            LoadClientComboBox();

            txtFirstName.Clear();
            txtSurname.Clear();
            dpDOB.SelectedDate = null;
            txtAddress.Clear();
            txtPhone.Clear();
            chkAllergies.IsChecked = false;
            txtAllergyDetails.Clear();

            selectedClientId = -1;

            MessageBox.Show("Client deleted successfully.");
        }

        private void UpdateAppointment_Click(object sender, RoutedEventArgs e) // Update Appointment info method
        {
            if (selectedAppointmentId == -1)
            {
                MessageBox.Show("Please select an Appointment.");
                return;
            }
            TimeSpan? parsedTime = null;
            if (!string.IsNullOrWhiteSpace(txtAppointmentTime.Text) &&
                    TimeSpan.TryParse(txtAppointmentTime.Text, out var t))
                parsedTime = t;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = @"
                    UPDATE Appointments
                    SET
                        ClientID = @ClientID,
                        TreatmentID = @TreatmentID,
                        AppointmentDate = @AppointmentDate,
                        AppointmentTime = @AppointmentTime,
                        AppointmentNotes = @Notes
                    WHERE AppointmentsID = @AppointmentsID";

                SqlCommand command = new SqlCommand(query, connection);
                var p = command.Parameters.Add("@AppointmentTime", SqlDbType.Time);

                command.Parameters.AddWithValue("@ClientID", cbClients.SelectedValue);
                command.Parameters.AddWithValue("@TreatmentID", cbTreatments.SelectedValue);
                command.Parameters.AddWithValue("@AppointmentDate", dpAppointmentDate.SelectedDate);

                p.Value = parsedTime.HasValue ? (object)parsedTime.Value : DBNull.Value;
                command.Parameters.AddWithValue("@Notes", txtAppointmentNotes.Text);
                command.Parameters.AddWithValue("@AppointmentsID", selectedAppointmentId);

                command.ExecuteNonQuery();
            }

            LoadAppointments();

            MessageBox.Show("Appointment Updated Successfully");
        }

        private void DeleteAppointment_Click(Object sender, RoutedEventArgs e)       //Delete Appointment click method
        {
            if (selectedAppointmentId == -1)
            {
                MessageBox.Show("Please select an Appointment.");
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Are you sure you want to delete this Appointment?",
                "Confirm Delete",
                MessageBoxButton.YesNo);

            if (result != MessageBoxResult.Yes)
                return;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "DELETE FROM Appointments WHERE AppointmentsID = @AppointmentsID";
                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@AppointmentsID", selectedAppointmentId);

                command.ExecuteNonQuery();
            }
            LoadAppointments();

            selectedAppointmentId = -1;
            txtAppointmentTime.Clear();
            txtAppointmentNotes.Clear();
            dpAppointmentDate.SelectedDate = null;


            MessageBox.Show("Appointment deleted successfully.");
        }
        private void LoadClientComboBox()               //CLient ComboBox Method
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
                    SELECT
                        ClientID,
                        FirstName + ' ' + Surname AS FullName
                    FROM Clients";

                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);

                DataTable table = new DataTable();

                adapter.Fill(table);

                cbClients.ItemsSource = table.DefaultView;

                cbClients.DisplayMemberPath = "FullName";

                cbClients.SelectedValuePath = "ClientID";
            }
        }

        private void LoadTreatmentComboBox()                        //Treatments ComboBox Method
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            { 
                connection.Open();

                string query = @"
                    SELECT
                        TreatmentID,
                        TreatmentName
                    FROM Treatments";

                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);

                DataTable table = new DataTable();

                adapter.Fill(table);
                
                cbTreatments.ItemsSource = table.DefaultView;

                cbTreatments.DisplayMemberPath = "TreatmentName";

                cbTreatments.SelectedValuePath = "TreatmentID";
            }
        }
        
        private void BookAppointment_Click(object sender, RoutedEventArgs e)        //Book Appointment click method
        {
            TimeSpan? parsedTime = null;
            if (!string.IsNullOrWhiteSpace(txtAppointmentTime.Text) &&
                    TimeSpan.TryParse(txtAppointmentTime.Text, out var t))
                parsedTime = t;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
                    INSERT INTO Appointments
                        (
                            ClientID,
                            TreatmentID,
                            AppointmentDate,
                            AppointmentTime,
                            Notes
                         )
                    VALUES
                        (
                            @ClientID,
                            @TreatmentID,
                            @AppointmentDate,
                            @AppointmentTime,
                            @Notes
                        )";
                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@ClientID", cbClients.SelectedValue);

                command.Parameters.AddWithValue("@TreatmentID", cbTreatments.SelectedValue);

                command.Parameters.AddWithValue("@AppointmentDate", dpAppointmentDate.SelectedDate);

                var p = command.Parameters.Add("@AppointmentTime", SqlDbType.Time);
                p.Value = parsedTime.HasValue ? (object)parsedTime.Value : DBNull.Value;

                command.Parameters.AddWithValue("@Notes", txtAppointmentNotes.Text);

                command.ExecuteNonQuery();
            }
            LoadAppointments();

            MessageBox.Show("Appointment Booked Successfully.");
        }

        private void LoadAppointments()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
                    SELECT
                        a.AppointmentsID,
                        a.ClientID,
                        a.TreatmentID,
                        c.FirstName + ' ' + c.Surname AS ClientName,
                        t.TreatmentName,
                        t.Price,
                        a.AppointmentDate,
                        a.AppointmentTime,
                        a.Notes
                    FROM Appointments a
                    JOIN Clients c
                        ON a.ClientID = c.ClientID
                    JOIN Treatments t
                        ON a.TreatmentID = t.TreatmentID
                    ORDER BY
                        a.AppointmentDate,
                        a.AppointmentTime";

                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);

                DataTable table = new DataTable();

                adapter.Fill(table);
                dgAppointments.ItemsSource = table.DefaultView;
            }
        }
    }
}
