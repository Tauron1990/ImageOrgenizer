using Tauron.Application.Ioc;

namespace Tauron.Application.ImageOrganizer.Data
{
    [Export(typeof(IDatabaseSchema))]
    public sealed class DatabaseSchema : IDatabaseSchema
    {
        public void Update(string path) => DatabaseImpl.UpdateSchema(path);
    }
}