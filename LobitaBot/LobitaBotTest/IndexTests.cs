using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

namespace LobitaBot.Tests
{
    [TestClass()]
    public class IndexTests
    {
        DbCharacterIndex charIndex = 
            new DbCharacterIndex(
                ConfigUtils.GetCurrentDatabase(Constants.TestConfig));
        DbSeriesIndex seriesIndex = 
            new DbSeriesIndex(
                ConfigUtils.GetCurrentDatabase(Constants.TestConfig));
        private string exampleTag = "gawr_gura";
        private string withApostrophe = "ninomae_ina'nis";
        private string nonExistant = "couldneverexist";
        private string seriesName = "hololive";

        [TestMethod]
        public void LookupRandomPostTest()
        {
            using (MySqlConnection charConn = charIndex.GetConnection())
            using (MySqlConnection seriesConn = charIndex.GetConnection())
            {
                PostData pd = charIndex.LookupRandomPost(exampleTag, charConn);

                Assert.IsNotNull(pd);
                Assert.AreNotEqual(0, pd.TagId);
                Assert.IsFalse(string.IsNullOrEmpty(pd.TagName));
                Assert.IsFalse(string.IsNullOrEmpty(pd.Link));
                Assert.IsFalse(string.IsNullOrEmpty(pd.SeriesName));
                Assert.IsNull(pd.AdditionalData);

                pd = charIndex.LookupRandomPost(withApostrophe, charConn);

                Assert.IsNotNull(pd);
                Assert.AreNotEqual(0, pd.TagId);
                Assert.IsFalse(string.IsNullOrEmpty(pd.TagName));
                Assert.IsFalse(string.IsNullOrEmpty(pd.Link));
                Assert.IsFalse(string.IsNullOrEmpty(pd.SeriesName));
                Assert.IsNull(pd.AdditionalData);

                Assert.IsNull(charIndex.LookupRandomPost(nonExistant, charConn));

                pd = seriesIndex.LookupRandomPost(seriesName, seriesConn);

                Assert.IsNotNull(pd);
                Assert.AreNotEqual(0, pd.TagId);
                Assert.IsFalse(string.IsNullOrEmpty(pd.TagName));
                Assert.IsFalse(string.IsNullOrEmpty(pd.Link));
                Assert.IsFalse(string.IsNullOrEmpty(pd.SeriesName));
                Assert.IsNull(pd.AdditionalData);
            }
        }

        [TestMethod]
        public void LookupNextPostTest()
        {
            using (MySqlConnection conn = charIndex.GetConnection())
            {
                int postId = 2;
                PostData pd = charIndex.LookupNextPost(exampleTag, postId, conn);

                Assert.IsNotNull(pd);
                Assert.AreEqual(2, pd.PostIndex);

                postId = 3;
                pd = charIndex.LookupNextPost(exampleTag, postId, conn);

                Assert.IsNotNull(pd);
                Assert.AreEqual(2, pd.PostIndex);
            }
        }

        [TestMethod]
        public void LookupPreviousPostTest()
        {
            using (MySqlConnection conn = charIndex.GetConnection())
            {
                int postId = 2;
                PostData pd = charIndex.LookupPreviousPost(exampleTag, postId, conn);

                Assert.IsNotNull(pd);
                Assert.AreEqual(0, pd.PostIndex);

                postId = 1;
                pd = charIndex.LookupPreviousPost(exampleTag, postId, conn);

                Assert.IsNotNull(pd);
                Assert.AreEqual(0, pd.PostIndex);
            }
        }

        [TestMethod]
        public void TagExistsTest()
        {
            using (MySqlConnection charConn = charIndex.GetConnection())
            using (MySqlConnection seriesConn = charIndex.GetConnection())
            {
                Assert.IsTrue(charIndex.HasExactMatch(exampleTag, charConn, out string _));
                Assert.IsTrue(charIndex.HasExactMatch(withApostrophe, charConn, out string _));
                Assert.IsFalse(charIndex.HasExactMatch(nonExistant, charConn, out string _));

                Assert.IsTrue(seriesIndex.HasExactMatch(seriesName, seriesConn, out string _));
            }
        }

        [TestMethod]
        public void LookupRandomCollabTest()
        {
            using (MySqlConnection conn = charIndex.GetConnection())
            {
                PostData pd = charIndex.LookupRandomCollab(new string[] { exampleTag, withApostrophe }, conn);

                Assert.IsNotNull(pd);
                Assert.IsNotNull(pd.LinkId);
                Assert.IsNotNull(pd.PostIndex);
                Assert.IsNotNull(pd.AdditionalData);
                Assert.AreEqual(exampleTag, pd.TagName);
                Assert.AreEqual(seriesName, pd.SeriesName);
                Assert.AreEqual("3.png", pd.Link);
            }
        }

        [TestMethod]
        public void LookupNextCollabTest()
        {
            using (MySqlConnection conn = charIndex.GetConnection())
            {
                int postId = 3;
                PostData pd = charIndex.LookupNextCollab(new string[] { exampleTag, withApostrophe }, postId, conn);

                Assert.IsNotNull(pd);
                Assert.AreEqual(0, pd.PostIndex);
            }
        }

        [TestMethod]
        public void LookupPreviousCollabTest()
        {
            using (MySqlConnection conn = charIndex.GetConnection())
            {
                int postId = 3;
                PostData pd = charIndex.LookupPreviousCollab(new string[] { exampleTag, withApostrophe }, postId, conn);

                Assert.IsNotNull(pd);
                Assert.AreEqual(0, pd.PostIndex);
            }
        }

        [TestMethod]
        public void LookupTagNameIdTest()
        {
            using (MySqlConnection conn = charIndex.GetConnection())
            {
                Assert.AreEqual(1, charIndex.LookupTagIdByName(exampleTag, conn));
                Assert.AreEqual(exampleTag, charIndex.LookupTagById(1, conn));
            }
        }

        [TestMethod]
        public void LookupTagsTest()
        {
            using (MySqlConnection charConn = charIndex.GetConnection())
            using (MySqlConnection seriesConn = charIndex.GetConnection())
            {
                Assert.IsTrue(charIndex.LookupTags(exampleTag, charConn).Count != 0);
                Assert.IsTrue(charIndex.LookupTags(withApostrophe, charConn).Count != 0);
                Assert.IsTrue(charIndex.LookupTags(nonExistant, charConn).Count == 0);

                Assert.IsTrue(seriesIndex.LookupTags(seriesName, seriesConn).Count != 0);
            }
        }

        [TestMethod]
        public void LookupRandomTagTest()
        {
            using (MySqlConnection conn = charIndex.GetConnection())
            {
                Assert.IsTrue(charIndex.HasExactMatch(charIndex.LookupRandomTag(conn), conn, out string _));
            }
        }

        [TestMethod]
        public void LookupTagDataTest()
        {
            using (MySqlConnection charConn = charIndex.GetConnection())
            using (MySqlConnection seriesConn = charIndex.GetConnection())
            {
                List<string> tags = new List<string> { exampleTag, withApostrophe };
                List<TagData> tagData = charIndex.LookupTagData(tags, charConn);

                Assert.AreEqual(tags.Count, tagData.Count);

                foreach (TagData td in tagData)
                {
                    Assert.IsNotNull(td);

                    Assert.IsFalse(string.IsNullOrEmpty(td.TagName));
                    Assert.IsTrue(td.TagID > 0);
                    Assert.IsTrue(td.NumLinks >= 0);
                }

                List<string> series = new List<string> { seriesName };
                List<TagData> seriesData = seriesIndex.LookupTagData(series, seriesConn);

                Assert.AreEqual(series.Count, seriesData.Count);

                foreach (TagData td in seriesData)
                {
                    Assert.IsNotNull(td);

                    Assert.IsFalse(string.IsNullOrEmpty(td.TagName));
                    Assert.IsTrue(td.TagID > 0);
                    Assert.IsTrue(td.NumLinks >= 0);
                }
            }
        }

        [TestMethod]
        public void TestSeriesCharacters()
        {
            List<string> characters;
            string series;

            using (MySqlConnection charConn = charIndex.GetConnection())
            using (MySqlConnection seriesConn = charIndex.GetConnection())
            {
                characters = seriesIndex.CharactersInSeries(seriesName, seriesConn);
                series = charIndex.SeriesWithCharacter(exampleTag, charConn);

                Assert.AreEqual(2, characters.Count);
                Assert.IsTrue(characters.Contains(exampleTag));
                Assert.IsTrue(characters.Contains(withApostrophe));

                Assert.AreEqual(series, seriesName);
            }
        }

        [TestMethod]
        public void TestCharactersInPost()
        {
            using (MySqlConnection conn = charIndex.GetConnection())
            {
                List<string> post1 = charIndex.CharactersInPost(1, conn);
                List<string> post2 = charIndex.CharactersInPost(3, conn);

                Assert.AreEqual(1, post1.Count);
                Assert.AreEqual(2, post2.Count);

                Assert.IsTrue(post1.Contains(exampleTag));
                Assert.IsTrue(post2.Contains(exampleTag));
                Assert.IsTrue(post2.Contains(withApostrophe));
            }
        }

        [TestMethod]
        public void CollabsWithCharactersTest()
        {
            using (MySqlConnection conn = charIndex.GetConnection())
            {
                List<string> collabList = charIndex.CollabsWithCharacters(new string[] { exampleTag }, conn);

                Assert.IsTrue(collabList.Count == 1);
                Assert.IsTrue(collabList.Contains(withApostrophe));
            }
        }
    }
}
