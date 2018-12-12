using System;
using System.IO;
using System.Windows;
using K4os.Compression.LZ4;
using K4os.Compression.LZ4.Streams;
using Tauron.Application.ImageOrginazer.ViewModels.Views;
using Tauron.Application.Views;

namespace Tauron.Application.ImageOrganizer.Views
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
                using (var compressor = LZ4Stream.Encode(stream, LZ4Level.L12_MAX))
                    ItemsGrid.Serialize(compressor);

                return Convert.ToBase64String(stream.GetBuffer());
            }
        }

        private void ReadPersistendStade(string base64)
        {
            try
            {
                using (var stream = new MemoryStream(Convert.FromBase64String(base64)))
                    using (var decompressor = LZ4Stream.Decode(stream, 0))
                        ItemsGrid.Deserialize(decompressor);
            }
            catch
            {
                //Ignored
            }
        }
    }
}
