namespace Tauron.Application.ImageOrganizer.BL.Provider
{
    public interface IViewFetcher
    {
        string Name { get; }

        string Id { get; }

        bool IsValidLastValue(ref string value);

        FetcherResult GetNext(string next, string last);
    }
}