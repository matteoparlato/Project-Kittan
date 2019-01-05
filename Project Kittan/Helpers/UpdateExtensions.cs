using HtmlAgilityPack;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace Project_Kittan.Helpers
{
    internal class UpdateExtensions
    {
        /// <summary>
        /// Method which connects to Project Kittan releases page (GitHub) and then
        /// reads the HTML code of the web page to get the latest version published.
        /// </summary>
        /// <returns></returns>
        public static async Task Check()
        {
            try
            {
                HttpClient client = new HttpClient();

                HttpResponseMessage response = await client.GetAsync(new Uri("https://github.com/matteoparlato/Project-Kittan/releases"));
                response.EnsureSuccessStatusCode();

                HtmlDocument document = new HtmlDocument() { OptionFixNestedTags = true };

                document.LoadHtml(await response.Content.ReadAsStringAsync());

                HtmlNode[] subnode = document.DocumentNode.Descendants("a").Where(tag => tag.GetAttributeValue("class", "").Equals("muted-link css-truncate") && tag.GetAttributeValue("href", "").Contains("/matteoparlato/Project-Kittan/tree/")).ToArray();
                if (subnode.Length > 0)
                {
                    string latestVersion = subnode[0].GetAttributeValue("title", "");
                    if (latestVersion != Assembly.GetExecutingAssembly().GetName().Version.ToString())
                    {
                        if (MessageBox.Show("There's a new version of Project Kittan available on GitHub. Do you want to download it?", "Project Kittan", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                        {
                            Process.Start("https://github.com/matteoparlato/Project-Kittan/releases");
                        }
                    }
                }
            }
            catch(Exception) { }
        }
    }
}
