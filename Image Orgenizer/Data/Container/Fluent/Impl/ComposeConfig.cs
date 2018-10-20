using System;
using System.Collections.Generic;
using System.Linq;
using ImageOrganizer.Data.Container.Compse;

namespace ImageOrganizer.Data.Container.Fluent.Impl
{
    public class ComposeConfig : IComposeConfig
    {
        private readonly Dictionary<string, Func<(IComposerExpose Container, string Custom)>> _factorys = new Dictionary<string, Func<(IComposerExpose Container, string Custom)>>();

        public IContainerFile Initialize(string name)
        {
            if(_factorys.Count == 0) throw new InvalidOperationException("No Container Added");

            var temp = new ComposeFile();
            foreach (var ele in _factorys.Select(cfb => cfb.Value()))
                temp.Add(ele.Container, ele.Custom);

            temp.Initialize(name);
            return temp;
        }

        public void Add(string name, Func<(IComposerExpose Container, string Custom)> func) => _factorys[name] = func;
    }
}