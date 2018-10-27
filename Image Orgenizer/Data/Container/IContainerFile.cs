using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace ImageOrganizer.Data.Container
{
    [PublicAPI]
    public interface IContainerFile
    {
        void Initialize(string name);
        void AddFile(string file, string name, IContainerTransaction transaction);
        void AddFile(byte[] file, string name, IContainerTransaction transaction);
        void AddFiles(string[] files, Func<string, string> nameSelector, Controller controller = null);
        Stream GetStream(string name);
        bool Conatins(string name);
        void Remove(string name, IContainerTransaction transaction);
        IContainerTransaction CreateTransaction();
        void Sync(IEnumerable<string> expectedContent, Action<(string Name, ErrorType Error)> onErrorFound, Action<string> onMessage, IContainerTransaction transaction);
        void Recuvery(string targetDirectory, Action<RecuveryMessage> onMessage);
        string[] GetContainerNames();
        string[] GetAllContentNames();
        long ComputeSize();
    }
}