using System;
using System.Collections.Generic;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;
using Tauron.Application.ImageOrganizer.Data.Repositories;

namespace Tauron.Application.ImageOrganizer.Data
{
    [ExportRepositoryExtender]
    public sealed class MainDatabase : CommonRepositoryExtender<DatabaseImpl>
    {
        public override IEnumerable<(Type, Type)> GetRepositoryTypes()
        {
            yield return (typeof(ITagRepository), typeof(TagRepository));
            yield return (typeof(ITagTypeRepository), typeof(TagTypeRepository));
            yield return (typeof(IImageRepository), typeof(ImageRepository));
            yield return (typeof(IOptionRepository), typeof(OptionRepository));
            yield return (typeof(IProfileRepository), typeof(ProfileRepository));
            yield return (typeof(IDownloadRepository), typeof(DownloadRepository));
        }
    }
}