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

        public string BuildSuggestions(List<string> tags, string searchTerm)
        {
            StringBuilder sb = new StringBuilder();
            string suggestion;
            string[] parts;
            int i = 1;

            foreach (string n in tags)
            {
                suggestion = $@"{i}. {n}" + Environment.NewLine;

                if ((sb.ToString() + suggestion).Length > MaxDescriptionSize)
                {
                    break;
                }

                if (searchTerm.Contains("_"))
                {
                    if (n.Split("(")[0].Contains(searchTerm))
                    {
                        sb.Append(suggestion);

                        i++;
                    }
                }
                else
                {
                    parts = n.Split("(")[0].Split("_");

                    if (n.Split("(")[0].Split("_").Contains(searchTerm))
                    {
                        sb.Append(suggestion);

                        i++;
                    }
                }
            }

            if (i > 1)
            {
                sb.Insert(0, "```");
                sb.Append("```");
            }

            return sb.ToString();
        }
    }
}
