using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Serialization;

namespace KenshiScraper
{
    class SteamScraper : AbstractScraper
    {
        public override async Task<IEnumerable<Article>> ScrapeAsync(IEnumerable<Article> articles)
        {
            var m_strFilePath = "http://api.steampowered.com/ISteamNews/GetNewsForApp/v0002/?appid=233860&count=500&maxlength=100&format=xml";
            string xmlStr;
            using (var wc = new WebClient())
            {
                xmlStr = await wc.DownloadStringTaskAsync(m_strFilePath);
            }

            XmlSerializer ser = new XmlSerializer(typeof(SteamNewsCollection));

            SteamNewsCollection news = null;
            using (var reader = new StringReader(xmlStr))
            {
                news = (SteamNewsCollection)ser.Deserialize(reader);
            }

            await Initialize();

            List<Article> results;
            if (articles == null)
            {
                results = new List<Article>();
            }
            else
            {
                results = new List<Article>(articles);
            }

            foreach (var item in news.Items)
            {
                if (item.FeedType == 0)
                {
                    Console.WriteLine($"{item.Url}: {item.FeedType}");
                    continue;
                }

                Page[] pages = await browser.PagesAsync();
                Page page = pages[0];

                await page.GoToAsync(item.Url); // NavigationException
                var viewButton = await page.QuerySelectorAsync("div.partnereventshared_Button_1ABCO");
                if (viewButton != null)
                {
                    await viewButton.ClickAsync();
                }

                string body = await page.WaitForSelectorAsync(".EventDetailsBody").EvaluateFunctionAsync<string>("t => t.innerHTML");
                body = body.PrettifyHtml();
                body = HttpUtility.HtmlDecode(body);

                results.Add(new Article()
                {
                    Title = item.Title,
                    Author = item.Author,
                    Date = DateTimeOffset.FromUnixTimeSeconds(item.Date).DateTime,
                    Url = item.Url,
                    HtmlBody = body,
                    Source = "Steam"
                });
            }

            return results.ToArray();
        }
    }
}
