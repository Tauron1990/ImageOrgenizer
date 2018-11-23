using System.Linq;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;
using Tauron.Application.ImageOrganizer.Data.Repositories;

namespace Tauron.Application.ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.DeleteImage)]
    public class DeleteImageRule : IBusinessRuleBase<string>
    {
        public override void ActionImpl(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return;

            using (var db = RepositoryFactory.Enter())
            {
                var repo = RepositoryFactory.GetRepository<IImageRepository>();

                var imgent = repo.Query(true)
                    .FirstOrDefault(ent => ent.Name == input);
                if(imgent == null) return;

                repo.Remove(imgent);

                using (var trans = FileContainerManager.GetContainerTransaction())
                {
                    FileContainerManager.Remove(input, trans);

                    trans.Commit();
                    db.SaveChanges();
                }
            }
        }
    }
}