namespace Tauron.Application.ImageOrginazer.ViewModels.Views.ContainerManager
{
    public class ApplyError : UiBase
    {
        public string Message { get; }

        public ApplyError(string message) => Message = message;
    }
}