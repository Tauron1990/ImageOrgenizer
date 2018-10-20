using System;
using System.Threading.Tasks;

namespace ImageOrganizer.Views.ImageEditorHelper
{
    public interface IOperationManager<TRawData, TEditorItem>
        where TEditorItem : IEditorItem<TRawData>, new() where TRawData : IEquatable<TRawData>
    {
        TEditorItem CreatEditorItem(TRawData rawData);
        Task<TRawData> SendToDatabase(TEditorItem item);
        Task<TRawData> FetchFromDatabase(TEditorItem item);
        Task<bool> RemoveFromDatabase(TEditorItem item);
    }
}