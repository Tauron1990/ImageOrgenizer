using System.Linq;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;
using Tauron.Application.ImageOrganizer.Data.Repositories;

namespace Tauron.Application.ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.RemoveImage)]
    public class RemoveImageRule : IOBusinessRuleBase<ImageData, bool>
    {
        public override bool ActionImpl(ImageData input)
        {
            using (var db = RepositoryFactory.Enter())
            {
                var repo = db.GetRepository<IImageRepository>();
                var itRepo = db.GetRepository<IImageTagRepository>();

                var img = repo.Query(true).Single(e => e.Name == input.Name);
                
                repo.Remove(img);
                foreach (var imageTag in img.Tags)
                    itRepo.Remove(imageTag);


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