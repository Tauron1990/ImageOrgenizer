using System.Collections.Generic;

namespace Tauron.Application.ImageOrganizer.UI
{
    public interface IRecordDeletedArgs
    {
        IEnumerable<object> Items { get; }
    }
}