using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace TestTaskParser
{
    class Program
    {
        private static SqlConnection sqlConnection;

        static void Main(string[] args)
        {
            string[] partsNumber = PartNumberGet();
            string[] manualParts = new string[] { "100124620", "100124652", "100331120" };
            string connectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=TestCaseDb;Integrated Security=True";
            sqlConnection = new SqlConnection(connectionString);

            try
            {
                sqlConnection.Open();
            }
            catch (SqlException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("SQL CONNECTION ERROR");
                Console.ResetColor();
            }
            if (sqlConnection.State.ToString() == "Open")
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("SQL Connected");
                Console.ResetColor();
            }

            PartParser(partsNumber);
            sqlConnection.Close();
            Console.Read();
        }

        /// <summary>
        /// Gets part numbers from file
        /// </summary>
        /// <returns></returns>
        private static string[] PartNumberGet()
        //PartNumberToArray - DONE
        {
            string[] PartsCodes;
            Console.WriteLine("Reading File with parts to parse...");
            try
            {
                PartsCodes = File.ReadAllLines("CodesToParse.txt");                
            }
            catch(FileNotFoundException)
            {
                PartsCodes = null;
                Console.WriteLine("File not found!!!\nPlease add file and restart the programm");
                Console.ReadKey();
                Environment.Exit(0);
            }
            finally
            {
                Console.WriteLine("Success reading file.");
            }

            if(PartsCodes == null)
            {
                Console.WriteLine("File was empty.");
                Environment.Exit(0);
            }

            for (int arrayIndex = 0; arrayIndex < PartsCodes.Length; arrayIndex++)
            {
                var partCode = PartsCodes[arrayIndex].Split(' ');
                PartsCodes[arrayIndex] = partCode[0];
            }

            return PartsCodes;
        }

        /// <summary>
        /// Checks if part needs to be parsed.
        /// </summary>
        /// <param name="partToCheck"></param>
        /// <returns></returns>
        private static bool PartNeedsParse(string partToCheck)
        //Check if part was already parsed - DONE
        {
            if (!File.Exists("parser.log"))
                return true;

            string[] parserLog = File.ReadAllLines("parser.log");
            bool partNeedsParse = true;

            foreach (string part in parserLog)
            {
                if (part.Contains(partToCheck))
                {
                    if (part.Contains("ERROR"))
                    {
                        break;
                    }
                    else
                        partNeedsParse = false;
                }
                else
                    continue;
            }
            return partNeedsParse;
        }

        /// <summary>
        /// Parses array of parts and calls method to wtite it to DB.
        /// </summary>
        /// <param name="partNumbers"></param>
        private static void PartParser(string[] partNumbers)
        //Parser - 80% DONE; TODO: XML AND LINKED PARTS
        {
            //List<Part> partsResult = new List<Part>();
            Dictionary<string, string> patterns = new Dictionary<string, string>
            {
                { "patternBrand", "<td class=ProdBra>(?<result>.+)</td>" }
                ,{ "patternArt", "<td class=ProdArt>(?<result>.+)"}
                ,{ "patternName", "<td class=ProdName>(?<result>.+)</td>"}
                ,{ "patternCriteria", "<span class=criteria>(?<result>.+)</span><br>"}
            };
            bool parseSuccess = true;

            foreach (string partNumber in partNumbers)
            {
                Part parsedPart = new Part();
                if (PartNeedsParse(partNumber))
                {
                    //Link generation
                    string siteToParse = "http://otto-zimmermann.com.ua/autoparts/product/ZIMMERMANN/";
                    siteToParse += partNumber + @"/";
                    parsedPart.PartUrl = siteToParse;

                    //Download HTML
                    Console.WriteLine("\nGetting HTML from:\n{0}", siteToParse);
                    WebClient client = new WebClient
                    {
                        Encoding = Encoding.UTF8
                    };
                    string downloadedHTML = client.DownloadString(siteToParse);

                    Regex regexB = new Regex(patterns["patternBrand"]);
                    MatchCollection matchesB = regexB.Matches(downloadedHTML);
                    if (matchesB.Count == 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("PAGE NOT FOUND");
                        Console.ResetColor();
                        parseSuccess = false;
                        Logger(partNumber, parseSuccess);
                        continue;
                    }
                    File.AppendAllText("html.txt", downloadedHTML);

                    //Parsing
                    Console.WriteLine("Trying to parse...");
                    foreach(var pattern in patterns)
                    {
                        Regex regex = new Regex(pattern.Value.ToString());
                        MatchCollection matches = regex.Matches(downloadedHTML);
                        if (matches.Count > 0)
                        {
                            switch (pattern.Key)
                            {
                                case "patternBrand":
                                    parsedPart.PartBrand = matches[0].Groups["result"].Value.ToString();
                                    break;
                                case "patternArt":
                                    parsedPart.PartArtNumber = matches[0].Groups["result"].Value.ToString();
                                    break;
                                case "patternName":
                                    parsedPart.PartName = matches[0].Groups["result"].Value.ToString();
                                    break;
                                case "patternCriteria":
                                    foreach (Match match in matches)
                                        parsedPart.PartSpecs.Add(match.Groups["result"].Value.ToString());
                                    parsedPart.PartSpecs = parsedPart.PartSpecs.Distinct().ToList();
                                    break;
                                case "patternCriteriaNEW":
                                    foreach (Match match in matches)
                                    {
                                        Console.WriteLine(match.Groups["result"].Value.ToString()); 
                                    }
                                    Console.Read();
                                    break;
                                default:
                                    break;
                            }
                            parseSuccess = true;
                        }
                        else
                        {
                            parseSuccess = false;
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("ERROR ON PATTERN: " + pattern.Key.ToString() +"\t" + pattern.Value.ToString());
                            Console.ResetColor();
                        }
                    }
                }
                else
                    continue;
                if (parseSuccess)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("PARSING SUCCESSFUL");
                    Console.ResetColor();
                    Console.WriteLine(parsedPart.ToString());
                    PartToDbWriter(parsedPart);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("PARSING ERROR");
                    Console.ResetColor();
                }
                Logger(partNumber, parseSuccess);
            }
        }

        /// <summary>
        /// Writes part to DB.
        /// </summary>
        /// <param name="part"></param>
        private static void PartToDbWriter(Part part)
        //WRITING PART TO DB - DONE
        {
            try
            {
                string sqlExpression = "sp_InsertPart";
                SqlCommand sqlCommand = new SqlCommand(sqlExpression, sqlConnection)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };
                sqlCommand.Parameters.AddRange(new SqlParameter[]
                    {
                        new SqlParameter("@URL",        value: part.PartUrl),
                        new SqlParameter("@BrandName",  value: part.PartBrand),
                        new SqlParameter("@ArtNumber",  value: part.PartArtNumber),
                        new SqlParameter("@PartName",   value: part.PartName),
                        new SqlParameter("@Specs",      value: string.Join("\n", part.PartSpecs.ToArray()))
                    });
                sqlCommand.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("SQL INSERTING ERROR");
                Console.ResetColor();
            }
            finally
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("SQL QUERY  SUCCESS");
                Console.ResetColor();
            }

        }

        /// <summary>
        /// Logs part number and parser (un)success state
        /// </summary>
        /// <param name="parsedNumber"></param>
        /// <param name="parseSuccess"></param>
        private static void Logger(string parsedNumber, bool parseSuccess)
        //LOG TO FILE - DONE
        {
            if(parseSuccess)
                File.AppendAllText("parser.log", parsedNumber + " SUCCESS" + "\n");
            else
                File.AppendAllText("parser.log", parsedNumber + " ERROR" + "\n");
        }
    }
}
