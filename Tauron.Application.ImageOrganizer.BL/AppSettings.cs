using System;
using JetBrains.Annotations;
using Tauron.Application.ImageOrganizer.Core;

namespace Tauron.Application.ImageOrganizer.BL
{
    public class AppSettings : TauronProfile, ISettings
    {
        public AppSettings([NotNull] string application, [NotNull] string defaultPath) : base(application, defaultPath) { }

        public string CurrentDatabase
        {
            get => GetValue(nameof(CurrentDatabase), string.Empty);
            set => SetVaue(nameof(CurrentDatabase), value);
        }

        public int PageCount
        {
            get
            {
                if (int.TryParse(GetValue(nameof(PageCount), "20"), out var intResult)) return intResult;
                return 20;
            }
            set => SetVaue(nameof(PageCount), value);
        }

    }
}