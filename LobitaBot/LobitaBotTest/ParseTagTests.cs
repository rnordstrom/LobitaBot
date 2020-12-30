using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace LobitaBot.Tests
{
    [TestClass()]
    public class ParseTagTests
    {
        private TagParser parser = new TagParser();
        private ITagIndex index = new DbTagIndex();
        private string exampleTag = "gawr_gura";
        private string exampleTag2 = "hilda_valentine_goneril";
        string partial = "hilda_valentine";
        string part1;
        string part2;

        [TestInitialize]
        public void Setup()
        {
            part1 = exampleTag.Split("_")[0];
            part2 = exampleTag.Split("_")[1];
        }

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
        public void FilterSuggestionsTest()
        {
            List<string> tags = index.LookupTags(part1);
            List<string> suggestions = parser.FilterSuggestions(tags, part1);

            Assert.IsTrue(suggestions.Count <= tags.Count);
            Assert.IsTrue(suggestions.Contains(exampleTag));

            tags = index.LookupTags(part2);
            suggestions = parser.FilterSuggestions(tags, part2);

            Assert.IsTrue(suggestions.Count <= tags.Count);
            Assert.IsTrue(suggestions.Contains(exampleTag));

            tags = index.LookupTags(partial);
            suggestions = parser.FilterSuggestions(tags, partial);

            Assert.IsTrue(suggestions.Count <= tags.Count);
            Assert.IsTrue(suggestions.Contains(exampleTag2));
        }

        [TestMethod()]
        public void CompileSuggestionsTest()
        {
            List<string> tags = index.LookupTags(part1);
            List<string> suggestions = parser.FilterSuggestions(tags, part1);
            List<TagData> tagData = index.LookupTagData(suggestions);
            string description = parser.CompileSuggestions(tagData);

            Assert.IsTrue(description.Length < TagParser.MaxDescriptionSize);
            Assert.IsTrue(description.Contains(exampleTag));

            tags = index.LookupTags(part2);
            suggestions = parser.FilterSuggestions(tags, part2);
            tagData = index.LookupTagData(suggestions);
            description = parser.CompileSuggestions(tagData);

            Assert.IsTrue(description.Length < TagParser.MaxDescriptionSize);
            Assert.IsTrue(description.Contains(exampleTag));

            tags = index.LookupTags(partial);
            suggestions = parser.FilterSuggestions(tags, partial);
            tagData = index.LookupTagData(suggestions);
            description = parser.CompileSuggestions(tagData);

            Assert.IsTrue(description.Length < TagParser.MaxDescriptionSize);
            Assert.IsTrue(description.Contains(exampleTag2));
        }
    }
}
