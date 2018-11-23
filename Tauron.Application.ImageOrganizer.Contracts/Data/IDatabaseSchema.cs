using JetBrains.Annotations;

namespace Tauron.Application.ImageOrganizer.Data
{
    public interface IDatabaseSchema
    {
        void Update([CanBeNull] string path);
    }
}