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

        public MainWindow()
        {
            InitializeComponent();
            LoadClients();
            LoadClientComboBox();
            LoadTreatmentComboBox();
            LoadAppointments();
            LoadTreatments();
            LoadTimeSlots();
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
                command.Parameters.AddWithValue("@DOB", dpDOB.SelectedDate ?? (object)DBNull.Value);
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
                command.Parameters.AddWithValue("@ConsentDate", dpConsentDate.SelectedDate ?? (object)DBNull.Value);

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

            MessageBox.Show("Client added successfully.");
        }
        private void AddTreatment_Click(object sender, RoutedEventArgs e)          //Add treatment click method
        {
            // Basic validation
            if (string.IsNullOrWhiteSpace(txtTreatmentName.Text))
            {
                MessageBox.Show("Please enter a treatment name.");
                return;
            }

            if (!decimal.TryParse(txtPrice.Text, out decimal price))
            {
                MessageBox.Show("Please enter a valid price (e.g. 45.00)");
                return;
            }
            int? duration = null;
            if (int.TryParse(txtDurationMinutes.Text, out int d))
                duration = d;
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
            MessageBox.Show("Treatment added successfully.");
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
                command.Parameters.AddWithValue("@DOB", dpDOB.SelectedDate ?? (object)DBNull.Value);
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
                command.Parameters.AddWithValue("@ConsentDate", dpConsentDate.SelectedDate ?? (object)DBNull.Value);

                command.Parameters.AddWithValue("@ClientID", selectedClientId);

                command.ExecuteNonQuery();
            }

            LoadClients();
            LoadClientComboBox();

            MessageBox.Show("Client Updated Successfully");
        } 
        private void UpdateAppointment_Click(object sender, RoutedEventArgs e) // Update Appointment info method
        {
            if (selectedAppointmentId == -1)
            {
                MessageBox.Show("Please select an Appointment.");
                return;
            }
            TimeSpan? parsedTime = null;
            if (cbAppointmentTime.SelectedItem is string selectedTime)
                parsedTime = TimeSpan.Parse(selectedTime);

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

            MessageBox.Show("Appointment Updated Successfully");
        }
        private void UpdateTreatment_Click(object sender, RoutedEventArgs e) // Update treatment info method
        {
            if (selectedTreatmentId == -1)
            {
                MessageBox.Show("Please select a treatment.");
                return;
            }
            if (!decimal.TryParse(txtPrice.Text, out decimal price))
            {
                MessageBox.Show("Please enter a valid price (e.g. 45.00)");
                return;
            }
            //Parse duration saferly
            int? duration = null;
            if (int.TryParse(txtDurationMinutes.Text, out int d))
                duration = d;

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

            MessageBox.Show("Treatment Updated Successfully");
        }

        private void DeleteClient_Click(Object sender, RoutedEventArgs e)       //Delete client click method
        {
            if(selectedClientId == -1)
            {
                MessageBox.Show("Please select a client.");
                return;
            }

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
                MessageBoxResult result = MessageBox.Show(
                    "This client has an existing appointment. \n\n" +
                    "Click YES to delete the client AND all their appointments. \n" +
                    "Click NO to cancel.",
                    "Client Has Appointments",
                    MessageBoxButton.YesNo,
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

            MessageBox.Show("Client deleted successfully.");
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
            cbAppointmentTime.SelectedItem = null;
            txtAppointmentNotes.Clear();
            dpAppointmentDate.SelectedDate = null;


            MessageBox.Show("Appointment deleted successfully.");
        }
        private void DeleteTreatment_Click(Object sender, RoutedEventArgs e)       //Delete treatment click method
        {
            if (selectedTreatmentId == -1)
            {
                MessageBox.Show("Please select a treatment.");
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Are you sure you want to delete this treatment?",
                "Confirm Delete",
                MessageBoxButton.YesNo);

            if (result != MessageBoxResult.Yes)
                return;

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

            MessageBox.Show("Treatment deleted successfully.");
        }

        private void LoadTimeSlots()
        {
            var times = new List<string>();
            //Loop every hour and ever 5 min intervals
            for (int hour = 0; hour < 24; hour++)
            {
                for (int minute = 0; minute < 60; minute += 5)
                {
                    times.Add($"{hour:D2}:{minute:D2}");
                }
            }
            cbAppointmentTime.ItemsSource = times;
        }
        private void LoadClientComboBox()               //Client ComboBox Method
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
            if (cbAppointmentTime.SelectedItem is string selectedTime)
                parsedTime = TimeSpan.Parse(selectedTime);

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

                Border headerCell = new Border
                {
                    Background = (Brush)FindResource("MidLavBrush"),
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
                    Background = (Brush)FindResource("BackgroundBrush"),
                    BorderBrush = (Brush)FindResource("BorderBrush"),
                    BorderThickness = new Thickness(0.5),
                    Padding = new Thickness(4, 2, 4, 2)
                };

                TextBlock timeText = new TextBlock
                {
                    Text = slotTime.ToString("HH:mm"),
                    FontSize = 11,
                    Foreground = (Brush)FindResource("TextMutedBrush"),
                    VerticalAlignment = VerticalAlignment.Center
                };

                timeCell.Child = timeText;

                Grid.SetRow(timeCell, row + 1); // +1 because first row is header
                Grid.SetColumn(timeCell, 0);

                for (int day = 0; day < 7; day++)
                {
                    Border dayCell = new Border
                    {
                        Background = (Brush)FindResource("SurfaceBrush"),
                        BorderBrush = (Brush)FindResource("BorderBrush"),
                        BorderThickness = new Thickness(0.5)
                    };

                    Grid.SetRow(dayCell, row + 1);
                    Grid.SetColumn(dayCell, day + 1); // +1 because first column is time

                    weekGrid.Children.Add(dayCell);
                }

                slotTime = slotTime.AddMinutes(30); // Increment by 30 minutes
            }

        }
        private void PreviousWeek_Click(object sender, RoutedEventArgs e)
        {

        }

        private void NextWeek_Click(object sender, RoutedEventArgs e)
        {

        }

        private void GoToToday_Click(object sender, RoutedEventArgs e)
        {

        }
        

    }


}
