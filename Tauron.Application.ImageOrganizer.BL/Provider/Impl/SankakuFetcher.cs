using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using Tauron.Application.ImageOrganizer.BL.Provider.Browser;
using Tauron.Application.Ioc;

namespace Tauron.Application.ImageOrganizer.BL.Provider.Impl
{
    [Export(typeof(IViewFetcher))]
    public sealed class SankakuFetcher : IViewFetcher
    {
        private IBrowserHelper _browserHelper;

        [Inject]
        public IBrowserManager BrowserManager { private get; set; }

        private IBrowserHelper BrowserHelper => _browserHelper ?? (_browserHelper = BrowserManager.GetBrowser());

        public string Name { get; } = Resources.BuissinesLayerResources.SankakuFetcher_Name;

        public string Id => "SankakuFetcher";

        public bool IsValidLastValue(ref string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;

            if (!value.StartsWith("p"))
                value = "p" + value;

            return value.Skip(1).All(char.IsDigit);
        }

        public FetcherResult GetNext(string next, string last)
        {
            try
            {
                const string urlTemplate = @"https://chan.sankakucomplex.com/?next={0}&tags=threshold%3A0&page=3";
                const string urlStartTemplate = @"https://chan.sankakucomplex.com/?tags=threshold%3A0&page=2";
                string targetUrl = string.IsNullOrWhiteSpace(next)
                    ? urlStartTemplate
                    : string.Format(urlTemplate, next);

                if (!BrowserHelper.Load(targetUrl))
                {
                    string errorMessage = BrowserHelper.CurrentError != null
                        ? BrowserHelper.CurrentError.Message
                        : Resources.BuissinesLayerResources.SankakuFetcher_CommonError;
                    return new FetcherResult(null, false, errorMessage, true, null, null, false);
                }

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(BrowserHelper.GetSource());

                var element = doc.GetElementbyId("post-list").Elements("div").ElementAt(2).Element("div");
                var paginator = doc.GetElementbyId("paginator");

                string nextPostUrl =
                    paginator.Element("div").GetAttributeValue("next-page-url", null) ??
                    paginator.Element("div").Elements("a").ElementAt(1).GetAttributeValue("href", null);
                string nextPost = nextPostUrl
                    .Split('&')[0]
                    .Split(new[] {'='}, StringSplitOptions.RemoveEmptyEntries)[1];

                string lastPost = null;
                bool lastArrived = false;

                List<FetcherImage> fetcherImages = new List<FetcherImage>();

                foreach (var postSpan in element.Elements("span"))
                {
                    lastArrived = ExtractPostNumber(last) >=
                                  ExtractPostNumber(postSpan.GetAttributeValue("id", string.Empty));
                    if (string.IsNullOrEmpty(lastPost))
                        lastPost = postSpan.GetAttributeValue("id", string.Empty);

                    var a = postSpan.Element("a");
                    var img = a.Element("img");

                    if (!BrowserHelper.Load("https:" + img.GetAttributeValue("src", string.Empty)))
                    {
                        string errorMessage = BrowserHelper.CurrentError != null
                            ? BrowserHelper.CurrentError.Message
                            : Resources.BuissinesLayerResources.SankakuFetcher_CommonError;
                        return new FetcherResult(null, false, errorMessage, true, null, null, false);
                    }

                    FetcherImage fImg = new FetcherImage
                    {
                        Info = img.GetAttributeValue("title", string.Empty),
                        Link = "https://chan.sankakucomplex.com" + a.GetAttributeValue("href", string.Empty),
                        Image = BrowserHelper.GetData()
                    };

                    fetcherImages.Add(fImg);
                }

                return new FetcherResult(fetcherImages, true, string.Empty, false, nextPost, lastPost, lastArrived);
            }
            catch (Exception e)
            {
                return new FetcherResult(null, false, e.Message, true, null, null, false);
            }
            finally
            {
                BrowserHelper.Clear();
            }
        }

        private int ExtractPostNumber(string input)
        {
            if (input.ToLower().StartsWith("p"))
                input = input.Remove(0, 1);

            return int.Parse(input);
        }
    }
}