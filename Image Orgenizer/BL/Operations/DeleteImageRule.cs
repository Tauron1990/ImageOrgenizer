using System.Linq;
using ImageOrganizer.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Syncfusion.Windows.Shared;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;

namespace ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.DeleteImage)]
    public class DeleteImageRule : IBusinessRuleBase<string>
    {
        public override void ActionImpl(string input)
        {
            if (input.IsNullOrWhiteSpace())
                return;

            using (var db = RepositoryFactory.Enter())
            {
                var repo = RepositoryFactory.GetRepository<IImageRepository>();

                var imgent = repo.Query()
                    .Include(e => e.ImageTags)
                    .ThenInclude(e => e.TagEntity)
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