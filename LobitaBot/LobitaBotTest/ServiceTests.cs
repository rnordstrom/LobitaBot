using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace LobitaBot.Tests
{
    [TestClass()]
    public class ServiceTests
    {
        private PageService service = new PageService();
        const ulong FirstId = 0;

        [TestMethod()]
        public void AddLimitedTest()
        {
            ulong id = FirstId;
            List<List<TagData>> pages = new List<List<TagData>>();
            PageData pageData = new PageData(pages);

            service.AddLimited(FirstId, pageData);

            for (int i = 0; i < 100; i++)
            {
                pages = new List<List<TagData>>();
                pageData = new PageData(pages);

                service.AddLimited(++id, pageData);
            }

            Assert.IsTrue(service.PageIndex.Count == 100);
            Assert.IsFalse(service.PageIndex.ContainsKey(FirstId));
        }

        // The tests below will fail if Discord's maximum number of fields per embed changes.
        // In that case, pay extra attention to the loops' exit conditions in the method SetupSort.

        [TestMethod()]
        public void SortAlphabeticalTest()
        {
            SetupSort();

            service.SortAlphabeticalAsc(FirstId);

            Assert.AreEqual("a", service.PageIndex[FirstId].Pages[0][0].TagName);
            Assert.AreEqual("b", service.PageIndex[FirstId].Pages[0][1].TagName);
            Assert.AreEqual("c", service.PageIndex[FirstId].Pages[0][2].TagName);
            Assert.AreEqual("h", service.PageIndex[FirstId].Pages[1][22].TagName);
            Assert.AreEqual("t", service.PageIndex[FirstId].Pages[1][23].TagName);
            Assert.AreEqual("v", service.PageIndex[FirstId].Pages[1][24].TagName);

            service.SortAlphabeticalDesc(FirstId);

            Assert.AreEqual("v", service.PageIndex[FirstId].Pages[0][0].TagName);
            Assert.AreEqual("t", service.PageIndex[FirstId].Pages[0][1].TagName);
            Assert.AreEqual("h", service.PageIndex[FirstId].Pages[0][2].TagName);
            Assert.AreEqual("c", service.PageIndex[FirstId].Pages[1][22].TagName);
            Assert.AreEqual("b", service.PageIndex[FirstId].Pages[1][23].TagName);
            Assert.AreEqual("a", service.PageIndex[FirstId].Pages[1][24].TagName);
        }

        [TestMethod()]
        public void SortNumPostsTest()
        {
            SetupSort();

            service.SortPostNumAsc(FirstId);

            Assert.AreEqual("h", service.PageIndex[FirstId].Pages[0][0].TagName);
            Assert.AreEqual("b", service.PageIndex[FirstId].Pages[0][1].TagName);
            Assert.AreEqual("t", service.PageIndex[FirstId].Pages[0][2].TagName);
            Assert.AreEqual("a", service.PageIndex[FirstId].Pages[1][22].TagName);
            Assert.AreEqual("c", service.PageIndex[FirstId].Pages[1][23].TagName);
            Assert.AreEqual("v", service.PageIndex[FirstId].Pages[1][24].TagName);

            service.SortPostNumDesc(FirstId);

            Assert.AreEqual("v", service.PageIndex[FirstId].Pages[0][0].TagName);
            Assert.AreEqual("c", service.PageIndex[FirstId].Pages[0][1].TagName);
            Assert.AreEqual("a", service.PageIndex[FirstId].Pages[0][2].TagName);
            Assert.AreEqual("t", service.PageIndex[FirstId].Pages[1][22].TagName);
            Assert.AreEqual("b", service.PageIndex[FirstId].Pages[1][23].TagName);
            Assert.AreEqual("h", service.PageIndex[FirstId].Pages[1][24].TagName);
        }

        [TestMethod]
        public void CacheServiceTests()
        {
            CacheService cacheService = new CacheService();

            Assert.IsTrue(cacheService.IsEmpty());

            PostData pd1 = new PostData(1, "gawr_gura", "1.jpg", "hololive", 0, 1);
            PostData pd2 = new PostData(2, "usada_pekora", "2.jpg", "hololive", 0, 2);

            cacheService.Add(pd1);

            Assert.IsTrue(cacheService.CharacterAloneInCache(pd1.TagName));
            Assert.IsTrue(cacheService.SeriesInCache(pd1.SeriesName));

            cacheService.Add(pd2);

            Assert.IsFalse(cacheService.CharacterAloneInCache(pd1.TagName));
            Assert.IsFalse(cacheService.CharacterAloneInCache(pd2.TagName));
            Assert.IsTrue(cacheService.SeriesInCache(pd1.SeriesName));

            cacheService.Clear();

            List<string> additionalTagNames = new List<string>();
            additionalTagNames.Add(pd2.TagName);

            pd1.AdditionalData = new AdditionalPostData(new List<int>(), additionalTagNames, new List<string>());
            cacheService.Add(pd1);

            Assert.IsTrue(cacheService.CollabInCache(new string[] { pd1.TagName, pd2.TagName }));

            cacheService.Clear();

            Assert.IsTrue(cacheService.IsEmpty());
        }

        private void SetupSort()
        {
            List<List<TagData>> pages = new List<List<TagData>>();

            List<TagData> page1 = new List<TagData>();
            List<TagData> page2 = new List<TagData>();

            TagData t1 = new TagData("h", 0, 1);
            TagData t2 = new TagData("b", 0, 2);
            TagData t3 = new TagData("t", 0, 3);
            TagData t4 = new TagData("a", 0, 5);
            TagData t5 = new TagData("c", 0, 6);
            TagData t6 = new TagData("v", 0, 7);

            page1.Add(t1);
            page1.Add(t2);
            page1.Add(t3);

            for (int i = 3; i < 25; i++)
            {
                page1.Add(new TagData("c", 0, 4));
            }

            for (int i = 25; i < 47; i++)
            {
                page2.Add(new TagData("h", 0, 4));
            }

            page2.Add(t4);
            page2.Add(t5);
            page2.Add(t6);

            pages.Add(page1);
            pages.Add(page2);

            PageData pageData = new PageData(pages);

            service.AddLimited(FirstId, pageData);
        }
    }
}