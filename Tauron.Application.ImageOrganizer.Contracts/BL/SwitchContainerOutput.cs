namespace Tauron.Application.ImageOrganizer.BL
{
    public class SwitchContainerOutput
    {
        public bool NeedSync { get; }

        public bool Sucssed { get; }

        public string Error { get; }

        public SwitchContainerOutput(bool needSync, bool sucssed, string error)
        {
            NeedSync = needSync;
            Sucssed = sucssed;
            Error = error;
        }
    }
}