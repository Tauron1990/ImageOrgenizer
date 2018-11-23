using System.IO;
using JetBrains.Annotations;

namespace Tauron.Application.ImageOrginazer.ViewModels.Views
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public class UiFileInfo
    {
        public string Size { get; }

        public string Name { get; }

        public string FullName { get; }

        public string CreationTime { get; }

        public UiFileInfo(FileInfo info)
        {
            CreationTime = info.CreationTime.ToString("G");
            FullName = info.FullName;
            Name = info.Name;
            Size = $"{info.Length / 1024} Kb";
        }
    }
}