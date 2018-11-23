using Tauron.Application.ImageOrginazer.ViewModels.Resources;

namespace Tauron.Application.ImageOrginazer.ViewModels.Views
{
    public class FileImporterProviderItem
    {
        public FileImporterProviderItem(string id) => Id = id;

        public string Id { get; }

        public string Name => UIResources.ResourceManager.GetString(Id) ?? Id;
    }
}