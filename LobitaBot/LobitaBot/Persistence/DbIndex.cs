using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LobitaBot
{
    public abstract class DbIndex
    {
        protected CacheService _cacheService;
        protected int limit;
        protected const int TimeOut = 300;
        string _dbName;
        protected string connectionString;

        protected DbIndex(string dbName, int batchLimit, CacheService cacheService)
        {
            _dbName = dbName;
            limit = batchLimit;
            _cacheService = cacheService;
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

        protected ICollection<int> GetLinkIdsForTag(string tagName)
        {
            string linkIdQuery = 
                $"SELECT l.id " +
                $"FROM tags AS t, tag_links AS tl, links AS l " +
                $"WHERE t.id = tl.tag_id AND tl.link_id = l.id AND t.name = '{tagName}'";
            MySqlCommand cmd;
            MySqlDataReader rdr;
            HashSet<int> linkIds = new HashSet<int>();

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

        protected ICollection<PostData> GetAllPostsForQuery(string postQuery, AdditionalPostData additionalData = null)
        {
            MySqlCommand cmd;
            MySqlDataReader rdr;
            PostData pd;
            int i = 0;
            HashSet<PostData> postSet = new HashSet<PostData>();

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
                            pd = new PostData((int)rdr[0], (string)rdr[1], (string)rdr[2], (string)rdr[3], i++, (int)rdr[4]);

                            if (additionalData != null)
                            {
                                pd.AdditionalData = additionalData;
                            }

                            postSet.Add(pd);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
                }
            }

            return postSet;
        }

        protected void PopulateCacheParallel(string postQuery, AdditionalPostData additionalData = null)
        {
            _cacheService.CTS.Cancel();

            try
            {
                _cacheService.CacheTask.Wait();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
            }

            _cacheService.Clear();

            _cacheService.CTS = new CancellationTokenSource();

            int i = 0;
            string postQueryLimit = postQuery + $" LIMIT {limit}";

            try
            {
                foreach (PostData pd in GetAllPostsForQuery(postQueryLimit, additionalData))
                {
                    _cacheService.Add(pd);
                }

                _cacheService.CacheTask = Task.Factory.StartNew(() => PopulateCacheParallel(postQueryLimit, i, _cacheService.CTS.Token, additionalData), TaskCreationOptions.LongRunning);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
            }
        }

        private Task PopulateCacheParallel(string queryString, int index, CancellationToken token, AdditionalPostData additionalData)
        {
            long offset = 0;
            string postQueryOffset;
            bool endReached = false;
            MySqlCommand cmd;
            MySqlDataReader rdr;
            PostData pd;

            while (!endReached && !token.IsCancellationRequested)
            {
                offset += limit;
                postQueryOffset = queryString + $" OFFSET {offset}";

                using (MySqlConnection Conn = Connect())
                {
                    cmd = new MySqlCommand(postQueryOffset, Conn);
                    cmd.CommandTimeout = TimeOut;

                    using (rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                pd = new PostData((int)rdr[0], (string)rdr[1], (string)rdr[2], (string)rdr[3], index++, (int)rdr[4]);

                                if (additionalData != null)
                                {
                                    pd.AdditionalData = additionalData;
                                }

                                _cacheService.Add(pd);
                            }
                        }
                        else
                        {
                            endReached = true;
                        }
                    }
                }
            }

            return Task.CompletedTask;
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
