using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using TestTaskParser;

namespace TestTaskParserClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static SqlConnection sqlConnection;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            List<Part> parts = GetAllDataFromDB();
            ListToListView(parts);
        }

        /// <summary>
        /// Changes DB connection state
        /// </summary>
        private static void DbChangeConnectionState()
        {
            string connectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=TestCaseDb;Integrated Security=True";
            sqlConnection = new SqlConnection(connectionString);
            if (sqlConnection.State.ToString() == "Closed")
            {
                try
                {
                    sqlConnection.Open();
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("SQL CONNECTION ERROR\n" + ex);
                    throw;
                }
                finally
                {
                    MessageBox.Show("SQL CONNECTED SUCCESSFULLY");
                }
            }
            else if (sqlConnection.State.ToString() == "Open")
            {
                try
                {
                    sqlConnection.Open();
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("SQL CONNECTION ERROR\n" + ex);
                    throw;
                }
                finally
                {
                    MessageBox.Show("SQL DISCONNECTED SUCCESSFULLY");
                }
            }
            else
                MessageBox.Show("SQL connection error state:\n" + sqlConnection.State.ToString());
        }

        /// <summary>
        /// Gets all data from DB.
        /// </summary>
        private List<Part> GetAllDataFromDB()
        {
            DbChangeConnectionState();
            List<Part> partsList = new List<Part>();
            string sqlExpression = "sp_GetAllParts";
            SqlCommand sqlCommand = new SqlCommand(sqlExpression, sqlConnection)
            {
                CommandType = CommandType.StoredProcedure
            };
            SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();

            while (sqlDataReader.Read())
            {
                Part part = new Part
                {
                    PartUrl = sqlDataReader["URL"].ToString(),
                    PartBrand = sqlDataReader["BrandName"].ToString(),
                    PartArtNumber = sqlDataReader["ArtNumber"].ToString(),
                    PartName = sqlDataReader["PartName"].ToString(),
                    PartSpecs = sqlDataReader["Specs"].ToString().Split('\n').ToList()
                };
                partsList.Add(part);
            }
            DbChangeConnectionState();
            return partsList;
        }

        /// <summary>
        /// Writes all List of parts to ListView
        /// </summary>
        /// <param name="parts"></param>
        private void ListToListView(List<Part> parts)
        {
            foreach (Part part in parts)
            {
                ListViewByNumber.Items.Add(new Part { PartArtNumber = part.PartArtNumber, PartBrand = part.PartBrand });
                ListViewByName.Items.Add(new Part { PartName = part.PartName });
            }
        }

        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            List<Part> parts = GetAllDataFromDB();
            ListToListView(parts);
        }

        private void Buttonexit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
