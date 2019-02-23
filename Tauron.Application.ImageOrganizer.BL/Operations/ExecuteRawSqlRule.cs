using System;
using System.Linq;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;
using Tauron.Application.ImageOrganizer.BL.Operations.Helper;
using Tauron.Application.ImageOrganizer.Data.Repositories;

namespace Tauron.Application.ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.ExecuteRawSql)]
    public class ExecuteRawSqlRule : IOBusinessRuleBase<string, RawSqlResult>
    {
        [InjectRepo]
        public IImageRepository ImageRepository { get; set; }

        public override RawSqlResult ActionImpl(string input)
        {
            try
            {
                using (Enter())
                {
                    ImageData data;
                    var result = ImageRepository.QueryFromSql(input, true).FirstOrDefault();
                    if(result != null)
                        data = new ImageData(result, NaturalStringComparer.Comparer);
                    else
                        return new RawSqlResult(null, -1);
                    
                    int count = ImageRepository.QueryAsNoTracking(false)
                        .OrderBy(i => i.SortOrder)
                        .Count(i => i.Name != data.Name) - 1;

                    return new RawSqlResult(data, count);
                }
            }
            catch (Exception e)
            {
                return new RawSqlResult(e);
            }
        }
    }
}