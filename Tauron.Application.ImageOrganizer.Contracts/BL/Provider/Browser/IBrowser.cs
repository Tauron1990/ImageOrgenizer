using System;

namespace Tauron.Application.ImageOrganizer.BL.Provider.Browser
{
    public interface IBrowserHelper
    {
        string GetSource();
        byte[] GetData();

        bool Load(string url);
        void RegisterInterceptor(string name, Func<IDataInterceptor> interceptor);
        void Clear();

        Exception CurrentError { get; }
    }
}