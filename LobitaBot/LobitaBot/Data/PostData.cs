namespace LobitaBot
{
    public class PostData
    {
        public PostData(int tagId, string tagName, string link, string seriesName, int postIndex, int linkId)
        {
            TagId = tagId;
            TagName = tagName;
            LinkId = linkId;
            Link = link;
            SeriesName = seriesName;
            PostIndex = postIndex;
        }

        public int TagId { get; }
        public string TagName { get; }
        public string SeriesName { get; }
        public string Link { get; }
        public int PostIndex { get; }
        public int LinkId { get; }
    };
}
