using HtmlAgilityPack;
using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace Project_Kittan.Helpers
{
    /// <summary>
    /// UpdateExtensions class
    /// </summary>
    internal static class UpdateExtensions
    {
        /// <summary>
        /// Method which connects to Project Kittan releases page (GitHub) and then
        /// reads the HTML code of the web page to get the latest version published.
        /// </summary>
        /// <returns>The availability of new updates</returns>
        public static async Task<bool> Check()
        {
            try
            {
                using (HttpResponseMessage response = await new HttpClient().GetAsync(new Uri("https://github.com/matteoparlato/Project-Kittan/releases")))
                {
                    response.EnsureSuccessStatusCode();

                    HtmlDocument document = new HtmlDocument() { OptionFixNestedTags = true };
                    document.LoadHtml(await response.Content.ReadAsStringAsync());

                    HtmlNode[] subnode = document.DocumentNode.Descendants("a").Where(tag => tag.GetAttributeValue("class", "").Equals("muted-link css-truncate") && tag.GetAttributeValue("href", "").Contains("/matteoparlato/Project-Kittan/tree/")).ToArray();
                    if (subnode.Length > 0)
                    {
                        string latestVersion = subnode[0].GetAttributeValue("title", "");
                        if (latestVersion != Assembly.GetExecutingAssembly().GetName().Version.ToString())
                        {
                            return true;
                        }
                    }
                }
            }
            catch(Exception)
            {
                //
            }

            return false;
        }
    }
}
