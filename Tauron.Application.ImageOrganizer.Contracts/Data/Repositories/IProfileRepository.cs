using Tauron.Application.ImageOrganizer.Data.Entities;

namespace Tauron.Application.ImageOrganizer.Data.Repositories
{
    public interface IProfileRepository
    {
        ProfileEntity[] GetProfileData();
        void Save(ProfileEntity data);
        void Remove(string name);
        ProfileEntity GetEntity(string name);
    }
}