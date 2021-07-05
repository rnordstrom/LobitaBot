using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LobitaBot
{
    public class DbCharacterIndex : DbIndex, ITagIndex
    {
        private string _postQuery =
                $"SELECT t.id, t.name, l.url, s.name, l.id, t.post_count " +
                $"FROM links AS l, tag_links AS tl, tags AS t, series_tags AS st, series AS s " +
                $"WHERE l.id = tl.link_id AND t.id = tl.tag_id AND t.id = st.tag_id AND s.id = st.series_id AND t.name = '&' AND l.id = %";

        public DbCharacterIndex(string dbName) : base(dbName) { }

        public PostData LookupRandomPost(string searchTerm, MySqlConnection conn)
        {
            searchTerm = TagParser.EscapeApostrophe(searchTerm);
            string linkIdQuery =
                $"SELECT tl.link_id " +
                $"FROM tags AS t, tag_links AS tl " +
                $"WHERE t.id = tl.tag_id AND t.name = '{searchTerm}'";
            int randomId = GetRandomLinkIdForQuery(linkIdQuery, conn);

            return GetPostForQuery(BuildPostQuery(_postQuery, searchTerm, randomId), conn);
        }

        public PostData LookupNextPost(string searchTerm, int postId, MySqlConnection conn)
        {
            searchTerm = TagParser.EscapeApostrophe(searchTerm);
            int nextLinkId = GetNextLinkId(searchTerm, postId, conn);
            PostData pd = GetPostForQuery(BuildPostQuery(_postQuery, searchTerm, nextLinkId), conn);

            pd.PostIndex = GetIndexForPostId(searchTerm, nextLinkId, conn);

            return pd;
        }

        public PostData LookupPreviousPost(string searchTerm, int postId, MySqlConnection conn)
        {
            searchTerm = TagParser.EscapeApostrophe(searchTerm);
            int previousLinkId = GetPreviousLinkId(searchTerm, postId, conn);
            PostData pd = GetPostForQuery(BuildPostQuery(_postQuery, searchTerm, previousLinkId), conn);

            pd.PostIndex = GetIndexForPostId(searchTerm, previousLinkId, conn);

            return pd;
        }

        public PostData LookupRandomCollab(string[] searchTerms, MySqlConnection conn)
        {
            PostData postData = GetRandomCollabPost(searchTerms, conn);

            return postData;
        }

        public PostData LookupPreviousCollab(string[] searchTerms, int postId, MySqlConnection conn)
        {
            GetRandomCollabPost(searchTerms, conn);

            PostData postData = PreviousCollabPost(searchTerms, postId, conn);

            return postData;
        }

        public PostData LookupNextCollab(string[] searchTerms, int postId, MySqlConnection conn)
        {
            GetRandomCollabPost(searchTerms, conn);

            PostData postData = NextCollabPost(searchTerms, postId, conn);

            return postData;
        }

        public string LookupTagById(int id, MySqlConnection conn)
        {
            string tagQuery = $"SELECT name from tags WHERE id = '{id}'";

            return LookupTagById(tagQuery, conn);
        }

        public new int LookupTagIdByName(string tagName, MySqlConnection conn)
        {
            tagName = TagParser.EscapeApostrophe(tagName);

            string tagQuery = $"SELECT id from tags WHERE name = '{tagName}'";

            return base.LookupTagIdByName(tagQuery, conn);
        }

        public List<TagData> LookupTagData(List<string> tags, MySqlConnection conn)
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

            return LookupTagData(dataQuery, conn);
        }

        public new List<string> LookupTags(string searchTerm, MySqlConnection conn)
        {
            searchTerm = TagParser.EscapeApostrophe(searchTerm);

            string tagQuery = $"SELECT name from tags WHERE name LIKE '{searchTerm}'";

            return base.LookupTags(tagQuery, conn);
        }

        public new bool HasExactMatch(string searchTerm, MySqlConnection conn, out string matched)
        {
            searchTerm = TagParser.EscapeApostrophe(searchTerm);

            string tagQuery = $"SELECT name from tags WHERE name LIKE '{searchTerm}'";

            return base.HasExactMatch(tagQuery, conn, out matched);
        }

        public string LookupRandomTag(MySqlConnection conn)
        {
            string randQuery = $"SELECT id FROM tags ORDER BY RAND() LIMIT 1";
            MySqlCommand cmd;
            MySqlDataReader rdr;
            int id = 0;
            string tag = "";

            try
            {
                cmd = new MySqlCommand(randQuery, conn);

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

            if (id > 0)
            {
                tag = LookupTagById(id, conn);
            }

            return tag;
        }

        public string SeriesWithCharacter(string charName, MySqlConnection conn)
        {
            MySqlCommand cmd;
            MySqlDataReader rdr;

            charName = TagParser.EscapeApostrophe(charName);

            string seriesQuery =
                $"SELECT s.name " +
                $"FROM tags AS t, series_tags AS st, series AS s " +
                $"WHERE t.id = st.tag_id AND st.series_id = s.id AND t.name LIKE '{charName}'";

            string series = "";

            try
            {
                cmd = new MySqlCommand(seriesQuery, conn);

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

            return series;
        }

        public List<string> CharactersInPost(int postId, MySqlConnection conn)
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
                cmd = new MySqlCommand(postQuery, conn);

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

            return characters;
        }

        public List<string> CollabsWithCharacters(string[] searchTerms, MySqlConnection conn)
        {
            MySqlCommand cmd;
            MySqlDataReader rdr;
            ICollection<int> commonLinkIds = GetCommonLinkIds(searchTerms, conn);
            string linkIdsString = ToCommaSeparatedString(commonLinkIds);

            string suggestionsQuery =
                $"SELECT DISTINCT t.name " +
                $"FROM tags AS t, tag_links AS tl " +
                $"WHERE t.id = tl.tag_id AND tl.link_id IN ({linkIdsString})";

            List<string> characters = new List<string>();

            try
            {
                cmd = new MySqlCommand(suggestionsQuery, conn);
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

            foreach (string s in searchTerms)
            {
                if (characters.Contains(s))
                {
                    characters.Remove(s);
                }
            }

            return characters;
        }

        

        private PostData GetRandomCollabPost(string[] searchTerms, MySqlConnection conn)
        {
            PostData pd = RandomCollabPost(searchTerms, conn);
            AdditionalPostData apd = BuildAdditionalPostData(searchTerms, conn);

            pd.AdditionalData = apd;

            return pd;
        }

        private PostData RandomCollabPost(string[] searchTerms, MySqlConnection conn)
        {
            Random rand = new Random();
            List<int> commonLinkIds = GetCommonLinkIds(searchTerms, conn).ToList();
            commonLinkIds.Sort();
            string searchTermEscaped = TagParser.EscapeApostrophe(searchTerms[0]);
            int randomId = commonLinkIds[rand.Next(0, commonLinkIds.Count())];
            string postQuery = BuildPostQuery(_postQuery, searchTermEscaped, randomId);

            PostData pd = GetPostForQuery(postQuery, conn);

            pd.PostIndex = commonLinkIds.IndexOf(randomId);
            pd.PostCount = commonLinkIds.Count();

            return pd;
        }

        private PostData NextCollabPost(string[] searchTerms, int currentId, MySqlConnection conn)
        {
            List<int> commonLinkIds = GetCommonLinkIds(searchTerms, conn).ToList();
            commonLinkIds.Sort();
            int idIndex = commonLinkIds.IndexOf(currentId);
            int nextIndex = idIndex == commonLinkIds.Count() - 1 ? idIndex : idIndex + 1;
            string postQuery = BuildPostQuery(_postQuery, TagParser.EscapeApostrophe(searchTerms[0]), commonLinkIds[nextIndex]);

            PostData pd = GetPostForQuery(postQuery, conn);
            AdditionalPostData apd = BuildAdditionalPostData(searchTerms, conn);

            pd.PostIndex = nextIndex;
            pd.PostCount = commonLinkIds.Count();
            pd.AdditionalData = apd;

            return pd;
        }

        private PostData PreviousCollabPost(string[] searchTerms, int currentId, MySqlConnection conn)
        {
            List<int> commonLinkIds = GetCommonLinkIds(searchTerms, conn).ToList();
            commonLinkIds.Sort();
            int idIndex = commonLinkIds.IndexOf(currentId);
            int previousIndex = idIndex == 0 ? idIndex : idIndex - 1;
            string postQuery = BuildPostQuery(_postQuery, TagParser.EscapeApostrophe(searchTerms[0]), commonLinkIds[previousIndex]);

            PostData pd = GetPostForQuery(postQuery, conn);
            AdditionalPostData apd = BuildAdditionalPostData(searchTerms, conn);

            pd.PostIndex = previousIndex;
            pd.PostCount = commonLinkIds.Count();
            pd.AdditionalData = apd;

            return pd;
        }

        private ICollection<int> GetCommonLinkIds(string[] searchTerms, MySqlConnection conn)
        {
            IEnumerable<int> linkIds = GetLinkIdsForTag(searchTerms[0], conn);

            for (int i = 1; i < searchTerms.Length; i++)
            {
                linkIds = linkIds.Intersect(GetLinkIdsForTag(searchTerms[i], conn));
            }

            return linkIds.ToList();
        }

        private AdditionalPostData BuildAdditionalPostData(string[] searchTerms, MySqlConnection conn)
        {
            List<string> additionalTagNames = new List<string>();
            List<int> additionalTagIds = new List<int>();
            List<string> additionalSeriesNames = new List<string>();
            string seriesName;

            for (int i = 1; i < searchTerms.Length; i++)
            {
                additionalTagNames.Add(searchTerms[i]);
                additionalTagIds.Add(LookupTagIdByName(searchTerms[i], conn));

                seriesName = SeriesWithCharacter(searchTerms[i], conn);

                if (!additionalSeriesNames.Contains(seriesName))
                {
                    additionalSeriesNames.Add(seriesName);
                }
            }

            return new AdditionalPostData(additionalTagIds, additionalTagNames, additionalSeriesNames);
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
