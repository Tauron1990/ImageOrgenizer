namespace Tauron.Application.ImageOrganizer.Container
{
    public class RecuveryMessage
    {
        public string Name { get; }
        public int Current { get; }

        public RecuveryMessage(string name, int current)
        {
            Name = name;
            Current = current;
        }
    }
}