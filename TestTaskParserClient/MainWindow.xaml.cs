using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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

            Thread.Sleep(1000);
        }
        
        /// <summary>
        /// Changes DB connection state
        /// </summary>
        private void DbChangeConnectionState()
        {
            string connectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=TestCaseDb;Integrated Security=True";
            sqlConnection = new SqlConnection(connectionString);
            if (sqlConnection.State.ToString() == "Close")
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
        public List<Part> GetAllDataFromDB()
        {
            DbChangeConnectionState();
            List<Part> partsList = new List<Part>();
            string sqlExpression = "sp_PartByNumber";
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
    }
}
