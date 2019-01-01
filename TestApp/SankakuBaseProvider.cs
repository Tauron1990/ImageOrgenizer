using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using ExCSS;
using HtmlAgilityPack;
using JetBrains.Annotations;
using NLog;

namespace Tauron.Application.ImageOrganizer.BL.Provider.Impl
{
    [PublicAPI]
    public sealed class SankakuBaseProvider
    {
        private const string UserAgentString = "user-agent";
        private const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:61.0) Gecko/20100101 Firefox/61.0)";

        private Logger _logger = LogManager.GetCurrentClassLogger();
        
        private HtmlDocument _currentDocument;
        private string _url;
        
        
        public void Load(string url)
        {
            _statsNode = null;
            _url = url;
            _currentDocument = new HtmlDocument();
            _currentDocument.LoadHtml(url);
        }
        public bool CanRead()
        {
            var ele = _currentDocument.DocumentNode.Element("html").Element("body");
            var h1 = ele.Element("h1");

            if (h1 == null) return true;

            return !h1.InnerText.StartsWith("Error 429");
        }

        public bool IsValidFile(string file)
        {
            string name = file.GetFileNameWithoutExtension();

            return int.TryParse(name, out _);
        }

        private HtmlNode _statsNode;

        [DebuggerStepThrough]
        private HtmlNode GetStats() => _statsNode ?? (_statsNode = EnumerateNotes().First(StatsPredicate));

        [DebuggerStepThrough]
        private bool StatsPredicate(HtmlNode n) => n.Id == "stats";

        public string GetDownloadUrl()
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