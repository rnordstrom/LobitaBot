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

        private void SetupSort()
        {
            List<List<TagData>> pages = new List<List<TagData>>();

            List<TagData> page1 = new List<TagData>();
            List<TagData> page2 = new List<TagData>();

            TagData t1 = new TagData("h", 0);
            TagData t2 = new TagData("b", 0);
            TagData t3 = new TagData("t", 0);
            TagData t4 = new TagData("a", 0);
            TagData t5 = new TagData("c", 0);
            TagData t6 = new TagData("v", 0);

            page1.Add(t1);
            page1.Add(t2);
            page1.Add(t3);

            for (int i = 3; i < 25; i++)
            {
                page1.Add(new TagData("c", 0));
            }

            for (int i = 25; i < 47; i++)
            {
                page2.Add(new TagData("h", 0));
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