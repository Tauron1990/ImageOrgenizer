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
        public override RawSqlResult ActionImpl(string input)
        {
            try
            {
                const int pageSize = 50;

                using (RepositoryFactory.Enter())
                {
                    var repo = RepositoryFactory.GetRepository<IImageRepository>();

                    ImageData data;
                    var result = repo.QueryFromSql(input, true).FirstOrDefault();
                    if(result != null)
                        data = new ImageData(result, NaturalStringComparer.Comparer);
                    else
                        return new RawSqlResult(null, -1);

                    //int count = 0;
                    //bool run = true;

                    //while (run)
                    //{
                    //    var arr = repo.QueryAsNoTracking(false).OrderBy(ie => ie.Name).Skip(count).Take(pageSize).ToArray();
                    //    if (arr.Length != pageSize)
                    //        run = false;

                    //    var index = arr.IndexOf(result);

                    //    if (index == -1)
                    //        count += pageSize;
                    //    else
                    //    {
                    //        count += index;
                    //        run = false;
                    //    }
                    //}

                    int count = repo.QueryAsNoTracking(false)
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