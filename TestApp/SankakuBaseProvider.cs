using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text;
using ExCSS;
using HtmlAgilityPack;
using JetBrains.Annotations;
using Tauron;
using  UIColor = System.Windows.Media.Color;

namespace ImageOrganizer.BL.Provider.Impl
{
    [PublicAPI]
    public sealed class SankakuBaseProvider
    {
        private class InternalWebClient : WebClient
        {
            private const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.77 Safari/537.36";
            
            private readonly CookieContainer _cookieContainer = new CookieContainer(1000, 1000, short.MaxValue);

            protected override WebRequest GetWebRequest(Uri address)
            {
                var request = base.GetWebRequest(address);

                if (!(request is HttpWebRequest httpWebRequest)) return request;

                httpWebRequest.Method = "GET";
                httpWebRequest.UserAgent = UserAgent;
                httpWebRequest.CookieContainer = _cookieContainer;
                httpWebRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
                httpWebRequest.ContentType = "application/x-www-form-urlencoded";

                return request;
            }

            protected override WebResponse GetWebResponse(WebRequest request)
            {
                var response = base.GetWebResponse(request);

                if (!(response is HttpWebResponse httpWebResponse)) return response;

                foreach (Cookie cookie in httpWebResponse.Cookies)
                    _cookieContainer.Add(cookie);

                return response;
            }
        }

        private readonly WebClient _webClient;
        private HtmlDocument _currentDocument;
        private string _url;

        public SankakuBaseProvider()
        {
            _webClient = new InternalWebClient();
        }

        public void LoadPost(string name) => Load($"https://chan.sankakucomplex.com/post/show/{name}");

        public void Load(string url)
        {
            _url = url;
            _currentDocument = new HtmlDocument();
            _currentDocument.Load(new StringReader(_webClient.DownloadString(url)));
        }

        public string GetName()
        {
            string extension = Path.GetExtension(new Uri(GetDownloadUrl()).LocalPath);
            string name = new Uri(_url).LocalPath.Split('/').Last();

            return name + extension;
        }

        public byte[] DownloadImage()
        {
            return _webClient.DownloadData(GetDownloadUrl());
        }

        public IEnumerable<(string Type, string Name)> GetTags() => EnumeradeTags().Select(htmlNode => (htmlNode.GetAttributeValue("class", string.Empty), htmlNode.Element("a").InnerText));

        public bool CanRead()
        {
            var ele = _currentDocument.DocumentNode.Element("html").Element("body");
            var h1 = ele.Element("h1");

            if (h1 == null) return true;

            return !h1.InnerText.StartsWith("Error 429");
        }

        public DateTime GetDateAdded()
        {
            var stats = GetStats();
            var targetElement = stats.Element("ul").Elements("li").First().Elements("a").First();

            return DateTime.Parse(targetElement.GetAttributeValue("title", string.Empty));
        }

        public string GetAuthor()
        {
            var stats = GetStats();
            var targetElement = stats.Element("ul").Elements("li").First().Elements("a").ElementAt(1);

            return targetElement.InnerText;
        }

        public long GetSize()
        {
            var stats = GetStats();
            var targetElement = stats.Element("ul").Elements("li").First(e => e.InnerHtml.Contains("id=\"highres\"")).Element("a");
            string toParse = targetElement.GetAttributeValue("title", "0");

            StringBuilder filterd = new StringBuilder();

            foreach (var c in toParse)
            {
                if (char.IsDigit(c))
                    filterd.Append(c);
                else if (c == ',')
                    // ReSharper disable once RedundantJumpStatement
                    continue;
                else
                    break;
            }

            return long.Parse(filterd.ToString());
        }

        public string GetTagDescription(string tag)
        {
            string tagReplace = tag.Replace(' ', '_');
            tagReplace = WebUtility.UrlEncode(tagReplace);

            var doc = new HtmlDocument();
            doc.Load(new StringReader(_webClient.DownloadString($"https://chan.sankakucomplex.com/wiki/show?title={tagReplace}")));
            var temp = EnumerateNotes(doc).First(n => n.Id == "body");
            return temp.Elements("div").First().Element("p").InnerText;
        }

        public string GetTagColor(string tag)
        {
            string url = _currentDocument.DocumentNode.Element("html").Element("head")
                .Elements("link").First(n => n.GetAttributeValue("rel", string.Empty) == "stylesheet").GetAttributeValue("href", string.Empty);
            StylesheetParser parser = new StylesheetParser();
            var result = parser.Parse(_webClient.DownloadString(url));

            var rule = result.Children.OfType<IStyleRule>().FirstOrDefault(r => r.SelectorText.Contains(tag));
            if (rule == null) return null;
            var colorText = rule.Style.Color.Trim();
            Color? color;

            if (string.IsNullOrWhiteSpace(colorText))
                color = null;
            else if (colorText.StartsWith("#"))
                color = Color.FromHex(colorText);
            else if(colorText.StartsWith("rgb"))
                color = ParseRgb(colorText);
            else if (colorText.StartsWith("rgba"))
                color = ParseRgba(colorText);
            else if (colorText.StartsWith("hsl"))
                color = ParseHsl(colorText);
            else if (colorText.StartsWith("hsla"))
                color = ParseHsla(colorText);
            else
                color = null;

            return color == null ? null : UIColor.FromArgb(color.Value.A, color.Value.R, color.Value.G, color.Value.B).ToString();
        }

        public bool IsValidFile(string file)
        {
            string name = file.GetFileNameWithoutExtension();

            return int.TryParse(name, out _);
        }

        private Color ParseHsla(string txt)
        {
            string[] comp = GetColorComponents(txt);
            return Color.FromHsla(float.Parse(comp[0]), float.Parse(comp[1].Replace('%', ' ')), float.Parse(comp[2].Replace('%', ' ')), float.Parse(comp[3]));
        }

        private Color ParseHsl(string txt)
        {
            string[] comp = GetColorComponents(txt);
            return Color.FromHsl(float.Parse(comp[0]), float.Parse(comp[1].Replace('%', ' ')), float.Parse(comp[2].Replace('%', ' ')));
        }

        private Color ParseRgba(string txt)
        {
            var comp = GetColorComponents(txt);

            return Color.FromRgba(byte.Parse(comp[0]), byte.Parse(comp[1]), byte.Parse(comp[2]), float.Parse(comp[3]));
        }

        private Color ParseRgb(string txt)
        {
            var comp = GetColorComponents(txt);

            return Color.FromRgb(byte.Parse(comp[0]), byte.Parse(comp[1]), byte.Parse(comp[2]));
        }

        private string[] GetColorComponents(string txt) => txt.Split('(', ')')[1].Split(',').Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()).ToArray();

        private IEnumerable<HtmlNode> EnumeradeTags() => EnumerateNotes().First(n => n.Id == "tag-sidebar").Elements("li");

        private HtmlNode GetStats() => EnumerateNotes().First(n => n.Id == "stats");

        private string GetDownloadUrl()
        {
            var stats = GetStats();
            var target = stats.ChildNodes[3].ChildNodes[3].Element("a");
            string targetUri = target.GetAttributeValue("href", "");
            if (!targetUri.StartsWith("http"))
                targetUri = @"https:" + targetUri;
            return targetUri;
        }

        [DebuggerStepThrough]
        private static IEnumerable<HtmlNode> EnumerateNotes(HtmlDocument doc)
        {
            Stack<HtmlNode> nodes = new Stack<HtmlNode>();

            nodes.Push(doc.DocumentNode);

            do
            {
                var node = nodes.Pop();

                foreach (var childNode in node.ChildNodes)
                    nodes.Push(childNode);

                yield return node;
            } while (nodes.Count != 0);
        }

        [DebuggerStepThrough]
        private IEnumerable<HtmlNode> EnumerateNotes() => EnumerateNotes(_currentDocument);
    }
}