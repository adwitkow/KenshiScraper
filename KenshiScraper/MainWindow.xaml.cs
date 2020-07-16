using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Serialization;

namespace KenshiScraper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Dictionary<Button, AbstractScraper> scraperMap;

        public MainWindow()
        {
            InitializeComponent();

            scraperMap = new Dictionary<Button, AbstractScraper>()
            {
                [BlogScraperButton] = new KenshiBlogScraper(),
                [SteamScraperButton] = new SteamScraper()
            };
        }

        private async void ScraperButton_Click(object sender, RoutedEventArgs e)
        {
            var button = e.Source as Button;
            button.IsEnabled = false;

            IEnumerable<Article> articles = (IEnumerable<Article>)MainListView.ItemsSource;

            var scraper = scraperMap[button]; // TODO: Guard against key not available in dictionary
            articles = await scraper.ScrapeAsync(articles);

            articles = articles.OrderBy(article => article.Date);

            MainListView.ItemsSource = articles;

            button.IsEnabled = true;
        }

        private void MainListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // TODO: Move this to XAML EventSetter once it's fixed in VS update...
            var listView = e.Source as ListView;

            if (!(listView.SelectedItem is Article item))
            {
                return;
            }

            var converter = new MediaWikiConverter();

            PreviewTextBox.Text = converter.ConvertFromHtml(item.HtmlBody).Trim();
            UrlTextBox.Text = item.Url.Trim();
            TitleTextBox.Text = item.Title.Trim();
            AuthorTextBox.Text = item.Author.Trim();
            PlatformTextBox.Text = item.Source.Trim();
            DateTextBox.Text = item.Date.ToString("MMMM d, yyyy");
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainListView.ItemsSource == null)
            {
                return;
            }

            var directory = System.AppDomain.CurrentDomain.BaseDirectory;
            var path = Path.Combine(directory, "save.xml");

            List<Article> articleList;
            var articles = (IEnumerable<Article>)MainListView.ItemsSource;

            if (articles is List<Article>)
            {
                articleList = articles as List<Article>;
            }
            else
            {
                articleList = articles.ToList();
            }

            var serializer = new XmlSerializer(typeof(List<Article>));
            using var stream = File.OpenWrite(path);
            serializer.Serialize(stream, articleList);
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            var directory = System.AppDomain.CurrentDomain.BaseDirectory;
            var path = Path.Combine(directory, "save.xml");

            var serializer = new XmlSerializer(typeof(List<Article>));
            using var stream = File.OpenRead(path);
            var result = (List<Article>)serializer.Deserialize(stream);
            MainListView.ItemsSource = result;
        }
    }
}
