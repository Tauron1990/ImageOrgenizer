using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Tauron.Application.ImageOrginazer.ViewModels.Views.ImageEditorHelper
{
    [PublicAPI]
    public interface IEditorItem<TRawData> : IEditorItem, IEquatable<IEditorItem<TRawData>>, IEquatable<TRawData>
        where TRawData : IEquatable<TRawData>
    {
        TRawData Create();
        void Update(Task<TRawData> data);
    }

    public interface IEditorItem
    {
        event Action<IEditorItem> ChangedEvent;

        bool IsNew { get; }
    }
}