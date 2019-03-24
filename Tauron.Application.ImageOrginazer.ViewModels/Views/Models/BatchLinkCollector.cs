using System;
using System.IO;
using NLog;

namespace Tauron.Application.ImageOrginazer.ViewModels.Views.Models
{
    public class BatchLinkCollector : IFetcherLinkCollector
    {
        private const string FileName = "ImageOrganizerLinkList.txt";

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly StreamWriter _writer;
        private readonly string _path;
        private readonly IClipboardManager _clipboardManager;

        public BatchLinkCollector(IClipboardManager clipboardManager)
        {
            _clipboardManager = clipboardManager;
            _path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).CombinePath(FileName);
            _writer = new StreamWriter(new FileStream(_path, FileMode.Create, FileAccess.Write));
        }

        public void FetchCompled()
        {
            _writer.Flush();
            _writer.Dispose();

            while (true)
            {
                int errorCount = 0;
                try
                {
                    _clipboardManager.SetValue(_path);
                    break;
                }
                catch (Exception e)
                {
                    if (errorCount == 10)
                    {
                        Logger.Error(e);
                        break;
                    }
                    // ReSharper disable once RedundantAssignment
                    errorCount++;
                }
            }
        }

        public void AddLink(string link) 
            => _writer.Write(link);
    }
}