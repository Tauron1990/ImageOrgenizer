using System;

namespace Tauron.Application.ImageOrganizer.BL.Provider
{
    public sealed class ProviderLockChangeEventArgs : EventArgs
    {
        public string Name { get; }

        public DateTime TargetTime { get; }

        public bool IsLocked { get; }

        public ProviderLockChangeEventArgs(string name, DateTime targetTime, bool isLocked)
        {
            Name = name;
            TargetTime = targetTime;
            IsLocked = isLocked;
        }
    }
}