using System.Collections.Generic;

namespace LobitaBot
{
    public class PostData
    {
        public PostData(int tagId, string tagName, string link, string seriesName, int postIndex, int linkId)
        {
            TagId = tagId;
            AdditionalTagIds = null;
            TagName = tagName;
            AdditionalTagNames = null;
            LinkId = linkId;
            Link = link;
            SeriesName = seriesName;
            PostIndex = postIndex;
        }

        public int TagId { get; }
        public List<int> AdditionalTagIds { get; set; }
        public string TagName { get; }
        public List<string> AdditionalTagNames { get; set; }
        public string SeriesName { get; }
        public string Link { get; }
        public int PostIndex { get; }
        public int LinkId { get; }
    };
}
