using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace LobitaBot
{
    public class DbSeriesIndex : DbIndex, ITagIndex
    {
        public DbSeriesIndex(string dbName) : base(dbName) { }

        public new PostData LookupRandomPost(string searchTerm)
        {
            searchTerm = TagParser.EscapeApostrophe(searchTerm);

            string postQuery =
                $"SELECT t.id, t.name, l.url, s.name " +
                $"FROM links AS l, tags AS t, series_tags AS st, series AS s " +
                $"WHERE l.tag_id = t.id AND t.id = st.tag_id AND s.id = st.series_id AND s.name = '{searchTerm}' " +
                $"ORDER BY RAND() " +
                $"LIMIT 1";

            return base.LookupRandomPost(postQuery);
        }

        public string LookupSingleTag(int id)
        {
            string tagQuery = $"SELECT name from series WHERE id = '{id}'";

            return LookupSingleTag(tagQuery);
        }

        public List<TagData> LookupTagData(List<string> tags)
        {
            string dataQuery =
                $"SELECT s.name, s.id, COUNT(l.id) " +
                $"FROM tags AS t, series_tags AS st, series AS s, links AS l " +
                $"WHERE t.id = l.tag_id AND t.id = st.tag_id AND st.series_id = s.id AND s.name = @name";

            return LookupTagData(dataQuery, tags);
        }

        public new List<string> LookupTags(string searchTerm)
        {
            searchTerm = TagParser.EscapeApostrophe(searchTerm);

            string tagQuery = $"SELECT name from series WHERE name LIKE '%{searchTerm}%'";

            return base.LookupTags(tagQuery);
        }

        public new bool TagExists(string searchTerm)
        {
            searchTerm = TagParser.EscapeApostrophe(searchTerm);

            string tagQuery = $"SELECT name from series WHERE name = '{searchTerm}'";

            return base.TagExists(tagQuery);
        }

        public List<string> CharactersInSeries(string seriesName)
        {
            MySqlCommand cmd;
            MySqlDataReader rdr;

            string characterQuery = 
                $"SELECT t.name " +
                $"FROM tags AS t, series_tags AS st, series AS s " +
                $"WHERE t.id = st.tag_id AND st.series_id = s.id AND s.name = '{seriesName}'";

            List<string> characters = new List<string>();

            try
            {
                Conn.Open();

                cmd = new MySqlCommand(characterQuery, Conn);
                rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    characters.Add((string)rdr[0]);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
            }

            Conn.Close();

            return characters;
        }
    }
}
