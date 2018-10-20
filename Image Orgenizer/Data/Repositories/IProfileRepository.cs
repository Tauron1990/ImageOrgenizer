using System.Linq;
using ImageOrganizer.Data.Entities;
using Tauron.Application.Common.BaseLayer.Data;

namespace ImageOrganizer.Data.Repositories
{
    public interface IProfileRepository
    {
        ProfileEntity[] GetProfileData();
        void Save(ProfileEntity data);
        void Remove(string name);
        ProfileEntity GetEntity(string name);
    }

    public class ProfileRepository : Repository<ProfileEntity, int>, IProfileRepository
    {
        public ProfileRepository(IDatabase database) : base(database)
        {
        }

        public ProfileEntity[] GetProfileData()
        {
            return QueryAsNoTracking().ToArray();
        }

        public void Save(ProfileEntity data)
        {
            var profile = Query().FirstOrDefault(pe => pe.Name == data.Name);
            if (profile != null)
            {
                profile.CurrentPosition = data.CurrentPosition;
                profile.FilterString = data.FilterString;
                profile.NextImage = data.NextImage;
            }
            else
            {
                Add(data);
            }
        }

        public void Remove(string name)
        {
            var profile = Query().FirstOrDefault(pe => pe.Name == name);

            if (profile == null) return;

            Remove(profile);
        }

        public ProfileEntity GetEntity(string name)
        {
            return Query().First(pe => pe.Name == name);
        }
    }
}