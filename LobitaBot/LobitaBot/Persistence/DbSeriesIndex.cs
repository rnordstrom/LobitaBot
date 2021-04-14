using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace LobitaBot
{
    public class DbSeriesIndex : DbIndex, ITagIndex
    {
        public DbSeriesIndex(string dbName, CacheService cacheService) : base(dbName, cacheService) { }

        public PostData LookupRandomPost(string searchTerm)
        {
            if (_cacheService.SeriesInCache(searchTerm))
            {
                PostData pd = _cacheService.CacheRandom();

                if (!_cacheService.CharacterAloneInCache(pd.TagName))
                {
                    return pd;
                }
            }

            searchTerm = TagParser.EscapeApostrophe(searchTerm);
            string postQuery =
                $"SELECT t.id, t.name, l.url, s.name " +
                $"FROM links AS l, tags AS t, series_tags AS st, series AS s " +
                $"WHERE l.tag_id = t.id AND t.id = st.tag_id AND s.id = st.series_id AND s.name = '{searchTerm}' " +
                $"ORDER BY RAND() " +
                $"LIMIT 1000";

            PopulateCache(postQuery);

            return _cacheService.CacheRandom();
        }

        public PostData LookupNextPost(string searchTerm, int index)
        {
            throw new NotImplementedException();
        }

        public PostData LookupPreviousPost(string searchTerm, int index)
        {
            throw new NotImplementedException();
        }

        public string LookupSingleTag(int id)
        {
            string tagQuery = $"SELECT name from series WHERE id = '{id}'";

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
                    sb.Append($"'{escaped}'");
                }
                else
                {
                    sb.Append($"'{escaped}',");
                }
            }

            dataQuery =
                $"SELECT s.name, s.id, COUNT(l.id) " +
                $"FROM tags AS t, series_tags AS st, series AS s, links AS l " +
                $"WHERE t.id = l.tag_id AND t.id = st.tag_id AND st.series_id = s.id AND s.name IN ({sb}) " +
                $"GROUP BY s.name";

            return LookupTagData(tags, dataQuery);
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
