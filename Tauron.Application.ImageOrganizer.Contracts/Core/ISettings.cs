namespace Tauron.Application.ImageOrganizer.Core
{
    public interface ISettings
    {
        string CurrentDatabase { get; set; }

        int PageCount { get; set; }
    }
}