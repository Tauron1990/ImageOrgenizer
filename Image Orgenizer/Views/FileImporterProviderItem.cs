using ImageOrganizer.Resources;

namespace ImageOrganizer.Views
{
    public class FileImporterProviderItem
    {
        public FileImporterProviderItem(string id)
        {
            Id = id;
        }

        public string Id { get; }

        public string Name => UIResources.ResourceManager.GetString(Id) ?? Id;
    }
}