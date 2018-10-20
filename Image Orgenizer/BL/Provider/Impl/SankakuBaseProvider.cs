using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using ExCSS;
using HtmlAgilityPack;
using JetBrains.Annotations;
using Syncfusion.Windows.Shared;
using Tauron;
using  UIColor = System.Windows.Media.Color;

namespace ImageOrganizer.BL.Provider.Impl
{
    [PublicAPI]
    public sealed class SankakuBaseProvider
    {
        private const string UserAgentString = "user-agent";
        private const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:61.0) Gecko/20100101 Firefox/61.0)";

        private readonly WebClient _webClient;
        private readonly HtmlWeb _htmlWeb;
        private HtmlDocument _currentDocument;
        private string _url;

        public SankakuBaseProvider()
        {
            _webClient = new WebClient();
            _htmlWeb = new HtmlWeb();
        }

        public void LoadPost(string name) => Load($"https://chan.sankakucomplex.com/post/show/{name}");

        public void Load(string url)
        {
            _url = url;
            _currentDocument = _htmlWeb.Load(url);
        }

        public string GetName()
        {
            string extension = Path.GetExtension(new Uri(GetDownloadUrl()).LocalPath);
            string name = new Uri(_url).LocalPath.Split('/').Last();

            return name + extension;
        }

        public byte[] DownloadImage()
        {
            return PrepareClient().DownloadData(GetDownloadUrl());
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

        public string GetAutor()
        {
            var stats = GetStats();
            var targetElement = stats.Element("ul").Elements("li").First().Elements("a").ElementAt(1);

            return targetElement.InnerText;
        }

        public long GetSize()
        {
            var stats = GetStats();
            var targetElement = stats.Element("ul").Elements("li").ElementAt(1).Element("a");
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

            var doc = _htmlWeb.Load($"https://chan.sankakucomplex.com/wiki/show?title={tagReplace}");
            var temp = EnumerateNotes(doc).First(n => n.Id == "body");
            return temp.Elements("div").First().Element("p").InnerText;
        }

        public string GetTagColor(string tag)
        {
            string url = _currentDocument.DocumentNode.Element("html").Element("head")
                .Elements("link").First(n => n.GetAttributeValue("rel", string.Empty) == "stylesheet").GetAttributeValue("href", string.Empty);
            StylesheetParser parser = new StylesheetParser();
            var result = parser.Parse(PrepareClient().DownloadString(url));

            var rule = result.Children.OfType<IStyleRule>().FirstOrDefault(r => r.SelectorText.Contains(tag));
            if (rule == null) return null;
            var colorText = rule.Style.Color.Trim();
            Color? color;

            if (colorText.IsNullOrWhiteSpace())
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

        private WebClient PrepareClient()
        {
            _webClient.Headers.Add(UserAgentString, UserAgent);
            return _webClient;
        }

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

        private IEnumerable<HtmlNode> EnumerateNotes() => EnumerateNotes(_currentDocument);
    }
}