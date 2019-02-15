using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace TestTaskParser
{
    class Program
    {
        private static SqlConnection sqlConnection;

        static void Main(string[] args)
        {
            string[] partsNumber = PartNumberGet();
            //string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
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

        private static void ParserStart(string[] PartsCodes)
        //Start parsing in 4 threads - TODO
        {
            Thread[] parsers = new Thread[4];
            for (int i = 0; i < parsers.Length; i++)
            {
                parsers[i] = new Thread(() => PartParser(PartsCodes));
                parsers[i].Start();
            }
        }

        private static void PartParser(string[] partNumbers)
        //Parser - 80% DONE; TODO: XML AND LINKED PARTS
        {
            //List<Part> partsResult = new List<Part>();
            Dictionary<string, string> patterns = new Dictionary<string, string>
            {
                { "patternBrand", "<td class=ProdBra>(?<result>.+)</td>" },
                { "patternArt", "<td class=ProdArt>(?<result>.+)"},
                { "patternName", "<td></td><td class=ProdName>(?<result>.+)</td>"},
                { "patternCriteria", "<span class=criteria>(?<result>.+)</span><br>"}
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

        private static void PartToDbWriter(Part part)
        //WRITING PART TO DB - TODO
        {
            try
            {
                string sqlExpression = String.Format
                    ("INSERT INTO dbo.Parts (URL, ArtNumber, BrandName, PartName, Specs) VALUES ( '{0}', '{1}', '{2}', '{3}', '{4}' )",
                    part.PartUrl, part.PartArtNumber, part.PartBrand, part.PartName, part.PartSpecs);
                SqlCommand sqlCommand = new SqlCommand(sqlExpression, sqlConnection);
                sqlCommand.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("SQL INSERTING ERROR");
#if DEBUG
                Console.WriteLine(ex.ToString());
                Console.Read();
#endif
                Console.ResetColor();
            }
            finally
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("SQL QUERY  SUCCESS");
                Console.ResetColor();
            }

        }

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
