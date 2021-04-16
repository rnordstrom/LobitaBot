using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace LobitaBot
{
    public abstract class DbIndex
    {
        protected MySqlConnection Conn { get; }
        protected CacheService _cacheService;

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

        protected void PopulateCache(string postQuery)
        {
            _cacheService.Clear();

            MySqlCommand cmd;
            MySqlDataReader rdr;
            int i = 0;

            try
            {
                Conn.Open();

                cmd = new MySqlCommand(postQuery, Conn);
                rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    _cacheService.Add(new PostData((int)rdr[0], (string)rdr[1], (string)rdr[2], (string)rdr[3], i++, (int)rdr[4]));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
            }

            Conn.Close();
        }

        protected string LookupSingleTag(string tagQuery)
        {
            string tag = "";
            MySqlCommand cmd;
            MySqlDataReader rdr;

            try
            {
                Conn.Open();

                cmd = new MySqlCommand(tagQuery, Conn);
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

        protected List<TagData> LookupTagData(List<string> tags, string dataQuery)
        {
            List<TagData> tagData = new List<TagData>();
            MySqlCommand cmd;
            MySqlDataReader rdr;

            try
            {
                Conn.Open();

                cmd = new MySqlCommand(dataQuery, Conn);

                rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    tagData.Add(new TagData((string)rdr[0], (int)rdr[1], (long)rdr[2]));
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

        protected bool TagExists(string tagQuery)
        {
            string tag = "";
            bool exists = false;
            MySqlCommand cmd;
            MySqlDataReader rdr;

            try
            {
                Conn.Open();

                cmd = new MySqlCommand(tagQuery, Conn);
                rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    tag = (string)rdr[0];
                }

                if (!string.IsNullOrEmpty(tag))
                {
                    exists = true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
            }

            Conn.Close();

            return exists;
        }
    }
}
