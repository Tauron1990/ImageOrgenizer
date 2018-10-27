using System;

namespace ImageOrganizer.BL
{
    public class SwitchContainerInput
    {
        private string _customPath;

        public string CustomPath => _customPath ?? string.Empty;

        public ContainerType ContainerType { get; }

        public event Action<string, int, int> Message;

        public SwitchContainerInput(string customPath, ContainerType containerType)
        {
            _customPath = customPath;
            ContainerType = containerType;
        }

        public void ClearCustom() => _customPath = string.Empty;

        public void OnMessage(string message, int value, int maximum)
        {
            Message?.Invoke(message, value, maximum);
        }
    }
}