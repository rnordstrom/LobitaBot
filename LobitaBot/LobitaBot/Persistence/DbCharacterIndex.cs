using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace LobitaBot
{
    public class DbCharacterIndex : DbIndex, ITagIndex
    {
        public DbCharacterIndex(string dbName, CacheService cacheService) : base(dbName, cacheService) { }

        public PostData LookupRandomPost(string searchTerm)
        {
            if (_cacheService.CharacterAloneInCache(searchTerm))
            {
                return _cacheService.CacheRandom();
            }

            searchTerm = TagParser.EscapeApostrophe(searchTerm);
            string postQuery =
                $"SELECT t.id, t.name, l.url, s.name, l.id " +
                $"FROM links AS l, tag_links AS tl, tags AS t, series_tags AS st, series AS s " +
                $"WHERE l.id = tl.link_id AND t.id = tl.tag_id AND t.id = st.tag_id AND s.id = st.series_id AND t.name = '{searchTerm}'";

            PopulateCacheAsync(postQuery);

            return _cacheService.CacheRandom();
        }

        public PostData LookupNextPost(string searchTerm, int index)
        {
            if (_cacheService.CharacterAloneInCache(searchTerm))
            {
                return _cacheService.CacheNext(index);
            }

            searchTerm = TagParser.EscapeApostrophe(searchTerm);
            string postQuery =
                $"SELECT t.id, t.name, l.url, s.name, l.id " +
                $"FROM links AS l, tag_links AS tl, tags AS t, series_tags AS st, series AS s " +
                $"WHERE l.id = tl.link_id AND t.id = tl.tag_id AND t.id = st.tag_id AND s.id = st.series_id AND t.name = '{searchTerm}'";

            PopulateCacheAsync(postQuery);

            return _cacheService.CacheNext(index);
        }

        public PostData LookupPreviousPost(string searchTerm, int index)
        {
            if (_cacheService.CharacterAloneInCache(searchTerm))
            {
                return _cacheService.CachePrevious(index);
            }

            searchTerm = TagParser.EscapeApostrophe(searchTerm);
            string postQuery =
                $"SELECT t.id, t.name, l.url, s.name, l.id " +
                $"FROM links AS l, tag_links AS tl, tags AS t, series_tags AS st, series AS s " +
                $"WHERE l.id = tl.link_id AND t.id = tl.tag_id AND t.id = st.tag_id AND s.id = st.series_id AND t.name = '{searchTerm}'";

            PopulateCacheAsync(postQuery);

            return _cacheService.CachePrevious(index);
        }

        public PostData LookupCollab(string[] searchTerms)
        {
            for (int i = 0; i < searchTerms.Length; i++)
            {
                searchTerms[i] = TagParser.EscapeApostrophe(searchTerms[i]);
            }

            string baseQuery = 
                $"SELECT t0.tag_id, t0.tag_name, t0.url, t0.series_name, t0.id " +
                $"FROM " +
                $"(SELECT t.id AS tag_id, t.name AS tag_name, url, s.name AS series_name, l.id AS id " +
                $"FROM links AS l, tag_links AS tl, tags AS t, series_tags AS st, series AS s " +
                $"WHERE t.name = '{searchTerms[0]}' " +
                $"AND l.id = tl.link_id AND t.id = tl.tag_id AND t.id = st.tag_id AND s.id = st.series_id) t0";

            for (int i = 1; i < searchTerms.Length; i++)
            {
                baseQuery += " INNER JOIN ";
                baseQuery += 
                    $"(SELECT l.id AS id " +
                    $"FROM tags AS t, tag_links AS tl, links AS l " +
                    $"WHERE t.name = '{searchTerms[i]}' AND t.id = tl.tag_id AND l.id = tl.link_id) t{i}";
                baseQuery += $" ON(t0.id = t{i}.id)";
            }

            PopulateCacheAsync(baseQuery);

            return _cacheService.CacheRandom();
        }

        public string LookupTagById(int id)
        {
            string tagQuery = $"SELECT name from tags WHERE id = '{id}'";

            return LookupTagById(tagQuery);
        }

        public new int LookupTagIdByName(string tagName)
        {
            string tagQuery = $"SELECT id from tags WHERE name = '{tagName}'";

            return base.LookupTagIdByName(tagQuery);
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

            dataQuery = $"SELECT name, id, post_count FROM tags WHERE name IN ({sb})";

            return LookupTagData(tags, dataQuery);
        }

        public new List<string> LookupTags(string searchTerm)
        {
            searchTerm = TagParser.EscapeApostrophe(searchTerm);

            string tagQuery = $"SELECT name from tags WHERE name LIKE '{searchTerm}'";

            return base.LookupTags(tagQuery);
        }

        public new bool HasExactMatch(string searchTerm, out string matched)
        {
            searchTerm = TagParser.EscapeApostrophe(searchTerm);

            string tagQuery = $"SELECT name from tags WHERE name LIKE '{searchTerm}'";

            return base.HasExactMatch(tagQuery, out matched);
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
                tag = LookupTagById(chosen);
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
                $"WHERE t.id = st.tag_id AND st.series_id = s.id AND t.name LIKE '{charName}'";

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

        public List<string> CharactersInPost(int postId)
        {
            MySqlCommand cmd;
            MySqlDataReader rdr;

            string postQuery =
                $"SELECT t.name " +
                $"FROM tags AS t, tag_links AS tl, links AS l " +
                $"WHERE t.id = tl.tag_id AND l.id = tl.link_id AND l.id = {postId}";

            List<string> characters = new List<string>();

            try
            {
                Conn.Open();

                cmd = new MySqlCommand(postQuery, Conn);
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
