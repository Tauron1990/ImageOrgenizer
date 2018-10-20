using System;
using System.Collections.Generic;
using System.IO;
using ImageOrganizer.Data.Container;
using Tauron;

namespace ImageOrganizer.BL.Operations
{

    public static class FileContainerManager
    {
        private static IContainerFile _containerFile;

        public static Stream GetFile(string name) => _containerFile.Conatins(name) ? _containerFile.GetStream(name) : null;

        public static void Switch(string name, ISettings settings)
        {
            switch (settings.ContainerType)
            {
                case ContainerType.Compose:
                    string custom = settings.CustomMultiPath;
                    _containerFile = Factory.Begin()
                        .UseCompose()
                        .AddSingle()
                        .AddMulti(custom)
                        .Initialize(name);
                    break;
                case ContainerType.Single:
                    _containerFile = Factory.Begin().UseSingle().Initialize(name);
                    break;
                case ContainerType.Multi:
                    _containerFile = Factory.Begin().UseMulti().Initialize(name);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public  static IContainerTransaction GetContainerTransaction() => _containerFile.CreateTransaction();

        public static bool CanAdd(string file, Func<string, string> getFileName) => !_containerFile.Conatins(getFileName(file));

        public static bool AddFile(byte[] file, string name)
        {
            var trans = GetContainerTransaction();
            try
            {
                _containerFile.AddFile(file, name, trans);
                trans.Commit();
                return true;
            }
            catch
            {
                trans.Rollback();
                return false;
            }
            finally
            {
                trans.Dispose();
            }
        }

        public static bool Save(string[] filesToAdd, Func<string, string> nameSelector, Controller controller)
        {
            try
            {
                _containerFile.AddFiles(filesToAdd, nameSelector, controller);

                return true;
            }
            catch (Exception e)
            {
                if (CriticalExceptions.IsCriticalApplicationException(e))
                    throw;

                return false;
            }
        }

        public static void Remove(string name, IContainerTransaction containerTransaction) => _containerFile.Remove(name, containerTransaction);

        public static void Defrag(IEnumerable<string> expectedContent, Action<(string Name, ErrorType Error)> onErrorFound, Action<string> onMessage,
            IContainerTransaction transaction)
            => _containerFile.Sync(expectedContent, onErrorFound, onMessage, transaction);

        public static void Recuvery(string target, Action<RecuveryMessage> message) => _containerFile.Recuvery(target, message);
    }
}