using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace LobitaBot.Tests
{
    [TestClass()]
    public class ParseTagTests
    {
        private ITagIndex index = 
            new DbCharacterIndex(
                ConfigUtils.GetCurrentDatabase(Constants.TestConfig), 
                ConfigUtils.GetBatchQueryLimit(Constants.TestConfig), 
                new CacheService());
        private string exampleTag = "gawr_gura";

        [TestMethod()]
        public void BuildTitleTest()
        {
            string title = TagParser.BuildTitle(exampleTag).TrimEnd();
            string[] exampleParts = exampleTag.Split("_");
            string[] titleParts = title.ToLower().Split(" ");

            Assert.AreEqual(exampleParts.Length, titleParts.Length);

            for (int i = 0; i < exampleParts.Length; i++)
            {
                Assert.AreEqual(exampleParts[i], titleParts[i]);
            }
        }

        [TestMethod()]
        public void CompileSuggestionsListTest()
        {
            List<List<TagData>> pages;
            List<TagData> tagData = new List<TagData>();
            const int MaxFields = 25;

            for (int i = 0; i < 105; i++)
            {
                tagData.Add(new TagData("a", 1, 1));
            }

            pages = TagParser.CompileSuggestions(tagData, MaxFields);

            Assert.AreEqual(5, pages.Count);
            Assert.AreEqual(MaxFields, pages[0].Count);
            Assert.AreEqual(MaxFields, pages[1].Count);
            Assert.AreEqual(MaxFields, pages[2].Count);
            Assert.AreEqual(MaxFields, pages[3].Count);
            Assert.AreEqual(5, pages[4].Count);

            tagData.Clear();

            for (int i = 0; i < 100; i++)
            {
                tagData.Add(new TagData("a", 1, 1));
            }

            pages = TagParser.CompileSuggestions(tagData, MaxFields);

            Assert.AreEqual(4, pages.Count);
            Assert.AreEqual(MaxFields, pages[0].Count);
            Assert.AreEqual(MaxFields, pages[1].Count);
            Assert.AreEqual(MaxFields, pages[2].Count);
            Assert.AreEqual(MaxFields, pages[3].Count);
        }
    }
}
