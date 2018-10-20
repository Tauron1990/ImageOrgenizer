using System;
using System.IO;
using System.IO.Compression;
using System.Windows;
using LZ4;
using Tauron.Application.Views;

namespace ImageOrganizer.Views
{
    /// <summary>
    /// Interaktionslogik für DownloadManagerView.xaml
    /// </summary>
    [ExportView(AppConststands.DownloadManager)]
    public partial class DownloadManagerView
    {
        public DownloadManagerView()
        {
            InitializeComponent();
        }

        private void DownloadManagerView_OnLoaded(object sender, RoutedEventArgs e)
        {
            var model = (DownloadManagerViewModel) DataContext;

            model.GetPersistentStade += ModelOnGetPersistentStade;
            ReadPersistendStade(model.PersistentStade);
        }

        private string ModelOnGetPersistentStade()
        {
            if (!Dispatcher.CheckAccess())
                return Dispatcher.Invoke(ModelOnGetPersistentStade);

            using (MemoryStream stream = new MemoryStream())
            {
                using (var compressor = new LZ4Stream(stream, LZ4StreamMode.Compress, LZ4StreamFlags.HighCompression))
                {
                    ItemsGrid.Serialize(compressor);
                }

                return Convert.ToBase64String(stream.GetBuffer());
            }
        }

        private void ReadPersistendStade(string base64)
        {
            try
            {
                using (var stream = new MemoryStream(Convert.FromBase64String(base64)))
                {
                    using (var decompressor = new LZ4Stream(stream, CompressionMode.Decompress, LZ4StreamFlags.HighCompression))
                    {
                        ItemsGrid.Deserialize(decompressor);
                    }
                }
            }
            catch
            {
                //Ignored
            }
        }
    }
}
