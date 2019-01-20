using System;

namespace Tauron.Application.ImageOrganizer.BL.Provider.Browser
{
    public interface IBrowserHelper
    {
        string GetSource();
        bool Load(string url);
        void RegisterInterceptor(string name, Func<IDataInterceptor> interceptor);
        void Clear();
    }
}