using System;
using Tauron.Application.ImageOrganizer.Container;

namespace Tauron.Application.ImageOrganizer.BL
{
    public class RecuveryInput
    {
        public string ExportPath { get; }
        public Action<RecuveryMessage> Report { get; }

        public RecuveryInput(string exportPath, Action<RecuveryMessage> report)
        {
            ExportPath = exportPath;
            Report = report;
        }
    }
}