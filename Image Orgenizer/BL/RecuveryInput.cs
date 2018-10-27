using System;
using ImageOrganizer.Data.Container;

namespace ImageOrganizer.BL
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