using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;
using System.Threading;

namespace TestTaskConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            PartNumberGet();
            Console.Read();
        }

        public static void PartNumberGet()
        {
            string[] PartCodes = File.ReadAllLines("PartCodes.txt");
        }

        public void PartParser(string[] partNumbers)
        {
            //Генерация ссылки
            string siteToParse = "https://otto-zimmermann.com.ua/autoparts/product/ZIMMERMANN/";

            foreach (var partnumber in partNumbers)
            {
                siteToParse += partnumber + @"/";
            }

            //скачивание HTML
            Console.WriteLine("Getting HTML from:\n{0}", siteToParse);
            WebClient client = new WebClient
            {
                Encoding = Encoding.UTF8
            };
            string downloadedHTML = client.DownloadString(siteToParse);

            //Парсинг
            Console.WriteLine("Trying to parse...");

            string[] patterns = new string[]
            {
                "<td class=ProdBra>(?<result>.+)</td>"
                ,"<td class=ProdArt>(?<result>.+)"
                ,"<td class=ProdName>(?<result>.+)</td>"
                //,"^<div class=partsDescript>.*$</div></div>"
                //,"<td><a href = \"(?<result>.+)\" > 7H0 615 301 E</a></td>"
                //,"<td class=\"tarig\">(?<result>.+)</td>"
                //,"<a href=\"(?<result>.+)\">7H0 615 301 E</a>"
                //,"<span class=\"artkind_original\">(?<result>.+)</span>"
                //,"<span class=criteria>.+</span><br>"
            };

            string[] result = new string[patterns.Length + 1];
            result[0] = siteToParse;

            for (int patternNumber = 0; patternNumber < patterns.Length; patternNumber++)
            {
                Regex regex = new Regex(patterns[patternNumber]);
                MatchCollection matches = regex.Matches(downloadedHTML);
                if (matches.Count > 0)
                {
                    foreach (Match match in matches)
                    {
                        if (patternNumber != 3)
                            result[patternNumber + 1] = match.Groups["result"].Value.ToString();
                        else
                            result[patternNumber + 1] += match;
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("No matches on pattern " + patterns[patternNumber].ToString());
                    Console.Read();
                }
            }
            Logger(siteToParse);

            foreach (var str in result)
            {
                if(str != null)
                    Console.WriteLine("\nResult:\n" + str.ToString());
                else
                    Console.WriteLine("\nResult:\nNULL");
            }
        }

        public static void Logger(string parsedLink, bool parseSuccessed = true)
        {
            File.AppendAllText("parser.log", DateTime.Now.ToString() + "\t" + parsedLink + "\n");
        }
    }
}

/*
string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

using (SqlConnection connection = new SqlConnection(connectionString))
{
    SqlCommand command = new SqlCommand("", connection);
}
*/

