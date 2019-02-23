using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Tauron.Application.ImageOrganizer.Container;
using Tauron.Application.ImageOrganizer.Container.Compse;

namespace Tauron.Application.ImageOrganizer.BL.Operations
{

    public static class FileContainerManager
    {
        private static ReaderWriterLockSlim _containerLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public static IContainerFile ContainerFile { get; set; }

        public static ContainerType CurrentContainerType { get; private set; }

        public static IDisposable EnterLock()
        {
            _containerLock.EnterWriteLock();
            return new ActionDispose(_containerLock.ExitWriteLock);
        }

        public static Stream GetFile(string name)
        {
            _containerLock.EnterReadLock();
            try
            {
                return ContainerFile.Conatins(name) ? ContainerFile.GetStream(name) : null;
            }
            finally
            {
                _containerLock.ExitReadLock();
            }
        }

        public static void Switch(string name, ContainerType containerType, string custom)
        {
            _containerLock.EnterWriteLock();
            try
            {
                CurrentContainerType = containerType;
                switch (CurrentContainerType)
                {
                    case ContainerType.Compose:
                        ContainerFile = ContainerFactory.Begin()
                            .UseCompose()
                            .AddSingle()
                            .AddMulti(custom)
                            .Initialize(name);
                        break;
                    case ContainerType.Single:
                        ContainerFile = ContainerFactory.Begin().UseSingle().Initialize(name);
                        break;
                    case ContainerType.Multi:
                        ContainerFile = ContainerFactory.Begin().UseMulti().Initialize(name);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            finally
            {
                _containerLock.ExitWriteLock();
            }
        }

        public  static IContainerTransaction GetContainerTransaction() => ContainerFile.CreateTransaction();

        public static bool CanAdd(string file, Func<string, string> getFileName)
        {
            _containerLock.EnterReadLock();
            try
            {
                return !ContainerFile.Conatins(getFileName(file));
            }
            finally
            {
                _containerLock.ExitReadLock();
            }
        }

        public static bool AddFile(byte[] file, string name, IContainerTransaction transaction = null)
        {
            _containerLock.EnterWriteLock();
            var trans = transaction ?? GetContainerTransaction();
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
                _containerLock.ExitWriteLock();
            }
        }

        public static bool Save(string[] filesToAdd, Func<string, string> nameSelector, Controller controller)
        {
            _containerLock.EnterWriteLock();
            try
            {
                ContainerFile.AddFiles(filesToAdd, nameSelector, controller);

                return true;
            }
            catch (Exception e)
            {
                if (e.IsCriticalApplicationException())
                    throw;

                return false;
            }
            finally
            {
                _containerLock.ExitWriteLock();
            }
        }

        public static void Remove(string name, IContainerTransaction containerTransaction)
        {
            _containerLock.EnterWriteLock();
            try
            {
                ContainerFile.Remove(name, containerTransaction);
            }
            finally
            {
                _containerLock.ExitWriteLock();
            }
        }

        public static void Defrag(IEnumerable<string> expectedContent, Action<(string Name, ErrorType Error)> onErrorFound, Action<string> onMessage,
            IContainerTransaction transaction)
        {
            _containerLock.EnterWriteLock();
            try
            {
                ContainerFile.Sync(expectedContent, onErrorFound, onMessage, transaction);
            }
            finally
            {
                _containerLock.ExitWriteLock();
            }
        }

        public static void Recuvery(string target, Action<RecuveryMessage> message) => ContainerFile.Recuvery(target, message);

        public static void Import(IContainerFile file, IContainerTransaction transaction, Action<string, int, int> onPostMessage)
        {
            _containerLock.EnterWriteLock();
            try
            {
                string[] all = file.GetAllContentNames();
                int count = 0;
                foreach (var name in all)
                {
                    try
                    {
                        ContainerFile.AddFile(ComposeFile.ReadAll(file.GetStream(name)), name, transaction);
                        count++;
                        onPostMessage(name, count, all.Length);
                    }
                    catch (Exception e)
                    {
                        if (e.IsCriticalApplicationException())
                            throw;
                    }
                }
            }
            finally
            {
                _containerLock.ExitWriteLock();
            }
        }

        public static bool Contains(string name)
        {
            _containerLock.EnterReadLock();
            try
            {
                return ContainerFile.Conatins(name);
            }
            finally
            {
                _containerLock.ExitReadLock();
            }
        }

        //public static string[] GetContainerNames() => ContainerFile.GetContainerNames();

        public static long ComputeSize()
        {
            _containerLock.EnterReadLock();
            try
            {
                return ContainerFile.ComputeSize();
            }
            finally
            {
                _containerLock.ExitReadLock();
            }
        }
    }
}