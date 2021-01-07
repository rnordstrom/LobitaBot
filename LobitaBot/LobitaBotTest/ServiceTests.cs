using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace LobitaBot.Tests
{
    [TestClass()]
    public class ServiceTests
    {
        private SearchService service = new SearchService();
        const ulong FirstId = 0;

        [TestMethod()]
        public void AddLimitedTest()
        {
            ulong id = FirstId;
            List<string> pages = new List<string>();
            PageData pageData = new PageData(pages);

            service.AddLimited(id, pageData);

            for (int i = 0; i < 100; i++)
            {
                pages = new List<string>();
                pageData = new PageData(pages);

                service.AddLimited(++id, pageData);
            }

            Assert.IsTrue(service.PageIndex.Count == 100);
            Assert.IsFalse(service.PageIndex.ContainsKey(FirstId));
        }
    }
}