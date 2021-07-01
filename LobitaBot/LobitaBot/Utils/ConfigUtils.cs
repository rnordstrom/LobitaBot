using System;
using System.IO;
using System.Xml;

namespace LobitaBot
{
    public static class ConfigUtils
    {
        public static string GetCurrentDatabase(string configDirectory)
        {
            XmlDocument doc = new XmlDocument();

            doc.Load(Path.Join(Environment.GetEnvironmentVariable("CONFIG_LOCATION"), configDirectory, Constants.ConfigFile));

            return doc
                .SelectSingleNode("items")
                .SelectSingleNode("CurrentDatabase")
                .InnerText;
        }

        public static int GetBatchQueryLimit(string configDirectory)
        {
            XmlDocument doc = new XmlDocument();

            doc.Load(Path.Join(Environment.GetEnvironmentVariable("CONFIG_LOCATION"), configDirectory, Constants.ConfigFile));

            return int.Parse(doc
                .SelectSingleNode("items")
                .SelectSingleNode("BatchQueryLimit")
                .InnerText);
        }
    }
}
