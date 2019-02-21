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

        /// <summary>
        /// Event on window loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            List<Part> parts = GetAllDataFromDB();
            ListToListView(parts);
        }

        /// <summary>
        /// Changes connection state to param Open, Close
        /// </summary>
        /// <param name="connectionState"></param>
        private static void DbChangeConnectionState(string connectionState)
        {
            string connectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=TestCaseDb;Integrated Security=True";
            sqlConnection = new SqlConnection(connectionString);
            if (connectionState == "Open")
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
            }
            else if (connectionState == "Close")
            {
                try
                {
                    sqlConnection.Close();
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("SQL CONNECTION ERROR\n" + ex);
                    throw;
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
            ClearAllListViews();
            List<Part> partsList = new List<Part>();
            DbChangeConnectionState("Open");
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
            DbChangeConnectionState("Close");
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

        /// <summary>
        /// Gets all parts from DB filtered by number
        /// </summary>
        /// <returns></returns>
        private List<Part> GetDataFilteredByNumber(string partNumber)
        {
            List<Part> partsList = new List<Part>();
            DbChangeConnectionState("Open");
            string sqlExpression = "sp_GetPartsByNumber";
            SqlCommand sqlCommand = new SqlCommand(sqlExpression, sqlConnection)
            {
                CommandType = CommandType.StoredProcedure
            };
            sqlCommand.Parameters.Add(new SqlParameter
            {
                ParameterName = "@ArtNumberToFind",
                Value = partNumber
            });
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
            DbChangeConnectionState("Close");
            return partsList;
        }

        /// <summary>
        /// Gets all parts from DB filtered by name
        /// </summary>
        /// <returns></returns>
        private List<Part> GetDataFilteredByName(string partName)
        {
            List<Part> partsList = new List<Part>();
            DbChangeConnectionState("Open");
            string sqlExpression = "sp_GetPartsByName";
            SqlCommand sqlCommand = new SqlCommand(sqlExpression, sqlConnection)
            {
                CommandType = CommandType.StoredProcedure
            };
            sqlCommand.Parameters.Add(new SqlParameter
            {
                ParameterName = "@PartNameToFind",
                Value = partName
            });
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
            DbChangeConnectionState("Close");
            return partsList;
        }

        /// <summary>
        /// Clears two initialized listViews
        /// </summary>
        private void ClearAllListViews()
        {
            ListViewByName.Items.Clear();
            ListViewByNumber.Items.Clear();
        }

        /// <summary>
        /// Button that refreshes all the content in listViews
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            TextBoxFilterName.Clear();
            TextBoxFilterNumber.Clear();
            ClearAllListViews();
            List<Part> parts = GetAllDataFromDB();
            ListToListView(parts);
        }

        /// <summary>
        /// Exit button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Buttonexit_Click(object sender, RoutedEventArgs e)
        {
            DbChangeConnectionState("Close");
            Environment.Exit(0);
        }

        /// <summary>
        /// Event called than text is changed in TextBoxFilterNumber
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBoxFilterNumber_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (TextBoxFilterNumber.Text != "")
            {
                ClearAllListViews();
                ListToListView(GetDataFilteredByNumber(TextBoxFilterNumber.Text.ToString()));
            }
            else
            {
                List<Part> parts = GetAllDataFromDB();
                ListToListView(parts);
            }
        }

        /// <summary>
        /// Event called than text is changed in TextBoxFilterName
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBoxFilterName_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (TextBoxFilterName.Text != "")
            {
                ClearAllListViews();
                ListToListView(GetDataFilteredByName(TextBoxFilterName.Text.ToString()));
            }
            else
            {
                List<Part> parts = GetAllDataFromDB();
                ListToListView(parts);
            }
        }

        private void ListViewByNumber_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ListViewByName.SelectedIndex = ListViewByNumber.SelectedIndex;
        }

        private void ListViewByName_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ListViewByNumber.SelectedIndex = ListViewByName.SelectedIndex;
        }
    }
}
