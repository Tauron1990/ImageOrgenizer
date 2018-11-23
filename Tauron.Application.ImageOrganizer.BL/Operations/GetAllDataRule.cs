using System.Linq;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;
using Tauron.Application.ImageOrganizer.Data.Repositories;

namespace Tauron.Application.ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.GetAllData)]
    public class GetAllDataRule : IOBusinessRuleBase<DataType, AllDataResult>
    {
        public override AllDataResult ActionImpl(DataType input)
        {
            using (RepositoryFactory.Enter())
            {
                ImageData[] imageDatas = null;
                TagData[] tagDatas = null;
                TagTypeData[] tagTypeDatas = null;

                if (input.HasFlag(DataType.ImageData))
                {
                    var imageRepo = RepositoryFactory.GetRepository<IImageRepository>();

                    imageDatas = imageRepo.QueryAsNoTracking(true)
                        .Select(e => new ImageData(e))
                        .ToArray();
                }

                if (input.HasFlag(DataType.TagData))
                {
                    var tagRepo = RepositoryFactory.GetRepository<ITagRepository>();

                    tagDatas = tagRepo.QueryAll()
                        .Select(e => new TagData(e))
                        .ToArray();
                }

                if (input.HasFlag(DataType.TagTypeData))
                {
                    var tagTypeRepo = RepositoryFactory.GetRepository<ITagTypeRepository>();

                    tagTypeDatas = tagTypeRepo.QueryAll()
                        .Select(e => new TagTypeData(e))
                        .ToArray();
                }

                return new AllDataResult(imageDatas, tagDatas, tagTypeDatas);
            }
        }
    }
}