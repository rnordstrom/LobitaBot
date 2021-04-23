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
            if (_cacheService.CharacterInCache(searchTerm) && _cacheService.CharacterAloneInCache(searchTerm))
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
            if (_cacheService.CharacterInCache(searchTerm) && _cacheService.CharacterAloneInCache(searchTerm))
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
            if (_cacheService.CharacterInCache(searchTerm) && _cacheService.CharacterAloneInCache(searchTerm))
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
                    sb.Append($"'{escaped}'");
                }
                else
                {
                    sb.Append($"'{escaped}',");
                }
            }

            dataQuery = $"SELECT name, id FROM tags WHERE name IN ({sb})";

            return LookupTagData(tags, dataQuery);
        }

        public new List<string> LookupTags(string searchTerm)
        {
            searchTerm = TagParser.EscapeApostrophe(searchTerm);

            string tagQuery = $"SELECT name from tags WHERE name LIKE '{searchTerm}'";

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
