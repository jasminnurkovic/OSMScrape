using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace OSMWebScraper
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Please enter supported currency in accepted format (e.g. USD)");
            string inputCurrency = Console.ReadLine().ToUpper();
            Console.WriteLine("Please enter save directory");
            string savePath = Console.ReadLine();
            Console.WriteLine("Please enter file name");
            string fileName = Console.ReadLine();
            GetHtmlAsync(inputCurrency, savePath, fileName);
            Console.ReadLine();
        }

        private static async void GetHtmlAsync(String currency, String path, String fName)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            DateTime dateEnd = DateTime.Now;
            DateTime dateStart = dateEnd.AddDays(-2);

            var url = "https://srh.bankofchina.com/search/whpj/searchen.jsp";
            var httpClient = new HttpClient();
            var html = await httpClient.GetStringAsync(url);


            var values = new Dictionary<string, string>
            {
                { "erectDate", dateStart.ToString("yyyy-MM-dd")},
                { "nothing", dateEnd.ToString("yyyy-MM-dd")},
                {"pjname", currency}
            };

            var content = new FormUrlEncodedContent(values);

            var response = await httpClient.PostAsync(url, content);

            var responseString = await response.Content.ReadAsStringAsync();
            if (responseString == null)
            {
                Console.WriteLine("Failed to connect to website");
                return;
            }

            var recCount = responseString.Substring(responseString.IndexOf("m_nRecordCount"));
            recCount = recCount.Substring(recCount.IndexOf("=") + 2, recCount.IndexOf(";") - recCount.IndexOf("=") - 2);



            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(responseString);

            var TransactionList = htmlDocument
                .DocumentNode.Descendants("table")
                .Where(node => node.GetAttributeValue("width", "")
                .Equals("640")).ToList();

            var TransactionListItems = TransactionList[0].Descendants("tr").ToList();

            String result = "";

            Console.WriteLine("Starting first page");
            foreach (var TransactionListItem in TransactionListItems)
            {
                int counter = 0;
                for (int i = 0; i < 7; i++)
                {
                    if (counter < 6)
                    {
                        result += TransactionListItem.Descendants("td").ElementAtOrDefault(i).InnerText + ",";
                        //Console.Write(TransactionListItem.Descendants("td").ElementAtOrDefault(i).InnerText + ",");
                        counter++;
                    }
                    else
                    {
                        result += TransactionListItem.Descendants("td").ElementAtOrDefault(i).InnerText + "\n";
                        //Console.WriteLine(TransactionListItem.Descendants("td").ElementAtOrDefault(i).InnerText + "\n");
                    }
                }
            }

            for (int j = 2; j <= Math.Ceiling(Double.Parse(recCount) / 20); j++)
            {
                Console.WriteLine("Starting page " + j);
                values = new Dictionary<string, string>
            {
                { "erectDate", dateStart.ToString("yyyy-MM-dd")},
                { "nothing", dateEnd.ToString("yyyy-MM-dd")},
                {"pjname", currency},
                {"page", j+""}
            };

                content = new FormUrlEncodedContent(values);

                response = await httpClient.PostAsync(url, content);

                responseString = await response.Content.ReadAsStringAsync();

                htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(responseString);

                TransactionList = htmlDocument
                    .DocumentNode.Descendants("table")
                    .Where(node => node.GetAttributeValue("width", "")
                    .Equals("640")).ToList();

                TransactionListItems = TransactionList[0].Descendants("tr").ToList();

                bool header = true;

                foreach (var TransactionListItem in TransactionListItems)
                {
                    if (header == true)
                    {
                        header = false;
                        continue;
                    }
                    int counter = 0;
                    for (int i = 0; i < 7; i++)
                    {
                        if (counter < 6)
                        {
                            result += TransactionListItem.Descendants("td").ElementAtOrDefault(i).InnerText + ",";
                            //Console.Write(TransactionListItem.Descendants("td").ElementAtOrDefault(i).InnerText + ",");
                            counter++;
                        }
                        else
                        {
                            result += TransactionListItem.Descendants("td").ElementAtOrDefault(i).InnerText + "\n";
                            //Console.WriteLine(TransactionListItem.Descendants("td").ElementAtOrDefault(i).InnerText + "\n");
                        }
                    }
                }
            }
            fName += ".csv";
            path += @"\" + fName;
            File.WriteAllText(path, result);
            Console.WriteLine("Finished");
        }
    }
}
