using System;
using System.Windows.Input;
using ImageOrganizer.BL;
using Tauron.Application.Commands;

namespace ImageOrganizer.Views
{
    public class TagFilterElement
    {
        public TagFilterElement(TagElement tag, Action<TagElement> click)
        {
            Tag = tag;
            Click = new SimpleCommand(null, o => click?.Invoke(Tag));
        }

        protected TagFilterElement() { }

        public TagElement Tag { get; }

        public ICommand Click { get; }
    }
}