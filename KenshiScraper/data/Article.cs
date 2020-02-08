using System;

namespace KenshiScraper
{
    public class Article
    {
        public string Title { get; set; }
        public string Source { get; set; }
        public string Author { get; set; }
        public DateTime Date { get; set; }
        public string Url { get; set; }
        public string HtmlBody { get; set; }
    }
}
