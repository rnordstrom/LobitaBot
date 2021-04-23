﻿using System.Collections.Generic;

namespace LobitaBot
{
    public interface ITagIndex
    {
        PostData LookupRandomPost(string searchTerm);
        PostData LookupNextPost(string searchTerm, int index);
        PostData LookupPreviousPost(string searchTerm, int index);
        bool HasExactMatch(string searchTerm, out string matched);
        string LookupSingleTag(int id);
        List<string> LookupTags(string searchTerm);
        List<TagData> LookupTagData(List<string> tags);
    }
}
