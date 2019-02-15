using System;
using System.Collections.Generic;
using System.Configuration;
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
        static void Main(string[] args)
        {
            var manualParts = new string[] { "100124652", "100331120" };
            var partsNumber = PartNumberGet();
            PartParser(partsNumber);

            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;  

            Console.Read();
        }

        public static string[] PartNumberGet()
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

        public static bool PartNeedsParse(string partToCheck)
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

        public static void ParserStart(string[] PartsCodes)
        //Start parsing in 4 threads - TODO
        {
            Thread[] parsers = new Thread[4];
            for (int i = 0; i < parsers.Length; i++)
            {
                parsers[i] = new Thread(() => PartParser(PartsCodes));
                parsers[i].Start();
            }
        }

        public static void PartParser(string[] partNumbers)
        //Parser
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
                    parsedPart.Url = siteToParse;

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
                        Console.WriteLine("Page Not Found");
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
                                    parsedPart.ArtNumber = matches[0].Groups["result"].Value.ToString();
                                    break;
                                case "patternName":
                                    parsedPart.PartName = matches[0].Groups["result"].Value.ToString();
                                    break;
                                case "patternCriteria":
                                    foreach (Match match in matches)
                                        parsedPart.Specs.Add(match.Groups["result"].Value.ToString());
                                    parsedPart.Specs = parsedPart.Specs.Distinct().ToList();
                                    break;
                                default:
                                    break;
                            }
                            parseSuccess = true;
                        }
                        else
                        {
                            parseSuccess = false;
                            Console.WriteLine("Error on pattern: " + pattern.Key.ToString() +"\t" + pattern.Value.ToString());
                        }
                    }
                }
                else
                    continue;
                if (parseSuccess)
                {
                    Console.WriteLine(parsedPart.ToString());
                }
                else { Console.WriteLine("ERROR"); }
                Logger(partNumber, parseSuccess);
            }
        }

        public static void PartToDbWriter(Part part)
        //writing to DB
        {

        }

        public static void Logger(string parsedNumber, bool parseSuccess)
        {
            if(parseSuccess)
                File.AppendAllText("parser.log", parsedNumber + " SUCCESS" + "\n");
            else
                File.AppendAllText("parser.log", parsedNumber + " ERROR" + "\n");
        }
    }
}
