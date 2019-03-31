using JetBrains.Annotations;
using Tauron.Application.ImageOrganizer.Core;

namespace Tauron.Application.ImageOrganizer.BL
{
    public class AppSettings : TauronProfile, ISettings
    {
        public AppSettings([NotNull] string application, [NotNull] string defaultPath) : base(application, defaultPath) { }

        public string CurrentDatabase
        {
            get => GetValue(string.Empty);
            set => SetVaue(value);
        }

        public int PageCount
        {
            get => GetValue(20);
            set => SetVaue(value);
        }

    }
}