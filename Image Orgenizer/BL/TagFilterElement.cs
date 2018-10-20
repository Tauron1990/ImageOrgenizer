namespace ImageOrganizer.BL
{
    public class TagFilterElement
    {
        //public UIObservableCollection<TagFilterElement> Elements { get; }

        public TagFilterElement(TagElement tag)
        {
            Tag = tag;
            //Elements = new UIObservableCollection<TagFilterElement>();
        }

        public TagElement Tag { get; }
    }
}