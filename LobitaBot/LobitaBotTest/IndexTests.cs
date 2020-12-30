using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace LobitaBot.Tests
{
    [TestClass()]
    public class IndexTests
    {
        ITagIndex index = new DbTagIndex();
        private string exampleTag = "gawr_gura";
        private string withApostrophe = "ninomae_ina'nis";
        private string searchTerm1;
        private string searchTerm2;
        private string nonExistant = "couldneverexist";

        [TestInitialize]
        public void Setup()
        {
            searchTerm1 = exampleTag.Split("_")[0];
            searchTerm2 = exampleTag.Split("_")[1];
        }

        [TestMethod()]
        public void LookupRandomTest()
        {
            Assert.IsFalse(string.IsNullOrEmpty(index.LookupRandomLink(exampleTag)));
            Assert.IsFalse(string.IsNullOrEmpty(index.LookupRandomLink(withApostrophe)));
            Assert.IsTrue(string.IsNullOrEmpty(index.LookupRandomLink(nonExistant)));
        }

        [TestMethod()]
        public void TagExistsTest()
        {
            Assert.IsTrue(index.TagExists(exampleTag));
            Assert.IsTrue(index.TagExists(withApostrophe));
            Assert.IsFalse(index.TagExists(nonExistant));
        }

        [TestMethod()]
        public void LookupTagsTest()
        {
            Assert.IsTrue(index.LookupTags(searchTerm1).Count != 0);
            Assert.IsTrue(index.LookupTags(searchTerm2).Count != 0);
            Assert.IsTrue(index.LookupTags(nonExistant).Count == 0);
        }

        [TestMethod()]
        public void LookupRandomTagTest()
        {
            Assert.IsTrue(index.TagExists(index.LookupRandomTag()));
        }

        [TestMethod()]
        public void LookupTagDataTest()
        {
            List<string> tags = new List<string> { exampleTag, withApostrophe };
            List<TagData> tagData = index.LookupTagData(tags);

            Assert.IsTrue(tagData.Count == tags.Count);

            foreach (TagData td in tagData)
            {
                Assert.IsNotNull(td);

                Assert.IsFalse(string.IsNullOrEmpty(td.TagName));
                Assert.IsTrue(td.TagID > 0);
                Assert.IsTrue(td.NumLinks >= 0);
            }
        }
    }
}
