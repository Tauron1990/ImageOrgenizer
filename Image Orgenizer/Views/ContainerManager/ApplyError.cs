namespace ImageOrganizer.Views.ContainerManager
{
    public class ApplyError : UiBase
    {
        public string Message { get; }

        public ApplyError(string message) => Message = message;
    }
}