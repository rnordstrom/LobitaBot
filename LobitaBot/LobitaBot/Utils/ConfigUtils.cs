using System.Xml;

namespace LobitaBot
{
    public static class ConfigUtils
    {
        public static string GetCurrentDatabase()
        {
            XmlDocument doc = new XmlDocument();

            doc.Load("indexconfig.xml");

            return doc
                .SelectSingleNode("items")
                .SelectSingleNode("CurrentDatabase")
                .InnerText;
        }
    }
}
