using System.Linq;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;
using Tauron.Application.ImageOrganizer.Data.Repositories;

namespace Tauron.Application.ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.RemoveImage)]
    public class RemoveImageRule : IOBusinessRuleBase<ImageData, bool>
    {
        [InjectRepo]
        public IImageRepository ImageRepository { get; set; }

        public override bool ActionImpl(ImageData input)
        {
            using (var db = Enter())
            {
                var img = ImageRepository.Query(true).Single(e => e.Name == input.Name);

                ImageRepository.Remove(img);

                var trans = FileContainerManager.GetContainerTransaction();
                try
                {
                    FileContainerManager.Remove(input.Name, trans);
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