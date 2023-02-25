using System;
using System.IO;
using System.Net;
using System.Text;

namespace LobitaBot.Services
{
    public static class HttpXmlService
    {
        public static string GetRequestXml(string url)
        {
            var httpRequest = (HttpWebRequest)WebRequest.Create(url.Trim());
            var username = Environment.GetEnvironmentVariable("API_USER");
            var key = Environment.GetEnvironmentVariable("API_KEY");
            var encoding = Encoding.GetEncoding("iso-8859-1").GetBytes($"{username}:{key}");
            var credentials = Convert.ToBase64String(encoding);

            httpRequest.Headers["User-Agent"] = username;
            httpRequest.Headers["Authorization"] = $"Basic {credentials}";

            try 
            {
                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    return streamReader.ReadToEnd();
                }
            } catch (Exception e) {
                Console.WriteLine(e.Message);

                return null;
            }
        }
    }
}
