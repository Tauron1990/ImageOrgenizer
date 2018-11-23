using JetBrains.Annotations;

namespace Tauron.Application.ImageOrganizer.Core
{
    public interface ISettingsManager
    {
        [CanBeNull]
        ISettings Settings { get; }

        void Load(string name);
        void Save();
    }
}