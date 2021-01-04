﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace LobitaBot.Tests
{
    [TestClass()]
    public class ParseTagTests
    {
        private ITagIndex index = new DbCharacterIndex("tagdb_test");
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
            string title = TagParser.BuildTitle(exampleTag).TrimEnd();
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
            List<string> suggestions = TagParser.FilterSuggestions(tags, part1);

            Assert.IsTrue(suggestions.Count <= tags.Count);
            Assert.IsTrue(suggestions.Contains(exampleTag));

            tags = index.LookupTags(part2);
            suggestions = TagParser.FilterSuggestions(tags, part2);

            Assert.IsTrue(suggestions.Count <= tags.Count);
            Assert.IsTrue(suggestions.Contains(exampleTag));

            tags = index.LookupTags(partial);
            suggestions = TagParser.FilterSuggestions(tags, partial);

            Assert.IsTrue(suggestions.Count <= tags.Count);
            Assert.IsTrue(suggestions.Contains(exampleTag2));
        }

        [TestMethod()]
        public void CompileSuggestionsTest()
        {
            List<string> tags = index.LookupTags(part1);
            List<string> suggestions = TagParser.FilterSuggestions(tags, part1);
            List<TagData> tagData = index.LookupTagData(suggestions);
            List<string> description = TagParser.CompileSuggestions(tagData);
            bool found = false;

            foreach (string s in description)
            {
                Assert.IsTrue(s.Length < TagParser.MaxDescriptionSize);

                if (s.Contains(exampleTag))
                {
                    found = true;
                }
            }

            Assert.IsTrue(found);

            tags = index.LookupTags(part2);
            suggestions = TagParser.FilterSuggestions(tags, part2);
            tagData = index.LookupTagData(suggestions);
            description = TagParser.CompileSuggestions(tagData);
            found = false;

            foreach (string s in description)
            {
                Assert.IsTrue(s.Length < TagParser.MaxDescriptionSize);

                if (s.Contains(exampleTag))
                {
                    found = true;
                }
            }

            Assert.IsTrue(found);

            tags = index.LookupTags(partial);
            suggestions = TagParser.FilterSuggestions(tags, partial);
            tagData = index.LookupTagData(suggestions);
            description = TagParser.CompileSuggestions(tagData);
            found = false;

            foreach (string s in description)
            {
                Assert.IsTrue(s.Length < TagParser.MaxDescriptionSize);

                if (s.Contains(exampleTag2))
                {
                    found = true;
                }
            }

            Assert.IsTrue(found);
        }
    }
}