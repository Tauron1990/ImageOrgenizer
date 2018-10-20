using JetBrains.Annotations;

namespace ImageOrganizer.BL
{
    [UsedImplicitly]
    public class RadomPagerHelper
    {
        public int RandomView { get; set; }
        public string Name { get; set; }

        public RadomPagerHelper(int randomView, string name)
        {
            RandomView = randomView;
            Name = name;
        }

        public RadomPagerHelper()
        {
            
        }
    }
}