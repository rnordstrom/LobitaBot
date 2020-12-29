using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LobitaBot.Tests
{
    [TestClass()]
    public class ParseTagTests
    {
        private TagParser parser = new TagParser();
        private ITagIndex index = new DbTagIndex();
        private string exampleTag = "eastern_wolf_(kemono_friends)";

        [TestMethod()]
        public void BuildTitleTest()
        {
            string title = parser.BuildTitle(exampleTag).TrimEnd();
            string[] exampleParts = exampleTag.Split("_");
            string[] titleParts = title.ToLower().Split(" ");

            Assert.AreEqual(exampleParts.Length, titleParts.Length);

            for (int i = 0; i < exampleParts.Length; i++)
            {
                Assert.AreEqual(exampleParts[i], titleParts[i]);
            }
        }

        [TestMethod()]
        public void BuildSuggestionsTest()
        {
            string part1 = exampleTag.Split("_")[0];
            string part2 = exampleTag.Split("_")[1];
            string partial = exampleTag.Split("(")[0];
            string suggestions;

            suggestions = parser.BuildSuggestions(index.LookupTags(part1), part1);

            Assert.IsTrue(suggestions.Length < TagParser.MaxDescriptionSize);
            Assert.IsTrue(suggestions.Contains(exampleTag));

            suggestions = parser.BuildSuggestions(index.LookupTags(part2), part2);

            Assert.IsTrue(suggestions.Length < TagParser.MaxDescriptionSize);
            Assert.IsTrue(suggestions.Contains(exampleTag));

            suggestions = parser.BuildSuggestions(index.LookupTags(partial), partial);

            Assert.IsTrue(suggestions.Length < TagParser.MaxDescriptionSize);
            Assert.IsTrue(suggestions.Contains(exampleTag));
        }
    }
}
