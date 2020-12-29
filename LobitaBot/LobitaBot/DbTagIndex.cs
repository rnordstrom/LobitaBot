using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace LobitaBot
{
    public class DbTagIndex : ITagIndex
    {
        private string connStr = $"server=localhost;user=root;database=tagdb;port=3306;password={Environment.GetEnvironmentVariable("PWD")}";
        private MySqlConnection conn;

        public DbTagIndex()
        {
            conn = new MySqlConnection(connStr);
        }

        public string LookupRandom(string searchTerm)
        {
            if (searchTerm.Contains("'"))
            {
                searchTerm = searchTerm.Insert(searchTerm.IndexOf("'"), "'");
            }

            string minQuery = 
                $"SELECT MIN(l.id) " +
                $"FROM links AS l, tags AS t " +
                $"WHERE l.tag_id = t.id AND t.name = '{searchTerm}'";
            string maxQuery =
                $"SELECT MAX(l.id) " +
                $"FROM links AS l, tags AS t " +
                $"WHERE l.tag_id = t.id AND t.name = '{searchTerm}'";
            MySqlCommand cmd;
            MySqlDataReader rdr;
            Random rand = new Random();
            int minId = 0;
            int maxId = 0;
            int chosen;
            string linkQuery;
            string url = "";

            try
            {
                conn.Open();

                cmd = new MySqlCommand(minQuery, conn);
                rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    minId = (int)rdr[0];
                }

                rdr.Close();

                cmd = new MySqlCommand(maxQuery, conn);
                rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    maxId = (int)rdr[0];
                }

                rdr.Close();

                chosen = rand.Next(minId, maxId + 1);

                linkQuery =
                $"SELECT l.url " +
                $"FROM links AS l, tags AS t " +
                $"WHERE l.tag_id = t.id AND l.id = '{chosen}'";

                cmd = new MySqlCommand(linkQuery, conn);
                rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    url = (string)rdr[0];
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
            }

            conn.Close();

            return url;
        }

        public string LookupSingleTag(string searchTerm)
        {
            if (searchTerm.Contains("'"))
            {
                searchTerm = searchTerm.Insert(searchTerm.IndexOf("'"), "'");
            }

            string tagQuery = $"SELECT name from tags WHERE name = '{searchTerm}'";
            string tag = "";
            MySqlCommand cmd;
            MySqlDataReader rdr;

            try
            {
                conn.Open();

                cmd = new MySqlCommand(tagQuery, conn);
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

            conn.Close();

            return tag;
        }

        public List<string> LookupTags(string searchTerm)
        {
            if (searchTerm.Contains("'"))
            {
                searchTerm = searchTerm.Insert(searchTerm.IndexOf("'"), "'");
            }

            string tagQuery = $"SELECT name from tags WHERE name LIKE '%{searchTerm}%'";
            List<string> tags = new List<string>();
            MySqlCommand cmd;
            MySqlDataReader rdr;

            try
            {
                conn.Open();

                cmd = new MySqlCommand(tagQuery, conn);
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

            conn.Close();

            return tags;
        }
    }
}
