using System;
using System.Collections.Generic;
using System.IO;
using ImageOrganizer.Data.Container;
using ImageOrganizer.Data.Container.Compse;
using Tauron;

namespace ImageOrganizer.BL.Operations
{

    public static class FileContainerManager
    {
        public static IContainerFile ContainerFile { get; private set; }

        public static ContainerType CurrentContainerType { get; private set; }

        public static Stream GetFile(string name) => ContainerFile.Conatins(name) ? ContainerFile.GetStream(name) : null;

        public static void Switch(string name, ContainerType containerType, string custom)
        {
            CurrentContainerType = containerType;
            switch (CurrentContainerType)
            {
                case ContainerType.Compose:
                    ContainerFile = Factory.Begin()
                        .UseCompose()
                        .AddSingle()
                        .AddMulti(custom)
                        .Initialize(name);
                    break;
                case ContainerType.Single:
                    ContainerFile = Factory.Begin().UseSingle().Initialize(name);
                    break;
                case ContainerType.Multi:
                    ContainerFile = Factory.Begin().UseMulti().Initialize(name);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public  static IContainerTransaction GetContainerTransaction() => ContainerFile.CreateTransaction();

        public static bool CanAdd(string file, Func<string, string> getFileName) => !ContainerFile.Conatins(getFileName(file));

        public static bool AddFile(byte[] file, string name)
        {
            var trans = GetContainerTransaction();
            try
            {
                ContainerFile.AddFile(file, name, trans);
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
                ContainerFile.AddFiles(filesToAdd, nameSelector, controller);

                return true;
            }
            catch (Exception e)
            {
                if (CriticalExceptions.IsCriticalApplicationException(e))
                    throw;

                return false;
            }
        }

        public static void Remove(string name, IContainerTransaction containerTransaction) => ContainerFile.Remove(name, containerTransaction);

        public static void Defrag(IEnumerable<string> expectedContent, Action<(string Name, ErrorType Error)> onErrorFound, Action<string> onMessage,
            IContainerTransaction transaction)
            => ContainerFile.Sync(expectedContent, onErrorFound, onMessage, transaction);

        public static void Recuvery(string target, Action<RecuveryMessage> message) => ContainerFile.Recuvery(target, message);

        public static void Import(IContainerFile file, IContainerTransaction transaction, Action<string, int, int> onPostMessage)
        {
            string[] all = file.GetAllContentNames();
            int count = 0;
            foreach (var name in all)
            {
                ContainerFile.AddFile(ComposeFile.ReadAll(GetFile(name)), name, transaction);
                count++;
                onPostMessage(name, count, all.Length);
            }
        }

        public static string[] GetContainerNames() => ContainerFile.GetContainerNames();

        public static long ComputeSize() => ContainerFile.ComputeSize();
    }
}