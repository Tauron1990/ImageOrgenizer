using System.IO;
using Tauron.Application.ImageOrganizer.Core;

namespace Tauron.Application.ImageOrganizer.BL.Operations
{
    public static class BackUpDatabase
    {
        private static string GetDbFile() => CommonApplication.Current.Container.Resolve<ISettingsManager>().Settings?.CurrentDatabase;

        public static void MakeBackup()
        {
            try
            {
                var dbFile = GetDbFile();
                dbFile.CopyFileTo(dbFile + ".bak");
            }
            catch (IOException)
            {
            }
        }

        public static void Revert()
        {
            var dbFile = GetDbFile();
            dbFile.CopyFileTo(dbFile.Remove(dbFile.Length - 3, 3));
        }
    }
}