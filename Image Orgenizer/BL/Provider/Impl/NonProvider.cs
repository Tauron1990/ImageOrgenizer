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


        public bool IsValid(string file) => true;
        public bool IsValidUrl(string url) => false;

        public bool FillInfo(ImageData image, DownloadType downloadItemDownloadType, Operator op, out bool ok)
        {
            if (downloadItemDownloadType == DownloadType.DownloadImage)
            {
                TryAddFile(image, op, out ok);
                return false;
            }

            ok = true;
            return false;
        }

        private void TryAddFile(ImageData image, Operator op, out bool ok)
        {
            if (image.Name.ExisFile())
            {
                string name = image.Name.GetFileName();

                ok = op.AddFile(new AddFileInput(name, image.Name.ReadAllBytes())).Result;
                image.Name = name;
                return;
            }

            ok = false;
        }
    }
}