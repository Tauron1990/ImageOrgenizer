namespace ImageOrganizer.BL
{
    public class AllDataResult
    {
        public ImageData[] ImageDatas { get; }

        public TagData[] TagDatas { get; }

        public TagTypeData[] TagTypeDatas { get; }

        public AllDataResult(ImageData[] imageDatas, TagData[] tagDatas, TagTypeData[] tagTypeDatas)
        {
            ImageDatas = imageDatas;
            TagDatas = tagDatas;
            TagTypeDatas = tagTypeDatas;
        }
    }
}