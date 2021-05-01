using System.Collections.Generic;

namespace LobitaBot
{
    public class AdditionalPostData
    {
        public AdditionalPostData(List<int> additionalTagIds, List<string> additionalTagNames, List<string> additionalSeriesNames)
        {
            AdditionalTagIds = additionalTagIds;
            AdditionalTagNames = additionalTagNames;
            AdditionalSeriesNames = additionalSeriesNames;
        }

        public List<int> AdditionalTagIds { get; set; }
        public List<string> AdditionalTagNames { get; set; }
        public List<string> AdditionalSeriesNames { get; set; }
    }
}
