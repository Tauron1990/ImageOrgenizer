namespace Tauron.Application.ImageOrganizer.BL.Provider.Browser
{
    public interface IDataInterceptor
    {
        bool Active { get; }

        bool Intercept(string url);

        void Data(byte[] data, string url);

        bool LoadRequest(string url);
    }
}