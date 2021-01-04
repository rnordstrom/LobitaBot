﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LobitaBot
{
    public static class TagParser
    {
        public const int MaxDescriptionSize = 1000;

        public static string BuildTitle(string searchTerm)
        {
            string[] parts = searchTerm.Split("_");
            StringBuilder sb = new StringBuilder();

            foreach (string s in parts)
            {
                if (!string.IsNullOrEmpty(s))
                {
                    if (!(s.First().ToString() == "("))
                    {
                        sb.Append($"{s.First().ToString().ToUpper() + s[1..]} ");
                    }
                    else
                    {
                        if (s.Length >= 2)
                        {
                            sb.Append($"{s.First().ToString() + s[1].ToString().ToUpper() + s[2..]} ");
                        }
                        else
                        {
                            sb.Append($"{s.First().ToString() + s[1].ToString().ToUpper()} ");
                        }
                    }
                }
            }

            return sb.ToString();
        }

        public static List<string> FilterSuggestions(List<string> tags, string searchTerm)
        {
            List<string> filtered = new List<string>();
            string replaced;

            foreach (string t in tags)
            {
                if (t.Contains('/'))
                {
                    replaced = t.Replace('/', '_');
                }
                else
                {
                    replaced = t;
                }

                if (searchTerm.Contains("_"))
                {
                    if (replaced.Split("(")[0].Contains(searchTerm))
                    {
                        filtered.Add(t);
                    }
                }
                else
                {
                    if (replaced.Split("(")[0].Split("_").Contains(searchTerm))
                    {
                        filtered.Add(t);
                    }
                }
            }

            return filtered;
        }

        public static List<string> CompileSuggestions(List<TagData> tagData)
        {
            List<string> pages = new List<string>();
            StringBuilder sb = new StringBuilder();
            string suggestion;

            if (tagData.Count > 0)
            {
                sb.Append("```");

                foreach (TagData td in tagData)
                {
                    suggestion = $@"<{td.TagID}> {td.TagName} ({td.NumLinks})" + Environment.NewLine;

                    if ((sb.ToString() + suggestion).Length > MaxDescriptionSize)
                    {
                        sb.Append("```");
                        pages.Add(sb.ToString());

                        sb = new StringBuilder("```");

                        sb.Append(suggestion);

                        continue;
                    }
                    else
                    {
                        sb.Append(suggestion);
                    }
                }

                sb.Append("```");

                pages.Add(sb.ToString());
            }

            return pages;
        }

        public static string EscapeApostrophe(string tag)
        {
            string tagEscaped;

            if (tag.Contains("'"))
            {
                tagEscaped = tag.Insert(tag.IndexOf("'"), "'");
            }
            else
            {
                tagEscaped = tag;
            }

            return tagEscaped;
        }

        public static string EscapeUnderscore(string tag)
        {
            string tagEscaped;

            if (tag.Contains("_"))
            {
                tagEscaped = tag.Replace("_", "\\_");
            }
            else
            {
                tagEscaped = tag;
            }

            return tagEscaped;
        }

        public static string Format(string tag)
        {
            return tag.Replace("\\", string.Empty).ToLower();
        }
    }
}