using System.Collections.Generic;

namespace Tauron.Application.ImageOrganizer.UI
{
    public interface IRowValidatingArgs
    {
        int RowIndex { get; }
        object RowData { get; }
        Dictionary<string, string> ErrorMessages { get; }
        bool IsValid { get; set; }
    }
}