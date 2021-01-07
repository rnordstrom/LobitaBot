using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace LobitaBot
{
    public class DbCharacterIndex : DbIndex, ITagIndex
    {
        public DbCharacterIndex(string dbName) : base(dbName) { }

        public new PostData LookupRandomPost(string searchTerm)
        {
            searchTerm = TagParser.EscapeApostrophe(searchTerm);

            string postQuery =
                $"SELECT t.id, t.name, l.url, s.name " +
                $"FROM links AS l, tags AS t, series_tags AS st, series AS s " +
                $"WHERE l.tag_id = t.id AND t.id = st.tag_id AND s.id = st.series_id AND t.name = '{searchTerm}' " +
                $"ORDER BY RAND() " +
                $"LIMIT 1";

            return base.LookupRandomPost(postQuery);
        }


        public string LookupSingleTag(int id)
        {
            string tagQuery = $"SELECT name from tags WHERE id = '{id}'";

            return LookupSingleTag(tagQuery);
        }

        public List<TagData> LookupTagData(List<string> tags)
        {
            string escaped;
            string dataQuery;
            string last = tags[tags.Count - 1];
            StringBuilder sb = new StringBuilder();

            foreach (string s in tags)
            {
                escaped = TagParser.EscapeApostrophe(s);

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

            return LookupTagData(tags, dataQuery);
        }

        public new List<string> LookupTags(string searchTerm)
        {
            searchTerm = TagParser.EscapeApostrophe(searchTerm);

            string tagQuery = $"SELECT name from tags WHERE name LIKE '%{searchTerm}%'";

            return base.LookupTags(tagQuery);
        }

        public new bool TagExists(string searchTerm)
        {
            searchTerm = TagParser.EscapeApostrophe(searchTerm);

            string tagQuery = $"SELECT name from tags WHERE name = '{searchTerm}'";

            return base.TagExists(tagQuery);
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
                Conn.Open();

                cmd = new MySqlCommand(minQuery, Conn);
                rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    minId = (int)rdr[0];
                }

                rdr.Close();

                cmd = new MySqlCommand(maxQuery, Conn);
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

            Conn.Close();

            if (chosen > 0)
            {
                tag = LookupSingleTag(chosen);
            }

            return tag;
        }

        public List<string> SeriesWithCharacter(string charName)
        {
            MySqlCommand cmd;
            MySqlDataReader rdr;

            string seriesQuery =
                $"SELECT s.name " +
                $"FROM tags AS t, series_tags AS st, series AS s " +
                $"WHERE t.id = st.tag_id AND st.series_id = s.id AND t.name = '{charName}'";

            List<string> series = new List<string>();

            try
            {
                Conn.Open();

                cmd = new MySqlCommand(seriesQuery, Conn);
                rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    series.Add((string)rdr[0]);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
            }

            Conn.Close();

            return series;
        }
    }
}
