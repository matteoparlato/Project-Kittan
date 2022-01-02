using Newtonsoft.Json;
using Project_Kittan.Models;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
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
        /// Method which calls GitHun REST API to obtain lastest release version.
        /// </summary>
        /// <returns>The availability of a new release</returns>
        public static async Task<bool> Check()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://api.github.com/");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Project-Kittan", Assembly.GetExecutingAssembly().GetName().Version.ToString()));

                    HttpResponseMessage response = await client.GetAsync("repos/matteoparlato/Project-Kittan/releases/latest");
                    if (response.IsSuccessStatusCode)
                    {
                        Release release = JsonConvert.DeserializeObject<Release>(await response.Content.ReadAsStringAsync());
                        return !(release.tag_name == Assembly.GetExecutingAssembly().GetName().Version.ToString());
                    }
                }
            }
            catch (Exception)
            {
                //
            }

            return false;
        }
    }
}
