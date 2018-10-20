using System;
using System.IO;
using System.Runtime.CompilerServices;
using Tauron;

namespace ImageOrganizer.BL.Operations
{
    public static class BackUpDatabase
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string GetDbFile() => Properties.Settings.Default.CurrentDatabase;

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