using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LobitaBot.Services
{
    public class HttpXmlService
    {
        private static HttpClient client = new()
        {
            BaseAddress = new Uri(Literals.UrlBase)
        };

        public static void Initialize() {
            var username = Literals.ApiUser;
            var key = Literals.ApiKey;
            var encoding = Encoding.GetEncoding("iso-8859-1").GetBytes($"{username}:{key}");
            var credentials = Convert.ToBase64String(encoding);

            client.DefaultRequestHeaders.Add("User-Agent", username);
            client.DefaultRequestHeaders.Add("Authorization", $"Basic {credentials}");
            //client.DefaultRequestHeaders.Add("Accept", "application/xml");
        }

        public static async Task<string> GetRequestXml(string path)
        {
            try 
            {
                using var response = await client.GetAsync(path);

                return await response.Content.ReadAsStringAsync();
            } 
            catch (Exception e) 
            {
                Console.WriteLine(e.Message);

                return null;
            }
        }
    }
}
