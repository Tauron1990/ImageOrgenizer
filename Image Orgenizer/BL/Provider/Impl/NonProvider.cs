using ImageOrganizer.Data.Entities;
using Tauron;
using Tauron.Application.Ioc;

namespace ImageOrganizer.BL.Provider.Impl
{
    [Export(typeof(IProvider))]
    public class NonProvider : IProvider
    {
        public const string ProviderNon = "ProviderNon";

        public string Id { get; } = nameof(ProviderNon);

        [Inject]
        public Operator Operator { get; set; }

        public bool IsValid(string file) => true;
        public bool IsValidUrl(string url) => false;

        public bool FillInfo(ImageData image, DownloadType downloadItemDownloadType, out bool ok)
        {
            if (downloadItemDownloadType == DownloadType.DownloadImage)
            {
                TryAddFile(image, out ok);
                return false;
            }

            ok = true;
            return false;
        }

        private void TryAddFile(ImageData image, out bool ok)
        {
            if (image.Name.ExisFile())
            {
                string name = image.Name.GetFileName();

                Operator.AddFile(new AddFileInput(name, image.Name.ReadAllBytes()));
                image.Name = name;

                ok = true;
                return;
            }

            ok = false;
        }
    }
}