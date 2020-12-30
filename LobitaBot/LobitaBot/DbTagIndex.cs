using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

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

        public string LookupRandomLink(string searchTerm)
        {
            searchTerm = EscapeApostrophe(searchTerm);

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
            string link = "";

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
                    link = (string)rdr[0];
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
            }

            conn.Close();

            return link;
        }

        public string LookupRandomTag()
        {
            string minQuery = $"SELECT MIN(id) FROM tags";
            string maxQuery = $"SELECT MAX(id) FROM tags";
            MySqlCommand cmd;
            MySqlDataReader rdr;
            Random rand = new Random();
            int minId = 0;
            int maxId = 0;
            int chosen = 0;
            string tag = "";

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
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
            }

            conn.Close();

            if (chosen > 0)
            {
                tag = LookupSingleTag(chosen);
            }

            return tag;
        }

        public string LookupSingleTag(int id)
        {
            string tagQuery = $"SELECT name from tags WHERE id = '{id}'";
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

        public bool TagExists(string searchTerm)
        {
            searchTerm = EscapeApostrophe(searchTerm);

            string tagQuery = $"SELECT name from tags WHERE name = '{searchTerm}'";
            string tag = "";
            bool exists = false;
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

                if (!string.IsNullOrEmpty(tag))
                {
                    exists = true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
            }

            conn.Close();

            return exists;
        }

        public List<string> LookupTags(string searchTerm)
        {
            searchTerm = EscapeApostrophe(searchTerm);

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

        public List<TagData> LookupTagData(List<string> searchTerms)
        {
            string escaped;
            string dataQuery;
            string last = searchTerms[searchTerms.Count - 1];
            StringBuilder sb = new StringBuilder();
            List<TagData> tagData = new List<TagData>();
            MySqlCommand cmd;
            MySqlDataReader rdr;

            foreach (string s in searchTerms)
            {
                escaped = EscapeApostrophe(s);

                if (s == last)
                {
                    sb.Append($"t.name = '{escaped}'");
                }
                else
                {
                    sb.Append($"t.name = '{escaped}' OR ");
                }
            }

            dataQuery =
                $"SELECT t.name, t.id, COUNT(l.id) " +
                $"FROM tags AS t, links AS l " +
                $"WHERE t.id = l.tag_id AND({sb}) " +
                $"GROUP BY t.name";

            try
            {
                conn.Open();

                cmd = new MySqlCommand(dataQuery, conn);
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

            conn.Close();

            return tagData;
        }

        private string EscapeApostrophe(string tag)
        {
            if (tag.Contains("'"))
            {
                return tag.Insert(tag.IndexOf("'"), "'");
            }

            return tag;
        }
    }
}
