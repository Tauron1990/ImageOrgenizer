using System.Linq;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;
using Tauron.Application.ImageOrganizer.Data.Repositories;

namespace Tauron.Application.ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.DeleteImage)]
    public class DeleteImageRule : IBusinessRuleBase<string>
    {
        [InjectRepo]
        public IImageRepository ImageRepository { get; set; }

        public override void ActionImpl(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return;

            using (var db = Enter())
            {
                var imgent = ImageRepository.Query(true)
                    .FirstOrDefault(ent => ent.Name == input);
                if(imgent == null) return;

                ImageRepository.Remove(imgent);

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