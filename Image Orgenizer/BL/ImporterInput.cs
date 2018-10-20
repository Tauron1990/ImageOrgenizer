using System;

namespace ImageOrganizer.BL
{
    public class ImporterInput
    {
        public event Action Pause;
        public event Action Stop;

        public event Action<string, int, int, bool> PostMessage;

        public string FileLocation { get; }
        public string Provider { get; }

        public ImporterInput(string fileLocation, string provider)
        {
            FileLocation = fileLocation;
            Provider = provider;
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