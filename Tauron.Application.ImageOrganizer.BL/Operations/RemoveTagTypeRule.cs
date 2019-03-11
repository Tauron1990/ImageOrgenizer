using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;
using Tauron.Application.ImageOrganizer.Data.Repositories;

namespace Tauron.Application.ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.RemoveTagType)]
    public class RemoveTagTypeRule : IOBusinessRuleBase<TagTypeData, bool>
    {
        [InjectRepo]
        public ITagTypeRepository TagTypeRepository { get; set; }

        public override bool ActionImpl(TagTypeData input)
        {
            using (var db = Enter())
            {
                var temp = TagTypeRepository.Get(input.Name, true);
                if (temp == null) return false;
                TagTypeRepository.Remove(temp);

                db.SaveChanges();
                return true;
            }
        }
    }
}