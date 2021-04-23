namespace LobitaBot
{
    public class TagData
    {
        public TagData(string tagName, int tagID)
        {
            TagName = tagName;
            TagID = tagID;
        }

        public string TagName { get; }
        public int TagID { get; }
    }
}
