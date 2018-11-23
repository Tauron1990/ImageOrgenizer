using System;
using JetBrains.Annotations;

namespace Tauron.Application.ImageOrganizer.Container
{
    [PublicAPI]
    public class Controller
    {
        public event Action Pause;
        public event Action Stop;

        public event Action<string, int, int, bool> PostMessage;

        public IContainerTransaction Transaction { get; }
        
        public Controller(IContainerTransaction transaction = null)
        {
            Transaction = transaction;
        }

        public void OnPause()
        {
            Pause?.Invoke();
        }

        public void OnStop()
        {
            Stop?.Invoke();
        }

        public void OnPostMessage(string message, int minimum, int maximum, bool intermidiate)
        {
            PostMessage?.Invoke(message, minimum, maximum, intermidiate);
        }
    }
}