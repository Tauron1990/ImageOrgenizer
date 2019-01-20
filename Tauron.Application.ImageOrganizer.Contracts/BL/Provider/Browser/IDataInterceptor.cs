using System.Net;

namespace Tauron.Application.ImageOrganizer.BL.Provider.Browser
{
    //public interface IDataInterceptor
    //{
    //    bool Active { get; }

    //    bool Intercept(string url);

    //    void Data(byte[] data, string url);

    //    bool LoadRequest(string url);
    //}

    public interface IDataInterceptor
    {
        bool CanProcess(string url);

        void FeddData(byte[] data, string url);

        void Clear();

    }
}