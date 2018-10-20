using System;
using System.Linq;
using ImageOrganizer.Data.Repositories;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;

namespace ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.RemoveImage)]
    public class RemoveImageRule : IOBusinessRuleBase<ImageData, bool>
    {
        public override bool ActionImpl(ImageData input)
        {
            using (var db = RepositoryFactory.Enter())
            {
                var repo = db.GetRepository<IImageRepository>();

                var img = repo.Query().Single(e => e.Name == input.Name);
                
                repo.Remove(img);


                var trans = FileContainerManager.GetContainerTransaction();
                try
                {
                    FileContainerManager.Remove(input.Name, null);
                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    return false;
                }

                db.SaveChanges();
                return true;
            }
        }
    }
}