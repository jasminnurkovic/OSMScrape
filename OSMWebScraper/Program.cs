using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using System.Net.Http;
using System.Text;
using System.IO;

namespace OSMWebScraper
{
    class Program
    {
        static void Main(string[] args)
        {
            /*Console.WriteLine("Please enter supported currency in accepted format (e.g. USD)");
            string inputCurrency = Console.ReadLine().ToUpper();
            Console.WriteLine("Please enter save directory");
            string savePath = Console.ReadLine();
            Console.WriteLine("Please enter file name");
            string fileName = Console.ReadLine();*/
            GetHtmlAsync("USD",@"C:\Users\Jasmin\Desktop","Probasvi");
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
            if(responseString == null)
            {
                Console.WriteLine("Failed to connect to website");
                return;
            }

            var recCount = responseString.Substring(responseString.IndexOf("m_nRecordCount"));
            recCount = recCount.Substring(recCount.IndexOf("=") + 1, recCount.IndexOf(";") - recCount.IndexOf("=") + 1);
            Console.WriteLine(recCount);
            return;

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(responseString);

            var TransactionList = htmlDocument
                .DocumentNode.Descendants("table")
                .Where(node => node.GetAttributeValue("width", "")
                .Equals("640")).ToList();

            var TransactionListItems = TransactionList[0].Descendants("tr").ToList();

            String result = "";

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
            fName += ".csv";
            path += @"\" + fName;
            File.WriteAllText(path, result);
        }
    }
}
