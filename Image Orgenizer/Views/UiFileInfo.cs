using System.IO;
using JetBrains.Annotations;

namespace ImageOrganizer.Views
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