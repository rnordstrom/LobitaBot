using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LobitaBot
{
    public abstract class DbIndex
    {
        protected int limit;
        protected const int TimeOut = 300;
        string _dbName;
        protected string connectionString;

        protected DbIndex(string dbName)
        {
            _dbName = dbName;
        }

        protected MySqlConnection Connect()
        {
            MySqlConnection Conn = new MySqlConnection(
                $"server={Environment.GetEnvironmentVariable("DB_HOST")};" +
                $"user={Environment.GetEnvironmentVariable("DB_USER")};" +
                $"database={_dbName};port=3306;" +
                $"password={Environment.GetEnvironmentVariable("DB_PWD")};" +
                $"Allow User Variables=true;" +
                $"Ignore Prepare=false;");

            Conn.Open();

            return Conn;
        }

        protected string BuildPostQuery(string baseQuery, string tagName, int linkId)
        {
            return baseQuery.Replace("&", tagName).Replace("%", linkId.ToString());
        }

        protected int GetIndexForPostId(string tagName, int postId)
        {
            List<int> linkIds = GetLinkIdsForTag(tagName);

            return linkIds.IndexOf(postId);
        }

        protected int GetRandomLinkIdForQuery(string postQuery)
        {
            MySqlCommand cmd;
            MySqlDataReader rdr;
            List<int> idList = new List<int>();
            Random rand = new Random();
            int index;

            using (MySqlConnection Conn = Connect())
            {
                try
                {
                    cmd = new MySqlCommand(postQuery, Conn);
                    cmd.CommandTimeout = TimeOut;

                    using (rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            idList.Add((int)rdr[0]);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
                }
            }

            index = rand.Next(0, idList.Count);

            return idList.Count > 0 ? idList[index] : 0;
        }

        protected int GetNextLinkId(string tagName, int currentId)
        {
            List<int> linkIds = GetLinkIdsForTag(tagName);
            int idIndex = linkIds.IndexOf(currentId);
            int nextIndex = idIndex == linkIds.Count - 1 ? idIndex : idIndex + 1;

            return linkIds[nextIndex];
        }

        protected int GetPreviousLinkId(string tagName, int currentId)
        {
            List<int> linkIds = GetLinkIdsForTag(tagName);
            int idIndex = linkIds.IndexOf(currentId);
            int previousIndex = idIndex == 0 ? idIndex : idIndex - 1;

            return linkIds[previousIndex];
        }

        protected List<int> GetLinkIdsForTag(string tagName)
        {
            tagName = TagParser.EscapeApostrophe(tagName);
            string linkIdQuery = 
                $"SELECT l.id " +
                $"FROM tags AS t, tag_links AS tl, links AS l " +
                $"WHERE t.id = tl.tag_id AND tl.link_id = l.id AND t.name = '{tagName}'";
            MySqlCommand cmd;
            MySqlDataReader rdr;
            List<int> linkIds = new List<int>();

            using (MySqlConnection Conn = Connect())
            {
                try
                {
                    cmd = new MySqlCommand(linkIdQuery, Conn);
                    cmd.CommandTimeout = TimeOut;

                    using (rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            linkIds.Add((int)rdr[0]);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
                }
            }

            return linkIds;
        }

        protected PostData GetPostForQuery(string postQuery)
        {
            MySqlCommand cmd;
            MySqlDataReader rdr;
            PostData pd = null;

            using (MySqlConnection Conn = Connect())
            {
                try
                {
                    cmd = new MySqlCommand(postQuery, Conn);
                    cmd.CommandTimeout = TimeOut;

                    using (rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            pd = new PostData((int)rdr[0], (string)rdr[1], (string)rdr[2], (string)rdr[3], (int)rdr[4], (int)rdr[5]);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
                }
            }

            if (pd != null)
            {
                pd.PostIndex = GetIndexForPostId(pd.TagName, pd.LinkId);
            }

            return pd;
        }

        protected string LookupTagById(string tagQuery)
        {
            string tag = "";
            MySqlCommand cmd;
            MySqlDataReader rdr;

            using (MySqlConnection Conn = Connect())
            {
                try
                {
                    cmd = new MySqlCommand(tagQuery, Conn);
                    cmd.CommandTimeout = TimeOut;

                    using (rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            tag = (string)rdr[0];
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
                }
            }

            return tag;
        }

        protected int LookupTagIdByName(string tagQuery)
        {
            int id = -1;
            MySqlCommand cmd;
            MySqlDataReader rdr;

            using (MySqlConnection Conn = Connect())
            {
                try
                {
                    cmd = new MySqlCommand(tagQuery, Conn);
                    cmd.CommandTimeout = TimeOut;

                    using (rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            id = (int)rdr[0];
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
                }
            }

            return id;
        }

        protected List<TagData> LookupTagData(string dataQuery)
        {
            List<TagData> tagData = new List<TagData>();
            MySqlCommand cmd;
            MySqlDataReader rdr;

            using (MySqlConnection Conn = Connect())
            {
                try
                {
                    cmd = new MySqlCommand(dataQuery, Conn);
                    cmd.CommandTimeout = TimeOut;

                    using (rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            tagData.Add(new TagData((string)rdr[0], (int)rdr[1], (int)rdr[2]));
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
                }
            }

            return tagData;
        }

        protected List<string> LookupTags(string tagQuery)
        {
            List<string> tags = new List<string>();
            MySqlCommand cmd;
            MySqlDataReader rdr;

            using (MySqlConnection Conn = Connect())
            {
                try
                {
                    cmd = new MySqlCommand(tagQuery, Conn);
                    cmd.CommandTimeout = TimeOut;

                    using (rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            tags.Add((string)rdr[0]);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
                }
            }

            return tags;
        }

        protected bool HasExactMatch(string tagQuery, out string matched)
        {
            string tag = "";
            bool exists = false;
            MySqlCommand cmd;
            MySqlDataReader rdr;

            using (MySqlConnection Conn = Connect())
            {
                try
                {
                    cmd = new MySqlCommand(tagQuery, Conn);
                    cmd.CommandTimeout = TimeOut;
                    int i = 0;

                    using (rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            tag = (string)rdr[0];

                            i++;
                        }
                    }

                    if (!string.IsNullOrEmpty(tag) && i == 1)
                    {
                        exists = true;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
                }
            }

            matched = tag;

            return exists;
        }
    }
}
