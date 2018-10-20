using System;

namespace ImageOrganizer.Views.ImageEditorHelper
{
    public class InsertCheckEventArgs<TEditorItem> : EventArgs
    {
        public bool OverrideAdd { get; set; }

        public TEditorItem EditorItem { get; }

        public InsertCheckEventArgs(TEditorItem item) => EditorItem = item;
    }
}