using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KenshiScraper
{
    abstract class AbstractScraper : IDisposable
    {
        protected Browser browser;
        protected Page page;

        public async Task DownloadChromium()
        {
            var fetcher = new BrowserFetcher();
            await fetcher.DownloadAsync(BrowserFetcher.DefaultRevision);
        }

        public abstract Task<IEnumerable<Article>> ScrapeAsync(IEnumerable<Article> articles);

        protected async Task Initialize()
        {
            if (browser == null || page == null)
            {
                browser = await Puppeteer.LaunchAsync(new LaunchOptions() { Headless = false });
                var pages = await browser.PagesAsync();
                page = pages[0];
            }
        }

        protected async Task<string> ExtractPropertyAsync(ElementHandle handle, string nodeProperty)
        {
            return await handle.EvaluateFunctionAsync<string>($"t => t.{nodeProperty}");
        }

        protected async Task<string> ExtractPropertyAsync(ElementHandle handle, string cssSelector, string nodeProperty)
        {
            var queried = await handle.QuerySelectorAsync(cssSelector);
            return await ExtractPropertyAsync(queried, nodeProperty);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                page.Dispose();
                browser.Dispose();

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~AbstractScraper()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
