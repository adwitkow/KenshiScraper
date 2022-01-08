using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;

namespace KenshiScraper
{
    class KenshiBlogScraper : AbstractScraper
    {
        private static readonly string UPDATE_BLOG_URL = "https://lofigames.com/category/updates/";
        private static readonly string[] SELECTORS_TO_CLEAN = { "script", "div.shareit" };

        public override async Task<IEnumerable<Article>> ScrapeAsync(IEnumerable<Article> articles)
        {
            var results = await ScrapeRecursivelyAsync(UPDATE_BLOG_URL, articles);

            return results.ToArray();
        }

        private async Task<List<Article>> ScrapeRecursivelyAsync(string url, IEnumerable<Article> articles)
        {
            List<Article> results;
            if (articles == null)
            {
                results = new List<Article>();
            }
            else
            {
                results = new List<Article>(articles);
            }

            string nextUrl = string.Empty;

            await Initialize();

            await page.GoToAsync(url);

            var container = await page.QuerySelectorAsync("#content_box");
            var nextButton = await page.QuerySelectorAsync(".nav-previous > a");
            if (nextButton != null)
            {
                nextUrl = await ExtractPropertyAsync(nextButton, "href");
            }

            var newResults = new List<Article>();
            foreach (var content in await container.QuerySelectorAllAsync("article"))
            {
                var article = new Article();

                var anchor = ExtractPropertyAsync(content, "a.reply", "href");
                var author = ExtractPropertyAsync(content, "span.theauthor > a", "textContent");
                var timestampText = ExtractPropertyAsync(content, "span.thetime.updated", "textContent");
                var title = ExtractPropertyAsync(content, "h1 > a", "textContent");

                await Task.WhenAll(anchor, author, timestampText, title);

                var timestamp = DateTime.ParseExact(timestampText.Result, "Po\\s\\te\\d On MMMM d, yyyy",
                                       System.Globalization.CultureInfo.InvariantCulture);

                article.Title = title.Result.Trim();
                article.Author = author.Result;
                article.Date = timestamp;
                article.Url = anchor.Result;
                article.Source = "Blog";

                newResults.Add(article);
            }

            foreach (var article in newResults)
            {
                var tries = 0;
                var success = false;
                while (tries < 3 && !success)
                {
                    try
                    {
                        await page.GoToAsync(article.Url);
                        success = true;
                    }
                    catch
                    {
                        tries++;
                    }
                }
                var body = await page.QuerySelectorAsync("div.post-single-content");
                await CleanBlogBody(body);

                string bodyContent = await ExtractPropertyAsync(body, "innerHTML");
                bodyContent = bodyContent.PrettifyHtml();
                bodyContent = HttpUtility.HtmlDecode(bodyContent);

                article.HtmlBody = bodyContent;
            }

            results.AddRange(newResults);

            if (!string.IsNullOrEmpty(nextUrl))
            {
                results = await ScrapeRecursivelyAsync(nextUrl, results);
            }

            return results;
        }

        private async Task CleanBlogBody(ElementHandle element)
        {
            foreach (var selector in SELECTORS_TO_CLEAN)
            {
                var elements = await element.QuerySelectorAllAsync(selector);
                var elementsCount = elements.Length;
                for (int i = 0; i < elementsCount; i++)
                {
                    await elements[i].EvaluateFunctionAsync(@"
(element) => {
    element.parentNode.removeChild(element);
}", elements[i]);
                }
            }
        }
    }
}
