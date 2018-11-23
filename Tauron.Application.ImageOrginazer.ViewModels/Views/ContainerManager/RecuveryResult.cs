using Tauron.Application.ImageOrganizer.Container;

namespace Tauron.Application.ImageOrginazer.ViewModels.Views.ContainerManager
{
    public class RecuveryResult : UiBase
    {
        private readonly RecuveryMessage _message;

        public RecuveryResult(RecuveryMessage message) => _message = message;

        public string Name => _message.Name;

        public int Current => _message.Current;
    }
}