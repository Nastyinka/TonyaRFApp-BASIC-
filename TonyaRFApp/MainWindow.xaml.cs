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
        private DateTime currentWeekStart;      // Stores WHICH Monday we are viewing, navigation buttons will know how to react
        private DateTime selectedCalendarDate; // will store which date and time was clicked
        private TimeSpan selectedCalendarTime;
        private int selectedCalendarAppointmentId = -1; // will store which appointment was clicked on the calendar -> -1 means none selected

        private enum PanelMode { Book, Edit } //named set of states -- enum instead of bool
        private PanelMode currentPanelMode;
        public MainWindow()
        {
            InitializeComponent();
            LoadClients();
            LoadClientComboBox();
            LoadTreatmentComboBox();
            LoadAppointments();
            LoadTreatments();
            LoadTimeSlots();
            GenerateWeekGrid(GetMonday(DateTime.Today));
        }

        private void ShowDbError(string context, Exception ex)
        {
            CustomMessageBox.ShowError($"An error occurred while {context}.\n\n" + "Please make sure SQL Server is running.\n\n" + $"Details: {ex.Message}", "Database Error");
        }
        private void LoadClients()              
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))  //Create connection
                {
                    connection.Open();
                    string query = @"SELECT 
                                    c.ClientID, 
                                    c.FirstName, 
                                    c.Surname, 
                                    c.DOB, 
                                    c.Address, 
                                    c.PhoneNumber, 
                                    c.HasAllergies, 
                                    c.AllergyDetails, 
                                    c.HasImplants, 
                                    c.ImplantDetails, 
                                    c.HasBotox, 
                                    c.BotoxDetails, 
                                    c.HasFaceMetals, 
                                    c.FaceMetalDetails, 
                                    c.ConsentSigned, 
                                    c.ConsentDate,
                                    COUNT(a.AppointmentsID) AS TotalAppointments
                            FROM Clients c
                            LEFT JOIN Appointments a ON c.ClientID = a.ClientID
                            GROUP BY
                                    c.ClientID, c.FirstName, c.Surname, c.DOB, c.Address, c.PhoneNumber, c.HasAllergies, c.AllergyDetails, c.HasImplants, c.ImplantDetails, c.HasBotox, c.BotoxDetails, c.HasFaceMetals,
                                    c.FaceMetalDetails, c.ConsentSigned, c.ConsentDate";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    DataTable table = new DataTable();
                    adapter.Fill(table);

                    //Feeding both the seclection and records grid
                    dgClients.ItemsSource = table.DefaultView;
                    dgRecordsClients.ItemsSource = table.DefaultView;
                }
            }

            catch (SqlException ex)
            {
                ShowDbError("loading clients", ex);
            }
        } 
        private void LoadAppointments()
        {
            try
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

            catch (SqlException ex)
            {
                ShowDbError("loading appointments", ex);
            }
        }
        private void LoadTreatments()
        {
            try
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

            catch (SqlException ex)
            {
                ShowDbError("loading treatments", ex);
            }
        }
        //clear button for client form
        private void NewClient_Click(object sender, RoutedEventArgs e)
        {
            selectedClientId = -1;
            txtFirstName.Clear();
            txtSurname.Clear();
            dpDOB.SelectedDate = null;
            txtAddress.Clear();
            txtPhone.Clear();
            chkAllergies.IsChecked = false;
            txtAllergyDetails.Clear();
            chkImplants.IsChecked = false;
            txtImplantDetails.Clear();
            chkBotox.IsChecked = false;
            txtBotoxDetails.Clear();
            chkFaceMetals.IsChecked = false;
            txtFaceMetalDetails.Clear();
            chkConsentSigned.IsChecked = false;
            dpConsentDate.SelectedDate = null;
            txtTotalAppointments.Text = "Total Appointments -";
            dgClients.SelectedItem = null;
        }
        private void SearchClients(string searchText)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"
                        SELECT
                            c.ClientID, c.FirstName, c.Surname, c.DOB, c.Address, c.PhoneNumber, c.HasAllergies, c.AllergyDetails, 
                            c.HasImplants, c.ImplantDetails, c.HasBotox, c.BotoxDetails, c.HasFaceMetals, c.FaceMetalDetails, c.ConsentSigned, c.ConsentDate,
                        COUNT(a.AppointmentsID) AS TotalAppointments
                        FROM Clients c
                        LEFT JOIN Appointments a ON c.ClientID = a.ClientID
                        WHERE c.FirstName LIKE @Search OR c.Surname LIKE @Search OR c.PhoneNumber LIKE @Search
                        GROUP BY
                            c.ClientID, c.FirstName, c.Surname, c.DOB, c.Address, c.PhoneNumber, c.HasAllergies, c.AllergyDetails,
                            c.HasImplants, c.ImplantDetails, c.HasBotox, c.BotoxDetails, c.HasFaceMetals, c.FaceMetalDetails, c.ConsentSigned, c.ConsentDate";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Search", "%" + searchText + "%");
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable table = new DataTable();
                    adapter.Fill(table);
                    dgClients.ItemsSource = table.DefaultView;


                }
            }

            catch (SqlException ex)
            {
                ShowDbError("searching clients", ex);
            }
        }

        private void AddClient_Click(object sender, RoutedEventArgs e)          //Add client click method
        {
            if (string.IsNullOrWhiteSpace(txtFirstName.Text))
            {
                CustomMessageBox.Show("Please enter a first name.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtSurname.Text))
            {
                CustomMessageBox.Show("Please enter a surname");
                return;
            }
            
            if (string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                CustomMessageBox.Show("Please enter a phone number.");
                return;
            }
            try
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
                    AllergyDetails,
                    HasImplants,
                    ImplantDetails,
                    HasBotox,
                    BotoxDetails,
                    HasFaceMetals,
                    FaceMetalDetails,
                    ConsentSigned,
                    ConsentDate
                )
                VALUES
                (
                    @FirstName,
                    @Surname,
                    @DOB,
                    @Address,
                    @Phone,
                    @HasAllergies,
                    @AllergyDetails,
                    @HasImplants,
                    @ImplantDetails,
                    @HasBotox,
                    @BotoxDetails,
                    @HasFaceMetals,
                    @FaceMetalDetails,
                    @ConsentSigned,
                    @ConsentDate
                )";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@FirstName", txtFirstName.Text);
                    command.Parameters.AddWithValue("@Surname", txtSurname.Text);
                    command.Parameters.AddWithValue("@DOB", dpDOB.SelectedDate.HasValue ? (object)dpDOB.SelectedDate.Value.Date : DBNull.Value);
                    command.Parameters.AddWithValue("@Address", txtAddress.Text);
                    command.Parameters.AddWithValue("@Phone", txtPhone.Text);
                    command.Parameters.AddWithValue("@HasAllergies", chkAllergies.IsChecked == true);
                    command.Parameters.AddWithValue("@AllergyDetails", txtAllergyDetails.Text);
                    command.Parameters.AddWithValue("@HasImplants", chkImplants.IsChecked == true);
                    command.Parameters.AddWithValue("@ImplantDetails", txtImplantDetails.Text);
                    command.Parameters.AddWithValue("@HasBotox", chkBotox.IsChecked == true);
                    command.Parameters.AddWithValue("@BotoxDetails", txtBotoxDetails.Text);
                    command.Parameters.AddWithValue("@HasFaceMetals", chkFaceMetals.IsChecked == true);
                    command.Parameters.AddWithValue("@FaceMetalDetails", txtFaceMetalDetails.Text);
                    command.Parameters.AddWithValue("@ConsentSigned", chkConsentSigned.IsChecked == true);
                    command.Parameters.AddWithValue("@ConsentDate", dpConsentDate.SelectedDate.HasValue ? (object)dpConsentDate.SelectedDate.Value.Date : DBNull.Value);

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
                chkImplants.IsChecked = false;
                txtImplantDetails.Clear();
                chkBotox.IsChecked = false;
                txtBotoxDetails.Clear();
                chkFaceMetals.IsChecked = false;
                txtFaceMetalDetails.Clear();
                chkConsentSigned.IsChecked = false;
                dpConsentDate.SelectedDate = null;

                CustomMessageBox.Show("Client added successfully.");
            }

            catch (SqlException ex)
            {
                ShowDbError("adding client", ex);
            }
        }

        private void AddTreatment_Click(object sender, RoutedEventArgs e)          //Add treatment click method
        {
            // Basic validation
            if (string.IsNullOrWhiteSpace(txtTreatmentName.Text))
            {
                CustomMessageBox.Show("Please enter a treatment name.");
                return;
            }

            if (!decimal.TryParse(txtPrice.Text, out decimal price))
            {
                CustomMessageBox.Show("Please enter a valid price (e.g. 45.00)");
                return;
            }
            int? duration = null;
            if (int.TryParse(txtDurationMinutes.Text, out int d))
                duration = d;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"
                INSERT INTO Treatments
                (
                    TreatmentName,
                    Price,
                    DurationMinutes
                )
                VALUES
                (
                    @TreatmentName,
                    @Price,
                    @DurationMinutes
                )";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@TreatmentName", txtTreatmentName.Text);
                    command.Parameters.AddWithValue("@Price", price);
                    var p = command.Parameters.Add("@DurationMinutes", SqlDbType.Int);
                    p.Value = duration.HasValue ? (object)duration.Value : DBNull.Value;

                    command.ExecuteNonQuery();
                }


            LoadTreatments();
            LoadTreatmentComboBox(); //  keeps the booking tab in sync
            CustomMessageBox.Show("Treatment added successfully.");
            }
            catch (SqlException ex)
            {
                ShowDbError("adding treatment", ex);
            }
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
            int? clientId = row.Field<int?>("ClientID");
            if (!clientId.HasValue || clientId.Value == 0)
                return;
          
            selectedClientId = clientId.Value;


            txtFirstName.Text = row.Field<string>("FirstName");
            txtSurname.Text = row.Field<string>("Surname");
            dpDOB.SelectedDate = row.Field<DateTime?>("DOB");
            txtAddress.Text = row.Field<string>("Address");
            txtPhone.Text = row.Field<string>("PhoneNumber");
            chkAllergies.IsChecked = row.Field<bool?>("HasAllergies") ?? false;
            txtAllergyDetails.Text = row.Field<string>("AllergyDetails");
            chkImplants.IsChecked = row.Field<bool?>("HasImplants") ?? false;
            txtImplantDetails.Text = row.Field<string>("ImplantDetails");
            chkBotox.IsChecked = row.Field<bool?>("HasBotox") ?? false;
            txtBotoxDetails.Text = row.Field<string>("BotoxDetails");
            chkFaceMetals.IsChecked = row.Field<bool?>("HasFaceMetals") ?? false;
            txtFaceMetalDetails.Text = row.Field<string>("FaceMetalDetails");
            chkConsentSigned.IsChecked = row.Field<bool?>("ConsentSigned") ?? false;
            dpConsentDate.SelectedDate = row.Field<DateTime?>("ConsentDate");

            int totalAppts = row.Field<int>("TotalAppointments");
            txtTotalAppointments.Text = $"Total Appointments: {totalAppts}";
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
            cbAppointmentTime.SelectedItem = appointmentTime?.ToString(@"hh\:mm") ?? string.Empty;
            txtAppointmentNotes.Text = row.Field<string>("Notes") ?? "N/A";
        }
        private void dgTreatments_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgTreatments.SelectedItem == null)
                return;

            if (!(dgTreatments.SelectedItem is DataRowView drv))
                return;

            DataRow row = drv.Row;

            int? v = row.Field<int?>("TreatmentID");
            if (!v.HasValue) return;
            selectedTreatmentId = v.Value;

            txtTreatmentName.Text = row.Field<string>("TreatmentName");
            txtPrice.Text = row.Field<decimal>("Price").ToString();
            txtDurationMinutes.Text = row.Field<int?>("DurationMinutes")?.ToString() ?? "";
        }

        // Fixing the time showing for dob and consent date
        private void dgClients_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyName == "DOB" || e.PropertyName == "ConsentDate")
            {
                var column = e.Column as DataGridTextColumn;
                if (column != null)
                {
                    column.Binding = new System.Windows.Data.Binding(e.PropertyName)
                    {
                        StringFormat = "dd/MM/yyyy"
                    };
                }
            }
        }
        private void UpdateClient_Click(object sender, RoutedEventArgs e) // Update client info method
        {
            if (selectedClientId == -1)
            {
                CustomMessageBox.Show("Please select a client.");
                return;
            }
            try
            {
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
                        AllergyDetails = @AllergyDetails,
                        HasImplants = @HasImplants,
                        ImplantDetails = @ImplantDetails,
                        HasBotox = @HasBotox,
                        BotoxDetails = @BotoxDetails,
                        HasFaceMetals = @HasFaceMetals,
                        FaceMetalDetails = @FaceMetalDetails,
                        ConsentSigned = @ConsentSigned,
                        ConsentDate = @ConsentDate
                    WHERE ClientID = @ClientID";

                    SqlCommand command = new SqlCommand(query, connection);

                    command.Parameters.AddWithValue("@FirstName", txtFirstName.Text);
                    command.Parameters.AddWithValue("@Surname", txtSurname.Text);
                    command.Parameters.AddWithValue("@DOB", dpDOB.SelectedDate.HasValue ? (object)dpDOB.SelectedDate.Value.Date : DBNull.Value);
                    command.Parameters.AddWithValue("@Address", txtAddress.Text);
                    command.Parameters.AddWithValue("@Phone", txtPhone.Text);
                    command.Parameters.AddWithValue("@HasAllergies", chkAllergies.IsChecked == true);
                    command.Parameters.AddWithValue("@AllergyDetails", txtAllergyDetails.Text);
                    command.Parameters.AddWithValue("@HasImplants", chkImplants.IsChecked == true);
                    command.Parameters.AddWithValue("@ImplantDetails", txtImplantDetails.Text);
                    command.Parameters.AddWithValue("@HasBotox", chkBotox.IsChecked == true);
                    command.Parameters.AddWithValue("@BotoxDetails", txtBotoxDetails.Text);
                    command.Parameters.AddWithValue("@HasFaceMetals", chkFaceMetals.IsChecked == true);
                    command.Parameters.AddWithValue("@FaceMetalDetails", txtFaceMetalDetails.Text);
                    command.Parameters.AddWithValue("@ConsentSigned", chkConsentSigned.IsChecked == true);
                    command.Parameters.AddWithValue("@ConsentDate", dpConsentDate.SelectedDate.HasValue ? (object)dpConsentDate.SelectedDate.Value.Date : DBNull.Value);

                    command.Parameters.AddWithValue("@ClientID", selectedClientId);

                    command.ExecuteNonQuery();
                }

            LoadClients();
            LoadClientComboBox();

            CustomMessageBox.Show("Client Updated Successfully");
            }

            catch (SqlException ex)
            {
                ShowDbError("updating client", ex);
            }

        }
        private void UpdateAppointment_Click(object sender, RoutedEventArgs e) // Update Appointment info method
        {
            if (selectedAppointmentId == -1)
            {
                CustomMessageBox.Show("Please select an Appointment.");
                return;
            }
            TimeSpan? parsedTime = null;
            if (cbAppointmentTime.SelectedItem is string selectedTime)
                parsedTime = TimeSpan.Parse(selectedTime);


            try
            {
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
                        Notes = @Notes
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
            GenerateWeekGrid(currentWeekStart); // Refresh the week grid to reflect changes

            CustomMessageBox.Show("Appointment Updated Successfully");
            }

            catch (SqlException ex)
            {
                ShowDbError("updating appointment", ex);
            }
        }
        private void UpdateTreatment_Click(object sender, RoutedEventArgs e) // Update treatment info method
        {
            if (selectedTreatmentId == -1)
            {
                CustomMessageBox.Show("Please select a treatment.");
                return;
            }
            if (!decimal.TryParse(txtPrice.Text, out decimal price))
            {
                CustomMessageBox.Show("Please enter a valid price (e.g. 45.00)");
                return;
            }
            //Parse duration saferly
            int? duration = null;
            if (int.TryParse(txtDurationMinutes.Text, out int d))
                duration = d;

            try
            {

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                    UPDATE Treatments
                    SET
                        TreatmentName = @TreatmentName,
                        Price = @Price,
                        DurationMinutes = @DurationMinutes
                    WHERE TreatmentID = @TreatmentID";

                    SqlCommand command = new SqlCommand(query, connection);

                    command.Parameters.AddWithValue("@TreatmentName", txtTreatmentName.Text);
                    command.Parameters.AddWithValue("@Price", price);

                    var p = command.Parameters.Add("@DurationMinutes", SqlDbType.Int);
                    p.Value = duration.HasValue ? (object)duration.Value : DBNull.Value;

                    command.Parameters.AddWithValue("@TreatmentID", selectedTreatmentId);

                    command.ExecuteNonQuery();
                }

            LoadTreatments();
            LoadTreatmentComboBox();

            CustomMessageBox.Show("Treatment Updated Successfully");
            }

            catch (SqlException ex)
            {
                ShowDbError("updating treatment", ex);
            }
        }

        private void DeleteClient_Click(Object sender, RoutedEventArgs e)       //Delete client click method
        {
            if (selectedClientId == -1)
            {
                CustomMessageBox.Show("Please select a client.");
                return;
            }

            try
            {
                bool hasAppointments = false;


                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string checkQuery = @"
                   SELECT COUNT(*)
                    FROM Appointments
                    WHERE ClientID = @ClientID";
                    SqlCommand checkCommand = new SqlCommand(checkQuery, connection);
                    checkCommand.Parameters.AddWithValue("@ClientID", selectedClientId);
                    // returns single value
                    int appointmentCount = (int)checkCommand.ExecuteScalar();
                    hasAppointments = appointmentCount > 0;
                }
                if (hasAppointments)
                {
                    MessageBoxResult result = CustomMessageBox.ShowConfirm(
                        "This client has an existing appointment. \n\n" +
                        "Click YES to delete the client AND all their appointments. \n" +
                        "Click NO to cancel.",
                        "Client Has Appointments",
                        MessageBoxImage.Warning);

                    if (result != MessageBoxResult.Yes)     // Delete Appointments first, THEN delete client (Child records deleted first)
                        return;

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        string deleteAppointments =
                            "DELETE FROM Appointments WHERE ClientID = @ClientID";
                        SqlCommand cmd = new SqlCommand(deleteAppointments, connection);
                        cmd.Parameters.AddWithValue("@ClientID", selectedClientId);
                        cmd.ExecuteNonQuery();

                        string deleteClient =
                            "DELETE FROM Clients WHERE ClientID = @ClientID";
                        SqlCommand cmd2 = new SqlCommand(deleteClient, connection);
                        cmd2.Parameters.AddWithValue("@ClientID", selectedClientId);
                        cmd2.ExecuteNonQuery();
                    }

                }
                else
                {
                    // No Appointments, simply delete client - regular confirmation

                    MessageBoxResult result = CustomMessageBox.ShowConfirm(
                        "Are you sure you want to delete this client?",
                        "Confirm Delete");

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

                }
                LoadClients();
                LoadClientComboBox();
                LoadAppointments();

                txtFirstName.Clear();
                txtSurname.Clear();
                dpDOB.SelectedDate = null;
                txtAddress.Clear();
                txtPhone.Clear();
                chkAllergies.IsChecked = false;
                txtAllergyDetails.Clear();
                chkImplants.IsChecked = false;
                txtImplantDetails.Clear();
                chkBotox.IsChecked = false;
                txtBotoxDetails.Clear();
                chkFaceMetals.IsChecked = false;
                txtFaceMetalDetails.Clear();
                chkConsentSigned.IsChecked = false;
                dpConsentDate.SelectedDate = null;

                selectedClientId = -1;

                CustomMessageBox.Show("Client deleted successfully.");
            }
            catch (SqlException ex)
            {
                ShowDbError("deleting client", ex);
            }
        }

     
        private void DeleteAppointment_Click(Object sender, RoutedEventArgs e)       //Delete Appointment click method
        {
            if (selectedAppointmentId == -1)
            {
                CustomMessageBox.Show("Please select an Appointment.");
                return;
            }

            MessageBoxResult result = CustomMessageBox.ShowConfirm(
                "Are you sure you want to delete this Appointment?",
                "Confirm Delete");

            if (result != MessageBoxResult.Yes)
                return;
            try
            {
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
            cbAppointmentTime.SelectedItem = null;
            txtAppointmentNotes.Clear();
            dpAppointmentDate.SelectedDate = null;


            CustomMessageBox.Show("Appointment deleted successfully.");
            }
            catch (SqlException ex)
            {
                ShowDbError("deleting appointment", ex);
            }
        }
        private void DeleteTreatment_Click(Object sender, RoutedEventArgs e)       //Delete treatment click method
        {
            if (selectedTreatmentId == -1)
            {
                CustomMessageBox.Show("Please select a treatment.");
                return;
            }

            MessageBoxResult result = CustomMessageBox.ShowConfirm(
                "Are you sure you want to delete this treatment?",
                "Confirm Delete");

            if (result != MessageBoxResult.Yes)
                return;
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "DELETE FROM Treatments WHERE TreatmentID = @TreatmentID";
                    SqlCommand command = new SqlCommand(query, connection);

                    command.Parameters.AddWithValue("@TreatmentID", selectedTreatmentId);

                    command.ExecuteNonQuery();
                }
       
            LoadTreatments();
            LoadTreatmentComboBox();

            txtTreatmentName.Clear();
            txtPrice.Clear();
            txtDurationMinutes.Clear();

            selectedTreatmentId = -1;

            CustomMessageBox.Show("Treatment deleted successfully.");
            }
            catch (SqlException ex)
            {
                ShowDbError("deleting treatment", ex);
            }
        }

        private void LoadTimeSlots()
        {
            var times = new List<string>();
            //Loop every hour and ever 5 min intervals
            for (int hour = 0; hour < 24; hour++)
            {
                for (int minute = 0; minute < 60; minute += 30)
                {
                    times.Add($"{hour:D2}:{minute:D2}");
                }
            }
            cbAppointmentTime.ItemsSource = times;
            cbPanelTime.ItemsSource = times;
        }
        private void LoadClientComboBox()               //Client ComboBox Method
        {
            try
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

                    cbPanelClients.ItemsSource = table.DefaultView;
                    cbPanelClients.DisplayMemberPath = "FullName";
                    cbPanelClients.SelectedValuePath = "ClientID";
                }
            }
            catch (SqlException ex)
            {
                ShowDbError("loading client combobox", ex);
            }
        }

        private void LoadTreatmentComboBox()                        //Treatments ComboBox Method
        {
            try
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

                    cbPanelTreatments.ItemsSource = table.DefaultView;
                    cbPanelTreatments.DisplayMemberPath = "TreatmentName";
                    cbPanelTreatments.SelectedValuePath = "TreatmentID";
                }
            }
            catch (SqlException ex)
            {
                ShowDbError("loading treatment combobox", ex);
            }
        }
        
        private void BookAppointment_Click(object sender, RoutedEventArgs e)        //Book Appointment click method
        {
            TimeSpan? parsedTime = null;
            if (cbAppointmentTime.SelectedItem is string selectedTime)
                parsedTime = TimeSpan.Parse(selectedTime);

            if (cbClients.SelectedValue == null)
            {
                CustomMessageBox.Show("Please select a client.");
                return;
            }

            if (cbTreatments.SelectedValue == null)
            {
                CustomMessageBox.Show("Please select a treatment.");
                return;
            }

            if (dpAppointmentDate.SelectedDate == null)
            {
                CustomMessageBox.Show("Please select a date.");
                return;
            }

            if (cbAppointmentTime.SelectedItem == null)
            {
                CustomMessageBox.Show("Please select a time.");
                return;
            }
            try
            {
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
            GenerateWeekGrid(currentWeekStart); // Refresh the week grid to reflect new appointment

            CustomMessageBox.Show("Appointment Booked Successfully.");
            }
            catch (SqlException ex)
            {
                ShowDbError("booking appointment", ex);
            }
        }

        // --Calender & Navigation Methods--

        // Keeps the layout consistent
        private DateTime GetMonday(DateTime date)
        {
            // DayofWeek is an enum - Sunday=0 Monday=1... Saturday=6
            // Need to know how many days to go back to reach Monday

            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;

            return date.AddDays(-1 * diff).Date;
        }

        private void GenerateWeekGrid(DateTime weekStart)
        {
            // To remeber which week is shown, navigation buttons know what to calculate from
            currentWeekStart = weekStart;

            //Update the label to show the week range
            DateTime weekEnd = weekStart.AddDays(6);
            txtWeekLabel.Text = $"{weekStart:d MMM} - {weekEnd:d MMM yyyy}";

            // Clear everything to prevent old cells stacking under new ones
            weekGrid.Children.Clear();
            weekGrid.RowDefinitions.Clear();
            weekGrid.ColumnDefinitions.Clear();

            //Defining columns
            weekGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) }); // Time column

            for (int i = 0; i < 7; i++)
            {
                weekGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); //GridUnitType.Star means 'share space equally' like Width="*" in XAML
            }

            //Defining rows

            weekGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Header row for days of the week

            int totalSlots = 24;
            for (int i = 0; i < totalSlots; i++)
            {
                weekGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(30) });
            }

            // Building the header row

            Border cornerCell = new Border
            {
                Background = (Brush)FindResource("MidLavBrush"),
                BorderBrush = (Brush)FindResource("BorderBrush"),
                BorderThickness = new Thickness(0.5)
            };
            Grid.SetRow(cornerCell, 0);
            Grid.SetColumn(cornerCell, 0);
            weekGrid.Children.Add(cornerCell);

            //one header cell per day

            for (int day = 0; day < 7; day++)
            {
                DateTime thisDay = weekStart.AddDays(day);

                bool isToday = thisDay.Date == DateTime.Today;

                Border headerCell = new Border
                {

                    Background = isToday
                    ? (Brush)FindResource("PrimaryBrush")
                    : (Brush)FindResource("MidLavBrush"),
                    BorderBrush = (Brush)FindResource("BorderBrush"),
                    BorderThickness = new Thickness(0.5),
                    Padding = new Thickness(4)
                };

                TextBlock headerText = new TextBlock
                {
                    Text = thisDay.ToString("ddd\nd MMM"),
                    TextAlignment = TextAlignment.Center,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = (Brush)FindResource("TextLightBrush")
                };

                headerCell.Child = headerText;

                Grid.SetRow(headerCell, 0);

                Grid.SetColumn(headerCell, day + 1); // +1 because first column is time

                weekGrid.Children.Add(headerCell);
            }

            // Time Labels and Empty Day Cells

            DateTime slotTime = weekStart.Date.AddHours(8);

            for (int row = 0; row < totalSlots; row++)
            {
                Border timeCell = new Border
                {
                    Background = (Brush)FindResource("LavenderBrush"),
                    BorderBrush = (Brush)FindResource("BorderBrush"),
                    BorderThickness = new Thickness(0.5),
                    Padding = new Thickness(4, 2, 4, 2)
                };

                TextBlock timeText = new TextBlock
                {
                    Text = slotTime.ToString("HH:mm"),
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = (Brush)FindResource("TextDarkBrush"),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                timeCell.Child = timeText;

                Grid.SetRow(timeCell, row + 1); // +1 because first row is header
                Grid.SetColumn(timeCell, 0);

                weekGrid.Children.Add(timeCell); // Adds the timecell to grid

                for (int day = 0; day < 7; day++)
                {
                    DateTime thisDay = weekStart.AddDays(day);
                    // capturing specific DAY and TIME for lambda expressions, otherwise they will all reference the last value of the loop
                    DateTime capturedDay = weekStart.AddDays(day);
                    TimeSpan capturedTime = TimeSpan.FromHours(8) + TimeSpan.FromMinutes(row * 30);


                    Border dayCell = new Border
                    {
                        Background = (Brush)FindResource("SurfaceBrush"),
                        BorderBrush = (Brush)FindResource("BorderBrush"),
                        BorderThickness = new Thickness(0.5),
                        Cursor = Cursors.Hand // signals that the cell is clickable
                    };

                    //highlight on hover
                    dayCell.MouseEnter += (s, e) =>
                    {
                        dayCell.Background = (Brush)FindResource("LavenderBrush");
                    };

                    dayCell.MouseLeave += (s, e) =>
                    {
                        dayCell.Background = (Brush)FindResource("SurfaceBrush");
                    };

                    //wiring the click even with a lambda
                    dayCell.MouseLeftButtonUp += (sender, e) =>
                    {
                        CalendarCell_Clicked(capturedDay, capturedTime);
                    };

                    Grid.SetRow(dayCell, row + 1);
                    Grid.SetColumn(dayCell, day + 1); // +1 because first column is time

                    weekGrid.Children.Add(dayCell); // adds the daycell to grid
                }

                slotTime = slotTime.AddMinutes(30); // Increment by 30 minutes
            }

            LoadWeekAppointments(weekStart);
        }

        // -- Load Appointments into the Week Grid --

        private void LoadWeekAppointments(DateTime weekStart)
        {
            DateTime weekEnd = weekStart.AddDays(7);
            //query up to but not including the day AFTER sunday

            var appointments = new Dictionary<string, AppointmentInfo>();
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"
                    SELECT
                        a.AppointmentsID,
                        a.AppointmentDate,
                        a.AppointmentTime,
                        a.ClientID,
                        a.TreatmentID,
                        c.FirstName + ' ' + c.Surname AS ClientName,
                        t.TreatmentName,
                        ISNULL(t.DurationMinutes, 30) AS DurationMinutes, 
                        a.Notes
                    FROM Appointments a
                    JOIN Clients c ON a.ClientID = c.ClientID
                    JOIN Treatments t ON a.TreatmentID = t.TreatmentID
                    WHERE a.AppointmentDate >= @WeekStart AND a.AppointmentDate < @WeekEnd
                    ORDER BY a.AppointmentDate, a.AppointmentTime";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@WeekStart", weekStart.Date);
                    command.Parameters.AddWithValue("@WeekEnd", weekEnd.Date);

                    SqlDataReader reader = command.ExecuteReader(); //reads one row at a time, more efficient than loading all into memory

                    while (reader.Read())
                    {
                        int appointmentId = reader.GetInt32(0);
                        DateTime apptDate = reader.GetDateTime(1);
                        TimeSpan apptTime = reader.GetTimeSpan(2);
                        int clientId = reader.GetInt32(3);
                        int treatmentId = reader.GetInt32(4);

                        //lookup for key from date n time
                        string key = $"{apptDate:yyyy-MM-dd}-{apptTime:hh\\:mm}";

                        appointments[key] = new AppointmentInfo
                        {
                            AppointmentId = appointmentId,
                            ClientId = clientId,
                            TreatmentId = treatmentId,
                            ClientName = reader.GetString(5),
                            TreatmentName = reader.GetString(6),
                            DurationMinutes = reader.GetInt32(7),
                            Notes = reader.IsDBNull(8) ? "" : reader.GetString(8)
                        };

                        // reader.DBNULL checks if notes is empty in db, using empty string to prevent crashing
                    }
                }

                for (int day = 0; day < 7; day++)
                {
                    DateTime thisDay = weekStart.AddDays(day);
                    int col = day + 1; // +1 because first column is time, so days start at column 1

                    for (int slot = 0; slot < 24; slot++)
                    {
                        //working out what time this slot represents
                        TimeSpan slotTime = TimeSpan.FromHours(8) + TimeSpan.FromMinutes(slot * 30); // 8:00 + 0, 30, 60, 90... minutes

                        // same key used in storing
                        string key = $"{thisDay:yyyy-MM-dd}-{slotTime:hh\\:mm}";

                        //checking if there is an app at this day n time
                        if (!appointments.ContainsKey(key))
                            continue; //no booking = skip next slot

                        AppointmentInfo appt = appointments[key];

                        //Calculating how many rows the booking spans, math ceiling rounding up and math max ensuring at least 1 row

                        int rowSpan = Math.Max(1, (int)Math.Ceiling(appt.DurationMinutes / 30.0));

                        int gridRow = slot + 1; // +1 because first row is header

                        // Building the booking block

                        Border bookingBlock = new Border
                        {
                            Background = (Brush)FindResource("PrimaryBrush"),
                            BorderBrush = (Brush)FindResource("DarkBrush"),
                            BorderThickness = new Thickness(1),
                            CornerRadius = new CornerRadius(4),
                            Padding = new Thickness(4, 2, 4, 2),
                            Margin = new Thickness(1)
                        };

                        //Building the text inside the booking block

                        TextBlock bookingText = new TextBlock
                        {
                            Text = $"{appt.ClientName}\n{appt.TreatmentName}",
                            Foreground = (Brush)FindResource("TextLightBrush"),
                            FontSize = 11,
                            FontWeight = FontWeights.SemiBold,
                            TextWrapping = TextWrapping.Wrap,       // Ensures text wraps within the block, doesnt get cut off
                            VerticalAlignment = VerticalAlignment.Top,
                            TextTrimming = TextTrimming.CharacterEllipsis  // Adds "..." if text is too long for the block
                        };

                        //tooltip shows full details when hovering over the booking block
                        bookingBlock.ToolTip = new ToolTip
                        {
                            Content = $"{appt.ClientName}\n{appt.TreatmentName}\n{appt.DurationMinutes} mins" + (string.IsNullOrEmpty(appt.Notes) ? "" : $"\nNotes: {appt.Notes}")
                        };

                        bookingBlock.Child = bookingText;

                        //positioning the block in the grid

                        Grid.SetRow(bookingBlock, gridRow);
                        Grid.SetColumn(bookingBlock, col);
                        Grid.SetRowSpan(bookingBlock, rowSpan);
                        //setrowspan ensures the block spans multiple rows if the appointment is longer than 30 minutes

                        //booking block MUST be added to the grid AFTER the empty cells are created, otherwise it will be hidden behind them

                        weekGrid.Children.Add(bookingBlock);

                        // OLD VER -- bookingBlock.IsHitTestVisible = false; Ensures the booking block does not intercept mouse clicks, allowing the underlying cell to be clicked instead 
                        AppointmentInfo capturedAppt = appt; // Capture the appointment for the lambda
                        DateTime capturedDay = thisDay; // Capture the day for the lambda
                        TimeSpan capturedTime = slotTime; // Capture the time for the lambda

                        bookingBlock.Cursor = Cursors.Hand; // Change cursor to hand to indicate it's clickable
                        bookingBlock.MouseEnter += (s, e) =>
                        {
                            bookingBlock.Background = (Brush)FindResource("DarkBrush");
                        };
                        bookingBlock.MouseLeave += (s, e) =>
                        {
                            // Reset the background color when the mouse leaves
                            bookingBlock.Background = (Brush)FindResource("PrimaryBrush");
                        };
                        bookingBlock.MouseLeftButtonUp += (s, e) =>
                        {
                            // Stop click from reaching cell underneath - day cell
                            e.Handled = true;
                            CalendarBooking_Clicked(capturedAppt, capturedDay, capturedTime);
                        };
                    }
                }
            }
            catch (SqlException ex)
            {
                ShowDbError("loading week appointments", ex);
            }
        }

        //Records selection handler
        private void dgRecordsClients_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(dgRecordsClients.SelectedItem is DataRowView drv))
                return;

            DataRow row = drv.Row;
            int? clientId = row.Field<int?>("ClientID");
            if (!clientId.HasValue)
                return;

            //updating the history label with client name
            string name = $"{row.Field<string>("FirstName")} " + $"{row.Field<string>("Surname")}";
            txtAppointmentHistoryLabel.Text = $"Appointment History - {name}";

            //load their appts into lower grid
            LoadClientAppointments(clientId.Value);
        }
        private void LoadClientAppointments(int clientId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                        SELECT
                            a.AppointmentDate, a.AppointmentTime, t.TreatmentName, t.Price, a.Notes
                        FROM Appointments a
                        JOIN Treatments t ON a.TreatmentID = t.TreatmentID
                        WHERE a.ClientID = @ClientID
                        ORDER BY
                            a.AppointmentDate DESC,
                            a.AppointmentTime DESC";
                    //order by DESC means most recent apps first

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@ClientID", clientId);

                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable table = new DataTable();
                    adapter.Fill(table);
                    dgClientAppointments.ItemsSource = table.DefaultView;
                }
            }
            catch (SqlException ex)
            {
                ShowDbError("loading client appointments", ex);
            }
        }
        //Buttons
        private void PreviousWeek_Click(object sender, RoutedEventArgs e)
        {
            GenerateWeekGrid(currentWeekStart.AddDays(-7));
        }

        private void NextWeek_Click(object sender, RoutedEventArgs e)
        {
            GenerateWeekGrid(currentWeekStart.AddDays(7));
        }

        private void GoToToday_Click(object sender, RoutedEventArgs e)
        {
            GenerateWeekGrid(GetMonday(DateTime.Today));

        }

        private void CalendarCell_Clicked(DateTime date, TimeSpan time)
        {
            selectedCalendarDate = date; //store clicked date and time
            selectedCalendarTime = time;
            selectedCalendarAppointmentId = -1; // clear any previous edit selection

            // Update the date/time label in the panel
            txtPanelDateTime.Text = $"{date:dddd d MMM  yyyy}\nat {time:hh\\:mm}";

            cbPanelClients.SelectedValue = 10; //walk-in default

            cbPanelTreatments.SelectedIndex = -1;
            txtPanelNotes.Clear();

            SetPanelMode(PanelMode.Book); // Set the panel to booking mode

            sidePanel.Visibility = Visibility.Visible; // Show the side panel

        }
        
        private void CalendarBooking_Clicked(AppointmentInfo appt, DateTime date, TimeSpan time)
        {
            // store which appt im editing
            selectedCalendarAppointmentId = appt.AppointmentId;
            selectedCalendarDate = date;
            selectedCalendarTime = time;

            // Updating the header label
            txtPanelDateTime.Text = $"{date:ddd d MMM yyyy}\nat {time:hh\\:mm}";

            dpPanelDate.SelectedDate = date;
            cbPanelTime.SelectedItem = time.ToString(@"hh\:mm");

            // Pre filling the comboboxes with existing values
            // SelectedValue matches by ClientID/TreatmentID
            cbPanelClients.SelectedValue = appt.ClientId;
            cbPanelTreatments.SelectedValue = appt.TreatmentId;
            txtPanelNotes.Text = appt.Notes;

            SetPanelMode(PanelMode.Edit); // Set the panel to edit mode
            sidePanel.Visibility = Visibility.Visible;
        }
        private void PanelDeleteAppointment_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = CustomMessageBox.ShowConfirm("Are you sure you want to delete this appointment?", "Confirm Delete", MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query =
                        "DELETE FROM Appointments WHERE AppointmentsID = @AppointmentsID";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@AppointmentsID", selectedCalendarAppointmentId);
                    command.ExecuteNonQuery();
                }
           
            LoadAppointments();
            GenerateWeekGrid(currentWeekStart);
            sidePanel.Visibility = Visibility.Collapsed;
            selectedCalendarAppointmentId = -1;
            CustomMessageBox.Show("Appointment deleted successfully");
            }
            catch (SqlException ex)
            {
                ShowDbError("panel deleting appointment", ex);
            }
        }
        private void PanelUpdateAppointment_Click(object sender, RoutedEventArgs e)
        {
            if (cbPanelClients.SelectedValue == null)
            {
                CustomMessageBox.Show("Please select a client.");
                return;
            }
            if (cbPanelTreatments.SelectedValue == null)
            {
                CustomMessageBox.Show("Please select a treatment.");
                return;
            }

            //Read from editable controls instead of stored fields
            DateTime? newDate = dpPanelDate.SelectedDate;
            TimeSpan? newTime = null;
            if (cbPanelTime.SelectedItem is string t)
                newTime = TimeSpan.Parse(t);

            if (newDate == null || newTime == null)
            {
                CustomMessageBox.Show("Please select a date and time.");
                return;
            }
            try
            {
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
                        Notes = @Notes
                    WHERE AppointmentsID = @AppointmentsID";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@ClientID", cbPanelClients.SelectedValue);
                    command.Parameters.AddWithValue("@TreatmentID", cbPanelTreatments.SelectedValue);
                    command.Parameters.AddWithValue("@AppointmentDate", newDate.Value.Date);

                    var p = command.Parameters.Add("@AppointmentTime", SqlDbType.Time);
                    p.Value = newTime.Value;

                    command.Parameters.AddWithValue("@Notes", txtPanelNotes.Text);
                    command.Parameters.AddWithValue("@AppointmentsID", selectedCalendarAppointmentId);

                    command.ExecuteNonQuery();
                }


            LoadAppointments();
            GenerateWeekGrid(currentWeekStart); // Refresh the week grid to reflect changes
            sidePanel.Visibility = Visibility.Collapsed; // Close the side panel
            CustomMessageBox.Show("Appointment Updated Successfully.");
            }
            catch (SqlException ex)
            {
                ShowDbError("panel updating appointment", ex);
            }
        }
        private void SetPanelMode(PanelMode mode)
        {
            currentPanelMode = mode;

            if (mode == PanelMode.Book)
            {
                //show book button hide edit buttons
                pnlDateTimeDisplay.Visibility = Visibility.Visible;
                pnlDateTimeEdit.Visibility = Visibility.Collapsed;
                btnPanelBook.Visibility = Visibility.Visible;
                pnlEditButtons.Visibility = Visibility.Collapsed;
            }
            else //edit
            {
                pnlDateTimeDisplay.Visibility= Visibility.Collapsed;
                pnlDateTimeEdit.Visibility = Visibility.Visible;
                btnPanelBook.Visibility = Visibility.Collapsed;
                pnlEditButtons.Visibility = Visibility.Visible;
            }
        }
        private void CloseSidePanel_Click(object sender, RoutedEventArgs e)
        {
            sidePanel.Visibility = Visibility.Collapsed; // Hide the side panel
        }

        private void PanelBookAppointment_Click(object sender, RoutedEventArgs e)
        {
            // Validate that a client and treatment are selected

            if (cbPanelClients.SelectedValue == null)
            {
                CustomMessageBox.Show("Please select a client.");
                return; 
            }
            if (cbPanelTreatments.SelectedValue == null)
            {
                CustomMessageBox.Show("Please select a treatment.");
                return;
            }
            try
            {
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
                    command.Parameters.AddWithValue("@ClientID", cbPanelClients.SelectedValue);
                    command.Parameters.AddWithValue("@TreatmentID", cbPanelTreatments.SelectedValue);
                    command.Parameters.AddWithValue("@AppointmentDate", selectedCalendarDate.Date);

                    var p = command.Parameters.Add("@AppointmentTime", SqlDbType.Time);
                    p.Value = selectedCalendarTime;

                    command.Parameters.AddWithValue("@Notes", txtPanelNotes.Text);

                    command.ExecuteNonQuery();
                }


            //refresh everything
            LoadAppointments();
            LoadClientComboBox();
            GenerateWeekGrid(currentWeekStart);

            // close panel
            sidePanel.Visibility = Visibility.Collapsed;

            CustomMessageBox.Show("Appointment Booked Successfully.");
            }
            catch (SqlException ex)
            {
                ShowDbError("panel booking appointment", ex);
            }
        }

    }


}
