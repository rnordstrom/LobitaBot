using Discord;
using System.Collections.Generic;
using System.Linq;

namespace LobitaBot
{
    public class PageService
    {
        public bool HandlerAdded { get; set; } = false;
        public Dictionary<ulong, PageData> PageIndex { get; } = new Dictionary<ulong, PageData>();

        public void AddLimited(ulong msgId, PageData pageData)
        {
            if (PageIndex.Count == 100)
            {
                ulong oldestMsg = PageIndex.Aggregate((x, y) => x.Value.DateTime < y.Value.DateTime ? x : y).Key;

                PageIndex.Remove(oldestMsg);
            }

            PageIndex.Add(msgId, pageData);
        }

        public void SortAlphabeticalAsc(ulong msgId)
        {
            PageData pageData = PageIndex[msgId];
            List<TagData> tagData = new List<TagData>();

            foreach (List<TagData> page in pageData.Pages)
            {
                foreach (TagData t in page)
                {
                    tagData.Add(t);
                }
            }

            tagData.Sort((t1, t2) => t1.TagName.CompareTo(t2.TagName));

            PageIndex[msgId].Pages = TagParser.CompileSuggestions(tagData, EmbedBuilder.MaxFieldCount);
            PageIndex[msgId].AlphabeticallySorted = true;
            PageIndex[msgId].NumericallySorted = false;
            PageIndex[msgId].SortedAscending = true;
        }

        public void SortAlphabeticalDesc(ulong msgId)
        {
            PageData pageData = PageIndex[msgId];
            List<TagData> tagData = new List<TagData>();

            foreach (List<TagData> page in pageData.Pages)
            {
                foreach (TagData t in page)
                {
                    tagData.Add(t);
                }
            }

            tagData.Sort((t1, t2) => t2.TagName.CompareTo(t1.TagName));

            PageIndex[msgId].Pages = TagParser.CompileSuggestions(tagData, EmbedBuilder.MaxFieldCount);
            PageIndex[msgId].AlphabeticallySorted = true;
            PageIndex[msgId].NumericallySorted = false;
            PageIndex[msgId].SortedAscending = false;
        }

        public void SortPostNumAsc(ulong msgId)
        {
            PageData pageData = PageIndex[msgId];
            List<TagData> tagData = new List<TagData>();

            foreach (List<TagData> page in pageData.Pages)
            {
                foreach (TagData t in page)
                {
                    tagData.Add(t);
                }
            }

            tagData.Sort((t1, t2) => t1.NumLinks.CompareTo(t2.NumLinks));

            PageIndex[msgId].Pages = TagParser.CompileSuggestions(tagData, EmbedBuilder.MaxFieldCount);
            PageIndex[msgId].AlphabeticallySorted = false;
            PageIndex[msgId].NumericallySorted = true;
            PageIndex[msgId].SortedAscending = true;
        }

        public void SortPostNumDesc(ulong msgId)
        {
            PageData pageData = PageIndex[msgId];
            List<TagData> tagData = new List<TagData>();

            foreach (List<TagData> page in pageData.Pages)
            {
                foreach (TagData t in page)
                {
                    tagData.Add(t);
                }
            }

            tagData.Sort((t1, t2) => t2.NumLinks.CompareTo(t1.NumLinks));

            PageIndex[msgId].Pages = TagParser.CompileSuggestions(tagData, EmbedBuilder.MaxFieldCount);
            PageIndex[msgId].AlphabeticallySorted = false;
            PageIndex[msgId].NumericallySorted = true;
            PageIndex[msgId].SortedAscending = false;
        }
    }
}
