namespace Tauron.Application.ImageOrganizer.BL
{
    public class ReplaceImageInput
    {
        public ReplaceImageInput(byte[] data, string name)
        {
            Data = data;
            Name = name;
        }

        public string Name { get; }

        public byte[] Data { get; }
    }
}