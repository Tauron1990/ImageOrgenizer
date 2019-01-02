using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using ExCSS;
using HtmlAgilityPack;
using JetBrains.Annotations;
using NLog;
using Tauron.Application.ImageOrganizer.BL.Provider.Browser;
using Tauron.Application.ImageOrganizer.BL.Provider.DownloadImpl;

namespace Tauron.Application.ImageOrganizer.BL.Provider.Impl
{
    [PublicAPI]
    public sealed class SankakuBaseProvider
    {
        //private const string UserAgentString = "user-agent";
        //private const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:61.0) Gecko/20100101 Firefox/61.0)";

        private Logger _logger = LogManager.GetCurrentClassLogger();

        //private readonly WebClient _webClient;
        //private readonly HtmlWeb _htmlWeb;
        private HtmlDocument _currentDocument = new HtmlDocument();
        private string _url;
        private IBrowserHelper _browser;
        private Func<byte[]> _dataFunc;

        //public SankakuBaseProvider()
        //{
        //    _webClient = new WebClient();
        //    _htmlWeb = new HtmlWeb();
        //}

        public void LoadPost(string name) => Load($"https://chan.sankakucomplex.com/post/show/{name}");

        public void Load(string url)
        {
            _statsNode = null;
            _url = url;
            _browser.Load(url);
            _currentDocument.LoadHtml(_browser.GetSource());
        }

        public string GetName()
        {
            string extension = Path.GetExtension(new Uri(GetDownloadUrl()).LocalPath);
            string name = new Uri(_url).LocalPath.Split('/').Last();

            return name + extension;
        }

        public byte[] DownloadImage() => _dataFunc();

        public IEnumerable<(string Type, string Name)> GetTags() => EnumeradeTags().Select(htmlNode => (htmlNode.GetAttributeValue("class", string.Empty), htmlNode.Element("a").InnerText));

        public bool CanRead(out bool delay)
        {
            var ele = _currentDocument.DocumentNode.Element("html").Element("body");
            var h1 = ele.Element("h1");

            if (h1 == null)
            {
                delay = false;
                return true;
            }

            delay = _currentDocument.DocumentNode.InnerHtml.Contains("429") || _currentDocument.DocumentNode.InnerHtml.Contains("502");
            return false;
        }

        public DateTime GetDateAdded()
        {
            var stats = GetStats();
            var targetElement = stats.Element("ul").Elements("li").First().Elements("a").First();

            return DateTime.Parse(targetElement.GetAttributeValue("title", string.Empty));
        }

        public string GetAuthor()
        {
            try
            {
                var stats = GetStats();
                var targetElement = stats.Element("ul").Elements("li").First().Elements("a").ElementAt(1);

                return targetElement.InnerText;
            }
            catch(Exception e)
            {
                _logger.Error(e, "Sankaku GetAuthor Exception");

                return string.Empty;
            }
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

        public void LoadWiki(string name)
        {
            string tagReplace = name.Replace(' ', '_');
            tagReplace = WebUtility.UrlEncode(tagReplace);
            Load($"https://chan.sankakucomplex.com/wiki/show?title={tagReplace}");
        }

        public string GetTagDescription(string tag, out bool ok)
        {
            try
            {
                var temp = EnumerateNotes(_currentDocument).First(n => n.Id == "body");
                ok = true;
                var desc = temp.Elements("div").First().InnerText;

                StringBuilder builder = new StringBuilder(desc.Length);
                bool filtermode = false;

                for (int i = 0; i < desc.Length; i++)
                {
                    char curr = desc[i];
                    if (filtermode)
                    {
                        if (curr == '>')
                        {
                            filtermode = false;
                            continue;
                        }
                    }

                    if (curr == '<' && desc[i + 1] == '/' && desc[i + 2] == 'a' || curr == '<' && desc[i + 1] == 'a')
                    {
                        if(desc[i + 1] == 'h')
                            break;
                        filtermode = true;
                        continue;
                    }

                    builder.Append(curr);
                }

                return builder.ToString();
            }
            catch (Exception e)
            {
                ok = false;
                _logger.Error(e, "Sankaku GetTagColor Exception");
                return string.Empty;
            }
        }

        public string GetCssUrl() => 
            _currentDocument.DocumentNode.Element("html").Element("head")
            .Elements("link").First(n => n.GetAttributeValue("rel", string.Empty) == "stylesheet").GetAttributeValue("href", string.Empty);

        private string _lastColorUrl;

        public string GetTagColor(string tag, string url, out bool ok)
        {
            try
            {
                if(_lastColorUrl != url)
                {
                    if (url.StartsWith("//"))
                        url = "https:" + url;
                    _browser.Load(url);
                    _lastColorUrl = url;
                }

                StylesheetParser parser = new StylesheetParser();
                var result = parser.Parse(_browser.GetSource());

                var rule = result.Children.OfType<IStyleRule>().FirstOrDefault(r => r.SelectorText.Contains(tag) && !r.SelectorText.Contains("a:hover"));
                if (rule == null)
                {
                    ok = true;
                    return "black";
                }
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

                ok = true;
                return color == null ? "black" : $"#{color.Value.A:X2}{color.Value.R:X2}{color.Value.G:X2}{color.Value.B:X2}";
            }
            catch(Exception e)
            {
                _logger.Error(e, "Sankaku GetTagColor Exception");

                ok = false;
                return DownloadEntry.FormatException(e);
            }
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

        private HtmlNode _statsNode;

        [DebuggerStepThrough]
        private HtmlNode GetStats() => _statsNode ?? (_statsNode = EnumerateNotes().First(StatsPredicate));

        [DebuggerStepThrough]
        private bool StatsPredicate(HtmlNode n) => n.Id == "stats";

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

        public void Init(IBrowserHelper browser, Func<byte[]> dataFunc)
        {
            _browser = browser;
            _dataFunc = dataFunc;
            _lastColorUrl = null;
        }
    }
}