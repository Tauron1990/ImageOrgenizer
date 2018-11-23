namespace Tauron.Application.ImageOrganizer.Container
{
    public class RecuverElement
    {
        public string Name { get; }
        public byte[] Data { get; }
        public int Current { get; }

        public RecuverElement(string name, byte[] data, int current)
        {
            Name = name;
            Data = data;
            Current = current;
        }
    }
}