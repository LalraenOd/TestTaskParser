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

namespace TestTaskConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //Создание ссылки
            string partNumberToParser = "600323120";
            PartParser(partNumberToParser);
            Console.Read();
        }

        public static void PartParser(string partNumber)
        {
            string[] result = new string[5];
            //Генерация ссылки
            string mainLinkToParse = "https://otto-zimmermann.com.ua/autoparts/product/ZIMMERMANN/";
            string siteToParse = mainLinkToParse + partNumber + @"/";

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
                "<td class=ProdBra>(?<result>.+)</td>",
                "<td class=ProdArt>(?<result>.+)",
                "<td class=ProdName>(?<result>.+)</td>",
                "^<div class=partsDescript>.*$</div></div>"
                //"<span class=criteria>.+</span><br>",
                //"<td><a href = \"(?<result>.+)\" > 7H0 615 301 E</a></td>",
                //"<td class=\"tarig\">(?<result>.+)</td>",
                //"<a href=\"(?<result>.+)\">7H0 615 301 E</a>",
                //"<span class=\"artkind_original\">(?<result>.+)</span>"
            };

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
                    Environment.Exit(0);
                }
            }
            Logger(siteToParse);
            foreach (var str in result)
                Console.WriteLine("\nResult:\n" + str.ToString());
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

