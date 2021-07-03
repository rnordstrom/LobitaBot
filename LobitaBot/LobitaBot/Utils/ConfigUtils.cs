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

            doc.Load(BuildPath(configDirectory));

            return doc
                .SelectSingleNode("items")
                .SelectSingleNode("CurrentDatabase")
                .InnerText;
        }

        public static int GetBatchReadLimit(string configDirectory)
        {
            XmlDocument doc = new XmlDocument();

            doc.Load(BuildPath(configDirectory));

            return int.Parse(doc
                .SelectSingleNode("items")
                .SelectSingleNode("BatchReadLimit")
                .InnerText);
        }

        public static int GetPostLowerLimit(string configDirectory)
        {
            XmlDocument doc = new XmlDocument();

            doc.Load(BuildPath(configDirectory));

            return int.Parse(doc
                .SelectSingleNode("items")
                .SelectSingleNode("PostLowerLimit")
                .InnerText);
        }

        private static string BuildPath(string configDirectory)
        {
            return Path.Join(Environment.GetEnvironmentVariable("CONFIG_LOCATION"), configDirectory, Constants.ConfigFile);
        }
    }
}
