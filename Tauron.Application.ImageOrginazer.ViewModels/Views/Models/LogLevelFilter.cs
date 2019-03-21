using System;

namespace Tauron.Application.ImageOrginazer.ViewModels.Views.Models
{
    [Flags]
    public enum LogLevelFilter
    {
        None = 0,
        Trance = 1,
        Info = 2,
        Warn = 4,
        Debug = 8,
        Error = 16
    }
}