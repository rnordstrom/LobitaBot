using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LobitaBot
{
    public abstract class DbIndex
    {
        protected MySqlConnection Conn { get; }
        protected CacheService _cacheService;
        protected const int TimeOut = 300;
        protected const int Limit = 10000;

        protected DbIndex(string dbName, CacheService cacheService)
        {
            Conn = new MySqlConnection(
                $"server={Environment.GetEnvironmentVariable("DB_HOST")};" +
                $"user={Environment.GetEnvironmentVariable("DB_USER")};" +
                $"database={dbName};port=3306;" +
                $"password={Environment.GetEnvironmentVariable("DB_PWD")};" +
                $"Allow User Variables=true;" +
                $"Ignore Prepare=false;");
            _cacheService = cacheService;
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

            MySqlCommand cmd;
            MySqlDataReader rdr;
            PostData pd;
            int i = 0;
            string postQueryLimit = postQuery + $" LIMIT {Limit}";

            try
            {
                Conn.Open();

                cmd = new MySqlCommand(postQueryLimit, Conn);
                cmd.CommandTimeout = TimeOut;
                rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    pd = new PostData((int)rdr[0], (string)rdr[1], (string)rdr[2], (string)rdr[3], i++, (int)rdr[4]);

                    if (additionalData != null)
                    {
                        pd.AdditionalData = additionalData;
                    }

                    _cacheService.Add(pd);
                }

                rdr.Close();

                _cacheService.CacheTask = Task.Factory.StartNew(() => PopulateCacheParallel(postQueryLimit, i, _cacheService.CTS.Token, additionalData), TaskCreationOptions.LongRunning);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);

                Conn.Close();
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
                offset += Limit;
                postQueryOffset = queryString + $" OFFSET {offset}";

                cmd = new MySqlCommand(postQueryOffset, Conn);
                cmd.CommandTimeout = TimeOut;
                rdr = cmd.ExecuteReader();

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

                rdr.Close();
            }

            Conn.Close();

            return Task.CompletedTask;
        }

        protected string LookupTagById(string tagQuery)
        {
            string tag = "";
            MySqlCommand cmd;
            MySqlDataReader rdr;

            try
            {
                Conn.Open();

                cmd = new MySqlCommand(tagQuery, Conn);
                cmd.CommandTimeout = TimeOut;
                rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    tag = (string)rdr[0];
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
            }

            Conn.Close();

            return tag;
        }

        protected int LookupTagIdByName(string tagQuery)
        {
            int id = -1;
            MySqlCommand cmd;
            MySqlDataReader rdr;

            try
            {
                Conn.Open();

                cmd = new MySqlCommand(tagQuery, Conn);
                cmd.CommandTimeout = TimeOut;
                rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    id = (int)rdr[0];
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
            }

            Conn.Close();

            return id;
        }

        protected List<TagData> LookupTagData(string dataQuery)
        {
            List<TagData> tagData = new List<TagData>();
            MySqlCommand cmd;
            MySqlDataReader rdr;

            try
            {
                Conn.Open();

                cmd = new MySqlCommand(dataQuery, Conn);
                cmd.CommandTimeout = TimeOut;
                rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    tagData.Add(new TagData((string)rdr[0], (int)rdr[1], (int)rdr[2]));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
            }

            Conn.Close();

            return tagData;
        }

        protected List<string> LookupTags(string tagQuery)
        {
            List<string> tags = new List<string>();
            MySqlCommand cmd;
            MySqlDataReader rdr;

            try
            {
                Conn.Open();

                cmd = new MySqlCommand(tagQuery, Conn);
                cmd.CommandTimeout = TimeOut;
                rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    tags.Add((string)rdr[0]);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
            }

            Conn.Close();

            return tags;
        }

        protected bool HasExactMatch(string tagQuery, out string matched)
        {
            string tag = "";
            bool exists = false;
            MySqlCommand cmd;
            MySqlDataReader rdr;

            try
            {
                Conn.Open();

                cmd = new MySqlCommand(tagQuery, Conn);
                cmd.CommandTimeout = TimeOut;
                rdr = cmd.ExecuteReader();
                int i = 0;

                while (rdr.Read())
                {
                    tag = (string)rdr[0];

                    i++;
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

            Conn.Close();

            matched = tag;

            return exists;
        }
    }
}
