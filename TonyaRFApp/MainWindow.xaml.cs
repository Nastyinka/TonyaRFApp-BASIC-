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
        public MainWindow()
        {
            InitializeComponent();
            LoadClients();
        }
        private void LoadClients()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Clients";
                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                DataTable table = new DataTable();
                adapter.Fill(table);
                dgClients.ItemsSource = table.DefaultView;
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

        private void AddClient_Click(object sender, RoutedEventArgs e)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
                INSERT INTO Clients
                (
                    FirstName,
                    Surname,
                    PhoneNumber
                )
                VALUES
                (
                    @FirstName,
                    @Surname,
                    @Phone
                )";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@FirstName", txtFirstName.Text);
                command.Parameters.AddWithValue("@Surname", txtSurname.Text);
                command.Parameters.AddWithValue("@Phone", txtPhone.Text);

                command.ExecuteNonQuery();
            }

            LoadClients();
            txtFirstName.Clear();
            txtSurname.Clear();
            txtPhone.Clear();
        }
        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchClients(txtSearch.Text);
        }

        private void dgClients_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgClients.SelectedItem == null)
                return;

            DataRowView row = (DataRowView)dgClients.SelectedItem;
            selectedClientId = Convert.ToInt32(row["ClientID"]);

            txtFirstName.Text = row["FirstName"].ToString();
            txtSurname.Text = row["Surname"].ToString();
            txtPhone.Text = row["PhoneNumber"].ToString();
        }

        private void UpdateClient_Click(object sender, RoutedEventArgs e)
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
                        FirstName = @FirstName
                        Surname = @Surname
                        PhoneNumber = @Phone
                    WHERE ClientID = @ClientID";

                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@FirstName", txtFirstName.Text);
                command.Parameters.AddWithValue("@Surname", txtSurname.Text);
                command.Parameters.AddWithValue("@Phone", txtPhone.Text);

                command.Parameters.AddWithValue("@ClientID", selectedClientId);

                command.ExecuteNonQuery();
            }

            LoadClients();
            MessageBox.Show("Client Updated Successfully");
        }
        private void DeleteClient_Click(Object sender, RoutedEventArgs e)
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

            txtFirstName.Clear();
            txtSurname.Clear();
            txtPhone.Clear();

            selectedClientId = -1;

            MessageBox.Show("Client deleted successfully.");
        }
    }
}
