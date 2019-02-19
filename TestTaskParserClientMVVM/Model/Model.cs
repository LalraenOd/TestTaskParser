using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using TestTaskParser;

namespace TestTaskParserClientMVVM.Model
{
    public class Model
    {
        public static SqlConnection sqlConnection;

        /// <summary>
        /// Gets all data from DB.
        /// </summary>
        public static List<Part> GetAllDataFromDB()
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
        /// Changes connection state to param Open, Close
        /// </summary>
        /// <param name="connectionState"></param>
        public static void DbChangeConnectionState(string connectionState)
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
        /// Clears two initialized listViews
        /// </summary>
        public static void ClearAllListViews()
        {
            //ListViewByName.Items.Clear();
            //ListViewByNumber.Items.Clear();
        }
    }
}