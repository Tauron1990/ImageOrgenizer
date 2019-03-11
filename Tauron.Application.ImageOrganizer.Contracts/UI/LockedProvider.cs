using System;

namespace Tauron.Application.ImageOrganizer.UI
{
    public sealed class LockedProvider
    {
        public string Name { get; }

        public string Date { get; }

        public LockedProvider(string name, DateTime date)
        {
            Name = name;
            Date = date.ToShortTimeString();
        }
    }
}