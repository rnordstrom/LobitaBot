using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LobitaBot.Tests
{
    [TestClass()]
    public class IndexTests
    {
        ITagIndex index = new DbTagIndex();
        private string searchTerm = "gawr_gura";
        private string withApostrophe = "ninomae_ina'nis";
        private string searchTerm1;
        private string searchTerm2;
        private string nonExistant = "couldneverexist";

        [TestInitialize]
        public void Setup()
        {
            searchTerm1 = searchTerm.Split("_")[0];
            searchTerm2 = searchTerm.Split("_")[1];
        }

        [TestMethod()]
        public void LookupRandomTest()
        {
            Assert.IsFalse(string.IsNullOrEmpty(index.LookupRandom(searchTerm)));
            Assert.IsFalse(string.IsNullOrEmpty(index.LookupRandom(withApostrophe)));
            Assert.IsTrue(string.IsNullOrEmpty(index.LookupRandom(nonExistant)));
        }

        [TestMethod()]
        public void LookupSingleTagTest()
        {
            Assert.AreEqual(searchTerm, index.LookupSingleTag(searchTerm));
            Assert.AreEqual(withApostrophe, index.LookupSingleTag(withApostrophe));
            Assert.IsTrue(string.IsNullOrEmpty(index.LookupRandom(nonExistant)));
        }

        [TestMethod()]
        public void LookupTagsTest()
        {
            Assert.IsTrue(index.LookupTags(searchTerm1).Count != 0);
            Assert.IsTrue(index.LookupTags(searchTerm2).Count != 0);
            Assert.IsTrue(index.LookupTags(nonExistant).Count == 0);
        }
    }
}
