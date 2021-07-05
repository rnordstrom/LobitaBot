using System.Collections.Generic;

namespace LobitaBot
{
    public class PostData
    {
        public PostData(int tagId, string tagName, string link, string seriesName, int linkId, int postCount)
        {
            TagId = tagId;
            TagName = tagName;
            LinkId = linkId;
            Link = link;
            SeriesName = seriesName;
            PostCount = postCount;
            AdditionalData = null;
        }

        public int TagId { get; }
        public string TagName { get; }
        public string SeriesName { get; }
        public string Link { get; }
        public int LinkId { get; }
        public int PostCount { get; set; }
        public int PostIndex { get; set; }
        public AdditionalPostData AdditionalData { get; set; }
    };
}
