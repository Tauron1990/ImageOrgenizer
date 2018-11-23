using System;
using System.Windows.Input;
using Tauron.Application.Commands;
using Tauron.Application.ImageOrganizer.BL;

namespace Tauron.Application.ImageOrganizer.UI
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