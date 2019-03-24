namespace Tauron.Application.ImageOrginazer.ViewModels.Views.Models
{
    public interface IFetcherLinkCollector
    {
        void AddLink(string link);
        void FetchCompled();
    }
}