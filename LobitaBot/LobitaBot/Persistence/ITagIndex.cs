using MySql.Data.MySqlClient;
using System.Collections.Generic;

namespace LobitaBot
{
    public interface ITagIndex
    {
        MySqlConnection GetConnection();
        PostData LookupRandomPost(string searchTerm, MySqlConnection conn);
        PostData LookupNextPost(string searchTerm, int index, MySqlConnection conn);
        PostData LookupPreviousPost(string searchTerm, int index, MySqlConnection conn);
        bool HasExactMatch(string searchTerm, MySqlConnection conn, out string matched);
        string LookupTagById(int id, MySqlConnection conn);
        List<string> LookupTags(string searchTerm, MySqlConnection conn);
        List<TagData> LookupTagData(List<string> tags, MySqlConnection conn);
    }
}
