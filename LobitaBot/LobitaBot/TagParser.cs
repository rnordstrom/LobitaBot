using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LobitaBot
{
    public class TagParser
    {
        public const int MaxDescriptionSize = 2048;

        public string BuildTitle(string searchTerm)
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

        public List<string> FilterSuggestions(List<string> tags, string searchTerm)
        {
            List<string> filtered = new List<string>();
            string[] parts;

            foreach (string t in tags)
            {
                if (searchTerm.Contains("_"))
                {
                    if (t.Split("(")[0].Contains(searchTerm))
                    {
                        filtered.Add(t);
                    }
                }
                else
                {
                    parts = t.Split("(")[0].Split("_");

                    if (t.Split("(")[0].Split("_").Contains(searchTerm))
                    {
                        filtered.Add(t);
                    }
                }
            }

            return filtered;
        }

        public string CompileSuggestions(List<TagData> tagData)
        {
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
                        break;
                    }
                    else
                    {
                        sb.Append(suggestion);
                    }
                }

                sb.Append("```");
            }

            return sb.ToString();
        }
    }
}
