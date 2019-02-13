using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace TestTaskConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] partsToParseManual = new string[] { "109901140", "230235220", "230701701" };
            PartParser(partsToParseManual);
            //PartNumberGet();
            Console.Read();
        }

        public static void PartNumberGet()
        {
            Console.WriteLine("Reading File");
            string[] PartCodes = File.ReadAllLines("PartCodes.txt");

            //Запуск парсера в 4 потоках
            Thread[] parsers = new Thread[4];
            for (int i = 0; i < parsers.Length; i++)
            {
                parsers[i] = new Thread ( () => PartParser(PartCodes) );
                parsers[i].Start();
            }
        }

        public static void PartParser(string[] partNumbers)
        {
            foreach (var partNumber in partNumbers)
            {            
                //Генерация ссылки
                string siteToParse = "https://otto-zimmermann.com.ua/autoparts/product/ZIMMERMANN/";
                siteToParse += partNumber + @"/";
                
                //скачивание HTML
                Console.WriteLine("\nGetting HTML from:\n{0}", siteToParse);
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
                ,"^<div class=partsDescript>.*$</div></div>"
                ,"<td><a href = \"(?<result>.+)\" > 7H0 615 301 E</a></td>"
                ,"<td class=\"tarig\">(?<result>.+)</td>"
                ,"<a href=\"(?<result>.+)\">7H0 615 301 E</a>"
                ,"<span class=\"artkind_original\">(?<result>.+)</span>"
                ,"<span class=criteria>.+</span><br>"
                };

                string[] result = new string[patterns.Length + 1];
                result[0] = siteToParse;

                bool parseSuccess = true;
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
                        //Console.WriteLine("No matches on pattern " + patterns[patternNumber].ToString());
                        parseSuccess = false;
                    }
                }
                Logger(partNumber, parseSuccess);
                Console.WriteLine("Result:");
                foreach (var str in result)
                {
                    if (str != null)
                        Console.WriteLine(str.ToString());
                    else
                        Console.WriteLine("Not found");
                }
            }            
        }

        public static void Logger(string parsedNumber, bool parseSuccess)
        {
            if(parseSuccess)
                File.AppendAllText("parser.log", DateTime.Now.ToString() + "\t" + "SUCCESS" + "\t" + parsedNumber + "\n");
            else
                File.AppendAllText("parser.log", DateTime.Now.ToString() + "\t" + "UNSUCCESS" + "\t" + parsedNumber + "\n");
        }
    }
}
