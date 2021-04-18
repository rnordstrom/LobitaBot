using System;
using System.Collections.Generic;

namespace LobitaBot
{
    public class PageData
    {
        public List<List<TagData>> Pages { get; set; }
        public int PageNum { get; set; }
        public DateTime DateTime { get; }
        public bool AlphabeticallySorted { get; set; } = false;
        public bool NumericallySorted { get; set; } = false;
        public bool SortedAscending { get; set; } = false;

        public PageData(List<List<TagData>> pages)
        {
            Pages = pages;
            PageNum = 0;
            DateTime = DateTime.Now;
        }
    }
}
