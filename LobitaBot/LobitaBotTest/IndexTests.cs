using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace LobitaBot.Tests
{
    [TestClass()]
    public class IndexTests
    {
        DbCharacterIndex charIndex = 
            new DbCharacterIndex(ConfigUtils.GetCurrentDatabase(Constants.TestConfig), new CacheService());
        DbSeriesIndex seriesIndex = 
            new DbSeriesIndex(ConfigUtils.GetCurrentDatabase(Constants.TestConfig), new CacheService());
        private string exampleTag = "gawr_gura";
        private string withApostrophe = "ninomae_ina'nis";
        private string nonExistant = "couldneverexist";
        private string seriesName = "hololive";

        [TestMethod]
        public void LookupRandomTest()
        {
            PostData pd = charIndex.LookupRandomPost(exampleTag);

            Assert.IsNotNull(pd);
            Assert.AreNotEqual(0, pd.TagId);
            Assert.IsFalse(string.IsNullOrEmpty(pd.TagName));
            Assert.IsFalse(string.IsNullOrEmpty(pd.Link));
            Assert.IsFalse(string.IsNullOrEmpty(pd.SeriesName));

            pd = charIndex.LookupRandomPost(withApostrophe);

            Assert.IsNotNull(pd);
            Assert.AreNotEqual(0, pd.TagId);
            Assert.IsFalse(string.IsNullOrEmpty(pd.TagName));
            Assert.IsFalse(string.IsNullOrEmpty(pd.Link));
            Assert.IsFalse(string.IsNullOrEmpty(pd.SeriesName));

            Assert.IsNull(charIndex.LookupRandomPost(nonExistant));

            pd = seriesIndex.LookupRandomPost(seriesName);

            Assert.IsNotNull(pd);
            Assert.AreNotEqual(0, pd.TagId);
            Assert.IsFalse(string.IsNullOrEmpty(pd.TagName));
            Assert.IsFalse(string.IsNullOrEmpty(pd.Link));
            Assert.IsFalse(string.IsNullOrEmpty(pd.SeriesName));
        }

        [TestMethod]
        public void LookupNextTest()
        {
            int index = 1;
            PostData pd = charIndex.LookupNextPost(exampleTag, index);

            Assert.IsNotNull(pd);
            Assert.AreEqual(index + 1, pd.PostIndex);

            index = 999;
            pd = charIndex.LookupNextPost(exampleTag, index);

            Assert.IsNotNull(pd);
            Assert.AreEqual(2, pd.PostIndex);
        }

        [TestMethod]
        public void LookupPreviousTest()
        {
            int index = 1;
            PostData pd = charIndex.LookupPreviousPost(exampleTag, index);

            Assert.IsNotNull(pd);
            Assert.AreEqual(index - 1, pd.PostIndex);

            index = -999;
            pd = charIndex.LookupPreviousPost(exampleTag, index);

            Assert.IsNotNull(pd);
            Assert.AreEqual(0, pd.PostIndex);
        }

        [TestMethod]
        public void TagExistsTest()
        {
            Assert.IsTrue(charIndex.TagExists(exampleTag));
            Assert.IsTrue(charIndex.TagExists(withApostrophe));
            Assert.IsFalse(charIndex.TagExists(nonExistant));

            Assert.IsTrue(seriesIndex.TagExists(seriesName));
        }

        [TestMethod]
        public void LookupTagsTest()
        {
            Assert.IsTrue(charIndex.LookupTags(exampleTag).Count != 0);
            Assert.IsTrue(charIndex.LookupTags(withApostrophe).Count != 0);
            Assert.IsTrue(charIndex.LookupTags(nonExistant).Count == 0);

            Assert.IsTrue(seriesIndex.LookupTags(seriesName).Count != 0);
        }

        [TestMethod]
        public void LookupRandomTagTest()
        {
            Assert.IsTrue(charIndex.TagExists(charIndex.LookupRandomTag()));
        }

        [TestMethod]
        public void LookupTagDataTest()
        {
            List<string> tags = new List<string> { exampleTag, withApostrophe };
            List<TagData> tagData = charIndex.LookupTagData(tags);

            Assert.AreEqual(tags.Count, tagData.Count);

            foreach (TagData td in tagData)
            {
                Assert.IsNotNull(td);

                Assert.IsFalse(string.IsNullOrEmpty(td.TagName));
                Assert.IsTrue(td.TagID > 0);
            }

            List<string> series = new List<string> { seriesName };
            List<TagData> seriesData = seriesIndex.LookupTagData(series);

            Assert.AreEqual(series.Count, seriesData.Count);

            foreach (TagData td in seriesData)
            {
                Assert.IsNotNull(td);

                Assert.IsFalse(string.IsNullOrEmpty(td.TagName));
                Assert.IsTrue(td.TagID > 0);
            }
        }

        [TestMethod]
        public void TestSeriesCharacters()
        {
            List<string> characters;
            List<string> series;

            characters = seriesIndex.CharactersInSeries(seriesName);
            series = charIndex.SeriesWithCharacter(exampleTag);

            Assert.AreEqual(2, characters.Count);
            Assert.IsTrue(characters.Contains(exampleTag));
            Assert.IsTrue(characters.Contains(withApostrophe));

            Assert.AreEqual(1, series.Count);
            Assert.IsTrue(series.Contains(seriesName));
        }

        [TestMethod]
        public void TestCharactersInPost()
        {
            List<string> post1 = charIndex.CharactersInPost(1);
            List<string> post2 = charIndex.CharactersInPost(3);

            Assert.AreEqual(1, post1.Count);
            Assert.AreEqual(2, post2.Count);

            Assert.IsTrue(post1.Contains(exampleTag));
            Assert.IsTrue(post2.Contains(exampleTag));
            Assert.IsTrue(post2.Contains(withApostrophe));
        }
    }
}
