using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;
using Tauron.Application.ImageOrganizer.Data.Repositories;

namespace Tauron.Application.ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.RemoveTag)]
    public class RemoveTagRule : IOBusinessRuleBase<TagData, bool>
    {
        [InjectRepo]
        public ITagRepository TagRepository { get; set; }

        public override bool ActionImpl(TagData input)
        {
            using (var db = Enter())
            {
                var tag = TagRepository.GetName(input.Name, true);
                if (tag == null) return false;

                TagRepository.Remove(tag);
                db.SaveChanges();

                return true;
            }
        }
    }
}