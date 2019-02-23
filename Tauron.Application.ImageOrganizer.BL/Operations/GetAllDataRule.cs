using System;
using System.Linq;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;
using Tauron.Application.ImageOrganizer.BL.Operations.Helper;
using Tauron.Application.ImageOrganizer.Data.Repositories;

namespace Tauron.Application.ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.GetAllData)]
    public class GetAllDataRule : IOBusinessRuleBase<DataType, AllDataResult>
    {
        [InjectRepo]
        public Lazy<IImageRepository> ImageRepository { get; set; }

        [InjectRepo]
        public Lazy<ITagRepository> TagRepository { get; set; }

        [InjectRepo]
        public Lazy<ITagTypeRepository> TagTypeRepository { get; set; }

        public override AllDataResult ActionImpl(DataType input)
        {
            using (Enter())
            {
                ImageData[] imageDatas = null;
                TagData[] tagDatas = null;
                TagTypeData[] tagTypeDatas = null;

                if (input.HasFlag(DataType.ImageData))
                {
                    imageDatas = ImageRepository.Value.QueryAsNoTracking(true)
                        .Select(e => new ImageData(e, NaturalStringComparer.Comparer))
                        .ToArray();
                }

                if (input.HasFlag(DataType.TagData))
                {
                    tagDatas = TagRepository.Value.QueryAll()
                        .Select(e => new TagData(e))
                        .ToArray();
                }

                if (input.HasFlag(DataType.TagTypeData))
                {
                    tagTypeDatas = TagTypeRepository.Value.QueryAll()
                        .Select(e => new TagTypeData(e))
                        .ToArray();
                }

                return new AllDataResult(imageDatas, tagDatas, tagTypeDatas);
            }
        }
    }
}