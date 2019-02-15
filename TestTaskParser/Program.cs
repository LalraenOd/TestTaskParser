using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace TestTaskConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var partsNumber = PartNumberGet();
            PartParser(partsNumber);

            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;  

            Console.Read();
        }

        public static string[] PartNumberGet()
            //PartNumberToArray - READY
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
        {
            //Запуск парсера в 4 потоках
            Thread[] parsers = new Thread[4];
            for (int i = 0; i < parsers.Length; i++)
            {
                parsers[i] = new Thread(() => PartParser(PartsCodes));
                parsers[i].Start();
            }
        }

        public static void PartParser(string[] partNumbers)
        {
            foreach (var partNumber in partNumbers)
            {
                if (PartNeedsParse(partNumber))
                {
                    //Link generation
                    string siteToParse = "https://otto-zimmermann.com.ua/autoparts/product/ZIMMERMANN/";
                    siteToParse += partNumber + @"/";

                    //Download HTML
                    Console.WriteLine("\nGetting HTML from:\n{0}", siteToParse);
                    WebClient client = new WebClient
                    {
                        Encoding = Encoding.UTF8
                    };
                    string downloadedHTML = client.DownloadString(siteToParse);

                    //Parsing
                    Console.WriteLine("Trying to parse...");

                    string[] patterns = new string[]
                    {
                        "<td class=ProdBra>(?<result>.+)</td>"
                        ,"<td class=ProdArt>(?<result>.+)"
                        ,"<td class=ProdName>(?<result>.+)</td>"
                    //    ,"^<div class=partsDescript>(?<result>.+)</div></div>"
                    //    ,"<td><a href = \"(?<result>.+)\" > 7H0 615 301 E</a></td>"
                    //    ,"<td class=\"tarig\">(?<result>.+)</td>"
                    //    ,"<a href=\"(?<result>.+)\">7H0 615 301 E</a>"
                    //    ,"<span class=\"artkind_original\">(?<result>.+)</span>"
                        ,"<span class=criteria>(?<result>.+)</span><br>"
                    };

                    List<string> result = new List<string>();
                    result.Add(siteToParse);
                    
                    bool parseSuccess = true;
                    for (int patternNumber = 0; patternNumber < patterns.Length; patternNumber++)
                    {
                        Regex regex = new Regex(patterns[patternNumber]);
                        MatchCollection matches = regex.Matches(downloadedHTML);
                        if (patternNumber == 0 && matches.Count == 0)
                            //Check if page exists
                        {
                            Console.WriteLine("Page not found");
                            parseSuccess = false;
                            break;
                        }

                        if (matches.Count > 0)
                        {
                            foreach (Match match in matches)
                            {
                                //if (patternNumber != 3)
                                //    result[patternNumber + 1] = match.Groups["result"].Value.ToString();
                                //else
                                //    result[patternNumber + 1] += match;
                                result.Add(match.Groups["result"].Value.ToString());
                            }
                        }
                        else
                        {
                            //Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("No matches on pattern " + patterns[patternNumber].ToString());
                            parseSuccess = false;
                        }
                    }

                    Logger(partNumber, parseSuccess);
                    
                    //remove duplicates 
                    result = result.Distinct().ToList();

#if DEBUG
                    if (parseSuccess)
                    {
                        Console.WriteLine("Result:");
                        foreach (var str in result)
                        {
                            if (str != null)
                                Console.WriteLine(str.ToString());
                            else
                                Console.WriteLine("Not found");
                        }
                    }
#endif
                }
                else
                    continue;
                Console.Read();
            }            
        }

        public static void PartToDbWriter()
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
