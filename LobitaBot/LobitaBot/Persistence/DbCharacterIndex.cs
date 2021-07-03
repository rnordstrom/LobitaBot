using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LobitaBot
{
    public class DbCharacterIndex : DbIndex, ITagIndex
    {
        private string postQuery =
                $"SELECT t.id, t.name, l.url, s.name, l.id " +
                $"FROM links AS l, tag_links AS tl, tags AS t, series_tags AS st, series AS s " +
                $"WHERE l.id = tl.link_id AND t.id = tl.tag_id AND t.id = st.tag_id AND s.id = st.series_id AND t.name = ";

        public DbCharacterIndex(string dbName, int batchLimit, CacheService cacheService) : base(dbName, batchLimit, cacheService) { }

        public PostData LookupRandomPost(string searchTerm)
        {
            if (_cacheService.CharacterAloneInCache(searchTerm))
            {
                return _cacheService.CacheRandom();
            }

            searchTerm = TagParser.EscapeApostrophe(searchTerm);

            PopulateCacheParallel(postQuery + $"'{searchTerm}'");

            return _cacheService.CacheRandom();
        }

        public PostData LookupNextPost(string searchTerm, int index)
        {
            if (_cacheService.CharacterAloneInCache(searchTerm))
            {
                return _cacheService.CacheNext(index);
            }

            searchTerm = TagParser.EscapeApostrophe(searchTerm);

            PopulateCacheParallel(postQuery + $"'{searchTerm}'");

            return _cacheService.CacheNext(index);
        }

        public PostData LookupPreviousPost(string searchTerm, int index)
        {
            if (_cacheService.CharacterAloneInCache(searchTerm))
            {
                return _cacheService.CachePrevious(index);
            }

            searchTerm = TagParser.EscapeApostrophe(searchTerm);

            PopulateCacheParallel(postQuery + $"'{searchTerm}'");

            return _cacheService.CachePrevious(index);
        }

        public PostData LookupRandomCollab(string[] searchTerms)
        {
            if (_cacheService.CollabInCache(searchTerms))
            {
                return _cacheService.CacheRandom();
            }

            PopulateCacheWithAdditionalData(searchTerms);

            PostData postData = _cacheService.CacheRandom();

            return postData;
        }

        public PostData LookupPreviousCollab(string[] searchTerms, int index)
        {
            if (_cacheService.CollabInCache(searchTerms))
            {
                return _cacheService.CachePrevious(index);
            }

            PopulateCacheWithAdditionalData(searchTerms);

            PostData postData = _cacheService.CachePrevious(index);

            return postData;
        }

        public PostData LookupNextCollab(string[] searchTerms, int index)
        {
            if (_cacheService.CollabInCache(searchTerms))
            {
                return _cacheService.CacheNext(index);
            }

            PopulateCacheWithAdditionalData(searchTerms);

            PostData postData = _cacheService.CacheNext(index);

            return postData;
        }

        public string LookupTagById(int id)
        {
            string tagQuery = $"SELECT name from tags WHERE id = '{id}'";

            return LookupTagById(tagQuery);
        }

        public new int LookupTagIdByName(string tagName)
        {
            tagName = TagParser.EscapeApostrophe(tagName);

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

            return LookupTagData(dataQuery);
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
            string randQuery = $"SELECT id FROM tags ORDER BY RAND() LIMIT 1";
            MySqlCommand cmd;
            MySqlDataReader rdr;
            int id = 0;
            string tag = "";

            using (MySqlConnection Conn = Connect())
            {
                try
                {
                    cmd = new MySqlCommand(randQuery, Conn);

                    using (rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            id = (int)rdr[0];
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
                }
            }

            if (id > 0)
            {
                tag = LookupTagById(id);
            }

            return tag;
        }

        public string SeriesWithCharacter(string charName)
        {
            MySqlCommand cmd;
            MySqlDataReader rdr;

            charName = TagParser.EscapeApostrophe(charName);

            string seriesQuery =
                $"SELECT s.name " +
                $"FROM tags AS t, series_tags AS st, series AS s " +
                $"WHERE t.id = st.tag_id AND st.series_id = s.id AND t.name LIKE '{charName}'";

            string series = "";

            using (MySqlConnection Conn = Connect())
            {
                try
                {
                    cmd = new MySqlCommand(seriesQuery, Conn);

                    using (rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            series = (string)rdr[0];
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
                }
            }

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

            using (MySqlConnection Conn = Connect())
            {
                try
                {
                    cmd = new MySqlCommand(postQuery, Conn);

                    using (rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            characters.Add((string)rdr[0]);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
                }
            }

            return characters;
        }

        public List<string> CollabsWithCharacters(string[] searchTerms)
        {
            MySqlCommand cmd;
            MySqlDataReader rdr;
            ICollection<PostData> collabPosts = GetCollabPosts(searchTerms);
            List<int> linkIds = collabPosts.Select(e => e.LinkId).ToList();
            string linkIdsString = ToCommaSeparatedString(linkIds);

            string suggestionsQuery =
                $"SELECT DISTINCT t.name " +
                $"FROM tags AS t, tag_links AS tl, links AS l " +
                $"WHERE t.id = tl.tag_id AND l.id = tl.link_id AND l.id IN ({linkIdsString})";

            List<string> characters = new List<string>();

            using (MySqlConnection Conn = Connect())
            {
                try
                {
                    cmd = new MySqlCommand(suggestionsQuery, Conn);
                    cmd.CommandTimeout = TimeOut;

                    using (rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            characters.Add((string)rdr[0]);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
                }
            }

            foreach (string s in searchTerms)
            {
                if (characters.Contains(s))
                {
                    characters.Remove(s);
                }
            }

            return characters;
        }

        private void PopulateCacheWithAdditionalData(string[] searchTerms)
        {
            ICollection<PostData> postCollection = GetCollabPosts(searchTerms);
            AdditionalPostData apd = BuildAdditionalPostData(searchTerms);

            foreach (PostData pd in postCollection)
            {
                pd.AdditionalData = apd;
            }

            _cacheService.SetCache(postCollection);
        }

        private ICollection<PostData> GetCollabPosts(string[] searchTerms)
        {
            IEnumerable<int> linkIds = GetLinkIdsForTag(TagParser.EscapeApostrophe(searchTerms[0]));

            for (int i = 1; i < searchTerms.Length; i++)
            {
                linkIds = linkIds.Intersect(GetLinkIdsForTag(TagParser.EscapeApostrophe(searchTerms[i])));
            }

            string linkIdString = ToCommaSeparatedString(linkIds.ToList());

            if (linkIdString != string.Empty)
            {
                return IndexCollection(GetAllPostsForQuery(postQuery + $"'{TagParser.EscapeApostrophe(searchTerms[0])}' " + $"AND l.id IN ({linkIdString})"));
            }
            else
            {
                return new List<PostData>();
            }
        }

        private AdditionalPostData BuildAdditionalPostData(string[] searchTerms)
        {
            List<string> additionalTagNames = new List<string>();
            List<int> additionalTagIds = new List<int>();
            List<string> additionalSeriesNames = new List<string>();
            string seriesName;

            for (int i = 1; i < searchTerms.Length; i++)
            {
                additionalTagNames.Add(searchTerms[i]);
                additionalTagIds.Add(LookupTagIdByName(searchTerms[i]));

                seriesName = SeriesWithCharacter(searchTerms[i]);

                if (!additionalSeriesNames.Contains(seriesName))
                {
                    additionalSeriesNames.Add(seriesName);
                }
            }

            return new AdditionalPostData(additionalTagIds, additionalTagNames, additionalSeriesNames);
        }

        private ICollection<PostData> IndexCollection(ICollection<PostData> postCollection)
        {
            int i = 0;

            foreach (PostData pd in postCollection)
            {
                pd.PostIndex = i++;
            }

            return postCollection;
        }

        private string ToCommaSeparatedString(ICollection<int> numberList)
        {
            if (numberList.Count > 0)
            {
                StringBuilder numberString = new StringBuilder();

                foreach (int n in numberList)
                {
                    numberString.Append($"{n},");
                }

                return numberString.Remove(numberString.Length - 1, 1).ToString();
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
