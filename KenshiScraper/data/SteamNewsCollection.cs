using System.Collections.Generic;
using System.Xml.Serialization;

namespace KenshiScraper
{
    [XmlRoot("appnews")]
    public class SteamNewsCollection
    {
        [XmlElement("appid")]
        public int AppId { get; set; }
        [XmlArray("newsitems")]
        [XmlArrayItem("newsitem")]
        public List<SteamNewsItem> Items { get; set; }
    }

    public class SteamNewsItem
    {
        [XmlElement("title")]
        public string Title { get; set; }
        [XmlElement("url")]
        public string Url { get; set; }
        [XmlElement("author")]
        public string Author { get; set; }
        [XmlElement("contents")]
        public string Contents { get; set; }
        [XmlElement("feedlabel")]
        public string FeedLabel { get; set; }
        [XmlElement("date")]
        public long Date { get; set; }
        [XmlElement("feedname")]
        public string FeedName { get; set; }
        [XmlElement("feed_type")]
        public int FeedType { get; set; }
        [XmlElement("appid")]
        public int AppId { get; set; }
        [XmlArray("tags")]
        [XmlArrayItem("tag")]
        public List<string> Tags { get; set; }
    }
}
