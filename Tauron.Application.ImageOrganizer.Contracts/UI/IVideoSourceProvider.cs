using System;

namespace Tauron.Application.ImageOrganizer.UI
{
    public interface IVideoSourceProvider
    {
        Video.IVideoSourceProvider VideoSource { get; set; }

        event Action LockEvent;

        event Action<IVideoSourceProvider> UnlockEvent;
    }
}