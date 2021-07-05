using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace LobitaBot
{
    public class DbSeriesIndex : DbIndex, ITagIndex
    {
        public DbSeriesIndex(string dbName) : base(dbName) { }

        public PostData LookupRandomPost(string searchTerm, MySqlConnection conn)
        {
            searchTerm = TagParser.EscapeApostrophe(searchTerm);
            string postQuery =
                $"SELECT t.id, t.name, l.url, s.name, l.id, s.post_count " +
                $"FROM links AS l, tags AS t, tag_links AS tl, series_tags AS st, series AS s " +
                $"WHERE l.id = tl.link_id AND t.id = tl.tag_id AND t.id = st.tag_id AND s.id = st.series_id AND s.name = '&' AND l.id = %";
            string linkIdQuery =
                $"SELECT tl.link_id " +
                $"FROM tags AS t, series AS s, series_tags AS st, tag_links AS tl " +
                $"WHERE s.id = st.series_id AND st.tag_id = t.id AND t.id = tl.tag_id AND s.name = '{searchTerm}'";

            return GetPostForQuery(BuildPostQuery(postQuery, searchTerm, GetRandomLinkIdForQuery(linkIdQuery, conn)), conn);
        }

        public PostData LookupNextPost(string searchTerm, int index, MySqlConnection conn)
        {
            throw new NotImplementedException();
        }

        public PostData LookupPreviousPost(string searchTerm, int index, MySqlConnection conn)
        {
            throw new NotImplementedException();
        }

        public string LookupTagById(int id, MySqlConnection conn)
        {
            string tagQuery = $"SELECT name from series WHERE id = '{id}'";

            return LookupTagById(tagQuery, conn);
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

            dataQuery = $"SELECT name, id, post_count FROM series WHERE name IN ({sb})";

            return LookupTagData(dataQuery, conn);
        }

        public new List<string> LookupTags(string searchTerm, MySqlConnection conn)
        {
            searchTerm = TagParser.EscapeApostrophe(searchTerm);

            string tagQuery = $"SELECT name from series WHERE name LIKE '{searchTerm}'";

            return base.LookupTags(tagQuery, conn);
        }

        public new bool HasExactMatch(string searchTerm, MySqlConnection conn, out string matched)
        {
            searchTerm = TagParser.EscapeApostrophe(searchTerm);

            string tagQuery = $"SELECT name from series WHERE name LIKE '{searchTerm}'";

            return base.HasExactMatch(tagQuery, conn, out matched);
        }

        public List<string> CharactersInSeries(string seriesName, MySqlConnection conn)
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
                cmd = new MySqlCommand(characterQuery, conn);

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
    }
}
