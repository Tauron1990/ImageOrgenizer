using System.Linq;
using Tauron.Application.Common.BaseLayer.Data;
using Tauron.Application.ImageOrganizer.Data.Entities;

namespace Tauron.Application.ImageOrganizer.Data.Repositories
{
    public class ProfileRepository : Repository<ProfileEntity, int>, IProfileRepository
    {
        public ProfileRepository(IDatabase database) : base(database) { }

        public ProfileEntity[] GetProfileData() => QueryAsNoTracking().ToArray();

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
                Add(data);
        }

        public void Remove(string name)
        {
            var profile = Query().FirstOrDefault(pe => pe.Name == name);

            if (profile == null) return;

            Remove(profile);
        }

        public ProfileEntity GetEntity(string name) => Query().First(pe => pe.Name == name);
    }
}