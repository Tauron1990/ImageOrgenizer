using System;
using Tauron.Application.ImageOrganizer.Container;

namespace Tauron.Application.ImageOrganizer.BL
{
    public class DefragInput
    {
        public Action<(string Name, ErrorType Error)> OnErrorFound { get; }
        public Action<string> OnMessage { get; }

        public DefragInput(Action<(string Name, ErrorType Error)> onErrorFound, Action<string> onMessage)
        {
            OnErrorFound = onErrorFound;
            OnMessage = onMessage;
        }
    }
}