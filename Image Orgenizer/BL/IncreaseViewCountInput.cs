namespace ImageOrganizer.BL
{
    public class IncreaseViewCountInput
    {
        public string Name { get;  }
        public bool IsRandom { get;}

        public IncreaseViewCountInput(string name, bool isRandom)
        {
            Name = name;
            IsRandom = isRandom;
        }
    }
}