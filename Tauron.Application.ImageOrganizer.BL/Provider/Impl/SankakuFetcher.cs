namespace Tauron.Application.ImageOrganizer.BL.Provider.Impl
{
    public sealed class SankakuFetcher : IViewFetcher
    {
        public string Name { get; }

        public string Id { get; } = "SankakuFetcher";

        public bool IsValidLastValue(ref string value)
        {
            
        }

        public FetcherResult GetNext(string next, string last)
        {

        }
    }
}