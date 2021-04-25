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
                $"SELECT t.id, t.name, l.url, s.name, l.id " +
                $"FROM links AS l, tags AS t, tag_links AS tl, series_tags AS st, series AS s " +
                $"WHERE l.id = tl.link_id AND t.id = tl.tag_id AND t.id = st.tag_id AND s.id = st.series_id AND s.name = '{searchTerm}'";

            PopulateCacheAsync(postQuery);

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

        public string LookupTagById(int id)
        {
            string tagQuery = $"SELECT name from series WHERE id = '{id}'";

            return LookupTagById(tagQuery);
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
                $"SELECT name, id, post_count FROM series WHERE name IN ({sb})";

            return LookupTagData(tags, dataQuery);
        }

        public new List<string> LookupTags(string searchTerm)
        {
            searchTerm = TagParser.EscapeApostrophe(searchTerm);

            string tagQuery = $"SELECT name from series WHERE name LIKE '{searchTerm}'";

            return base.LookupTags(tagQuery);
        }

        public new bool HasExactMatch(string searchTerm, out string matched)
        {
            searchTerm = TagParser.EscapeApostrophe(searchTerm);

            string tagQuery = $"SELECT name from series WHERE name LIKE '{searchTerm}'";

            return base.HasExactMatch(tagQuery, out matched);
        }

        public List<string> CharactersInSeries(string seriesName)
        {
            seriesName = TagParser.EscapeApostrophe(seriesName);

            MySqlCommand cmd;
            MySqlDataReader rdr;

            string characterQuery = 
                $"SELECT t.name " +
                $"FROM tags AS t, series_tags AS st, series AS s " +
                $"WHERE t.id = st.tag_id AND st.series_id = s.id AND s.name LIKE '{seriesName}'";

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
