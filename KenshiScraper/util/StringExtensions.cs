// Shamelessly stolen from https://github.com/philippfx/TreeFormatter/.
// Apologies, but the nuget package had way too many dependencies for comfort.

using HtmlAgilityPack;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace KenshiScraper
{
    /// <summary>
    /// Defines the <see cref="StringExtensions" />
    /// </summary>
    public static class StringExtensions
    {
        private static readonly string INDENT = "  ";
        private static readonly string NEW_LINE = Environment.NewLine;

        /// <summary>
        /// Minifies an html string
        /// </summary>
        /// <param name="source">source html <see cref="string"/></param>
        /// <returns>Minified html <see cref="string"/></returns>
        public static string MinifyHtml(this string source)
        {
            var redundantHtmlWhitespace = new Regex(@"(?<=>)\s+?(?=<)");
            return redundantHtmlWhitespace.Replace(source, String.Empty).Trim();
        }

        /// <summary>
        /// Prettifies an html string
        /// </summary>
        /// <param name="source">source html <see cref="string"/></param>
        /// <returns>Prettified html <see cref="string"/></returns>
        public static string PrettifyHtml(this string source)
        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(source);

            var stringBuilder = new StringBuilder();

            if (htmlDocument.DocumentNode != null)
            {
                foreach (var node in htmlDocument.DocumentNode.ChildNodes)
                {
                    stringBuilder = AppendNode(stringBuilder, node, 0);
                }
            }

            return stringBuilder.ToString();
        }

        public static string StripHtml(this string value)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(value);

            if (htmlDoc == null)
            {
                return value;
            }

            StringBuilder sanitizedString = new StringBuilder();

            foreach (var node in htmlDoc.DocumentNode.ChildNodes)
            {
                sanitizedString.Append(node.InnerText);
            }

            return sanitizedString.ToString();
        }

        /// <summary>
        /// Recursively appends html nodes
        /// </summary>
        /// <param name="stringBuilder">The stringBuilder<see cref="StringBuilder"/></param>
        /// <param name="node">The node<see cref="HtmlNode"/></param>
        /// <param name="indentLevel">The indentLevel<see cref="int"/></param>
        /// <returns>The <see cref="StringBuilder"/></returns>
        private static StringBuilder AppendNode(StringBuilder stringBuilder, HtmlNode node, int indentLevel)
        {
            // check parameter
            if (stringBuilder == null) return stringBuilder;
            if (node == null) return stringBuilder;
            if (node.Name.Equals("br")) return stringBuilder;

            if (!node.HasChildNodes)
            {
                for (int i = 0; i < indentLevel; i++)
                {
                    stringBuilder.Append(INDENT);
                }
                stringBuilder.Append(node.OuterHtml);
                stringBuilder.Append(NEW_LINE);
            }
            else
            {
                // indent
                for (int i = 0; i < indentLevel; i++)
                {
                    stringBuilder.Append(INDENT);
                }

                // open tag
                stringBuilder.Append(string.Format("<{0}", node.Name));
                if (node.HasAttributes)
                {
                    foreach (var attr in node.Attributes)
                    {
                        stringBuilder.Append(string.Format(" {0}=\"{1}\" ", attr.Name, attr.Value));
                    }
                }

                stringBuilder.Append(string.Format(">{0}", NEW_LINE));

                // childs
                foreach (var chldNode in node.ChildNodes)
                {
                    AppendNode(stringBuilder, chldNode, indentLevel + 1);
                }

                // close tag
                for (int i = 0; i < indentLevel; i++)
                {
                    stringBuilder.Append(INDENT);
                }
                stringBuilder.Append(string.Format("</{0}>{1}", node.Name, NEW_LINE));
            }

            return stringBuilder;
        }
    }
}
