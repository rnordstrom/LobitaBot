using System.Collections.Generic;

namespace LobitaBot
{
    public interface ITagIndex
    {
        string LookupRandom(string searchTerm);
        string LookupSingleTag(string searchTerm);
        List<string> LookupTags(string searchTerm);
    }
}
