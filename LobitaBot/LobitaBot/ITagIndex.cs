using System.Collections.Generic;

namespace LobitaBot
{
    public interface ITagIndex
    {
        string LookupRandomLink(string searchTerm);
        string LookupRandomTag();
        bool TagExists(string searchTerm);
        string LookupSingleTag(int id);
        List<string> LookupTags(string searchTerm);
        List<TagData> LookupTagData(List<string> tags);
    }
}
