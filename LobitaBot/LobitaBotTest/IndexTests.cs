using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace LobitaBot.Tests
{
    [TestClass()]
    public class IndexTests
    {
        DbCharacterIndex charIndex = new DbCharacterIndex("tagdb_test");
        DbSeriesIndex seriesIndex = new DbSeriesIndex("tagdb_test");
        private string exampleTag = "gawr_gura";
        private string withApostrophe = "ninomae_ina'nis";
        private string nonExistant = "couldneverexist";
        private string seriesName = "hololive";
        private string searchTerm1;
        private string searchTerm2;

        [TestInitialize]
        public void Setup()
        {
            searchTerm1 = exampleTag.Split("_")[0];
            searchTerm2 = exampleTag.Split("_")[1];
        }

        [TestMethod]
        public void LookupRandomTest()
        {
            Assert.AreNotEqual(0, charIndex.LookupRandomPost(exampleTag).PostId);
            Assert.IsFalse(string.IsNullOrEmpty(charIndex.LookupRandomPost(exampleTag).TagName));
            Assert.IsFalse(string.IsNullOrEmpty(charIndex.LookupRandomPost(exampleTag).Link));
            Assert.IsFalse(string.IsNullOrEmpty(charIndex.LookupRandomPost(exampleTag).SeriesName));

            Assert.AreNotEqual(0, charIndex.LookupRandomPost(withApostrophe).PostId);
            Assert.IsFalse(string.IsNullOrEmpty(charIndex.LookupRandomPost(withApostrophe).TagName));
            Assert.IsFalse(string.IsNullOrEmpty(charIndex.LookupRandomPost(withApostrophe).Link));
            Assert.IsFalse(string.IsNullOrEmpty(charIndex.LookupRandomPost(withApostrophe).SeriesName));

            Assert.AreEqual(0, charIndex.LookupRandomPost(nonExistant).PostId);
            Assert.IsTrue(string.IsNullOrEmpty(charIndex.LookupRandomPost(nonExistant).TagName));
            Assert.IsTrue(string.IsNullOrEmpty(charIndex.LookupRandomPost(nonExistant).Link));
            Assert.IsTrue(string.IsNullOrEmpty(charIndex.LookupRandomPost(nonExistant).SeriesName));

            Assert.AreNotEqual(0, seriesIndex.LookupRandomPost(seriesName).PostId);
            Assert.IsFalse(string.IsNullOrEmpty(seriesIndex.LookupRandomPost(seriesName).TagName));
            Assert.IsFalse(string.IsNullOrEmpty(seriesIndex.LookupRandomPost(seriesName).Link));
            Assert.IsFalse(string.IsNullOrEmpty(seriesIndex.LookupRandomPost(seriesName).SeriesName));
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
            Assert.IsTrue(charIndex.LookupTags(searchTerm1).Count != 0);
            Assert.IsTrue(charIndex.LookupTags(searchTerm2).Count != 0);
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

            Assert.IsTrue(tagData.Count == tags.Count);

            foreach (TagData td in tagData)
            {
                Assert.IsNotNull(td);

                Assert.IsFalse(string.IsNullOrEmpty(td.TagName));
                Assert.IsTrue(td.TagID > 0);
                Assert.IsTrue(td.NumLinks >= 0);
            }

            List<string> series = new List<string> { seriesName };
            List<TagData> seriesData = seriesIndex.LookupTagData(series);

            Assert.IsTrue(seriesData.Count == series.Count);

            foreach (TagData td in seriesData)
            {
                Assert.IsNotNull(td);

                Assert.IsFalse(string.IsNullOrEmpty(td.TagName));
                Assert.IsTrue(td.TagID > 0);
                Assert.IsTrue(td.NumLinks >= 0);
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
    }
}
