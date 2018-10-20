using System;
using System.Linq;
using ImageOrganizer.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Syncfusion.Data.Extensions;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;

namespace ImageOrganizer.BL.Operations
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
                    var result = repo.QueryAsNoTracking().FromSql(input).FirstOrDefault();
                    if(result != null)
                        data = new ImageData(result);
                    else
                        return new RawSqlResult(null, -1);

                    int count = 0;
                    bool run = true;

                    while (run)
                    {
                        var arr = repo.QueryAsNoTracking().OrderBy(ie => ie.Name).Skip(count).Take(pageSize).ToArray();
                        if (arr.Length != pageSize)
                            run = false;

                        var index = arr.IndexOf(result);

                        if (index == -1)
                            count += pageSize;
                        else
                        {
                            count += index;
                            run = false;
                        }
                    }



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