using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KenshiScraper
{
    class MediaWikiConverter
    {
        private readonly Dictionary<string, MediaWikiNode> nodeMap;

        public MediaWikiConverter()
        {
            nodeMap = new Dictionary<string, MediaWikiNode>()
            {
                ["#text"] = new MediaWikiNode(),
                ["p"] = new MediaWikiNode(),
                ["span"] = new MediaWikiNode(),
                ["ul"] = new MediaWikiNode(),
                ["ol"] = new MediaWikiNode(),
                ["blockquote"] = new MediaWikiNode(),
                ["div"] = new MediaWikiNode(),
                ["li"] = new MediaWikiNode()
                {
                    Prefix = "* ",
                    Suffix = Environment.NewLine
                },
                ["h1"] = new MediaWikiNode()
                {
                    Prefix = "==",
                    Suffix = "=="
                },
                ["h2"] = new MediaWikiNode()
                {
                    Prefix = "===",
                    Suffix = "==="
                },
                ["h3"] = new MediaWikiNode()
                {
                    Prefix = "====",
                    Suffix = "===="
                },
                ["h4"] = new MediaWikiNode()
                {
                    Prefix = "=====",
                    Suffix = "====="
                },
                ["h5"] = new MediaWikiNode()
                {
                    Prefix = "======",
                    Suffix = "======"
                },
                ["strong"] = new MediaWikiNode()
                {
                    Prefix = "'''",
                    Suffix = "'''"
                },
                ["b"] = new MediaWikiNode()
                {
                    Prefix = "'''",
                    Suffix = "'''"
                },
                ["em"] = new MediaWikiNode()
                {
                    Prefix = "''",
                    Suffix = "''"
                },
                ["i"] = new MediaWikiNode()
                {
                    Prefix = "''",
                    Suffix = "''"
                },
                ["img"] = new MediaWikiNode()
                {
                    Prefix = "[[File:",
                    Suffix = "]]",
                    AttributeValue = "src"
                },
                ["a"] = new MediaWikiNode()
                {
                    Prefix = "[",
                    Suffix = "]",
                    AttributeValue = "href"
                }
            };
        }

        public string ConvertFromHtml(string html)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            if (htmlDoc == null)
            {
                return html;
            }

            StringBuilder converted = new StringBuilder();

            foreach (var node in htmlDoc.DocumentNode.ChildNodes)
            {
                if (string.IsNullOrWhiteSpace(node.InnerText))
                {
                    continue;
                }
                converted = AppendNode(converted, node);
                converted.Append(Environment.NewLine);
            }

            return converted.ToString();
        }

        private StringBuilder AppendNode(StringBuilder stringBuilder, HtmlNode node)
        {
            // check parameter
            if (stringBuilder == null) return stringBuilder;
            if (node == null) return stringBuilder;
            if (node.Name.Equals("br")) return stringBuilder;

            var valueFound = nodeMap.TryGetValue(node.Name, out MediaWikiNode mwNode);
            if (!valueFound)
            {
                stringBuilder.Append(node.OuterHtml);
                return stringBuilder;
            }

            string attribute = node.GetAttributeValue(mwNode.AttributeValue, string.Empty);

            if (!node.HasChildNodes)
            {
                string text = node.InnerText;
                if (string.IsNullOrWhiteSpace(text))
                {
                    return stringBuilder;
                }

                if (mwNode.AttributeValue != string.Empty)
                {
                    Console.WriteLine("sztop");
                }
                stringBuilder.Append(mwNode.Prefix);

                if (!string.IsNullOrEmpty(attribute))
                {
                    stringBuilder.Append(attribute.Trim()).Append(" ");
                }

                var lines = text.Split("\n").ToList();
                for (int i = lines.Count - 1; i >= 0; i--)
                {
                    var line = lines[i];
                    if (string.IsNullOrWhiteSpace(lines[i]))
                    {
                        lines.Remove(line);
                    }
                    else
                    {
                        lines[i] = line.Trim();
                    }
                }

                stringBuilder.Append(string.Join(Environment.NewLine, lines));
                stringBuilder.Append(mwNode.Suffix);
            }
            else
            {
                stringBuilder.Append(mwNode.Prefix);
                if (!string.IsNullOrEmpty(attribute))
                {
                    stringBuilder.Append(attribute.Trim()).Append(" ");
                }

                foreach (var chldNode in node.ChildNodes)
                {
                    AppendNode(stringBuilder, chldNode);
                }

                stringBuilder.Append(mwNode.Suffix);
            }

            return stringBuilder;
        }
    }
}
