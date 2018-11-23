namespace Tauron.Application.ImageOrganizer.BL
{
    public class AddFileInput
    {
        public string Name { get; }

        public byte[] Bytes { get;  }

        public AddFileInput(string name, byte[] bytes)
        {
            Name = name;
            Bytes = bytes;
        }

    }
}