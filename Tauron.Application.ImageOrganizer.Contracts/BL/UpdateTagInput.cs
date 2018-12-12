namespace Tauron.Application.ImageOrganizer.BL
{
    public class UpdateTagInput
    {
        public TagData TagData { get; }

        public bool IgnoreTagType { get; }

        public UpdateTagInput(TagData tagData, bool ignoreTagType)
        {
            TagData = tagData;
            IgnoreTagType = ignoreTagType;
        }
    }
}