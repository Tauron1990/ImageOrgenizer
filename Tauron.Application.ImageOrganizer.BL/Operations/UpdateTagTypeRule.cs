using System;
using System.Collections.Generic;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;
using Tauron.Application.ImageOrganizer.Data.Repositories;

namespace Tauron.Application.ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.UpdateTagType)]
    public class UpdateTagTypeRule : IOBusinessRuleBase<TagTypeData[], TagTypeData[]>
    {
        [InjectRepo]
        public ITagTypeRepository TagTypeRepository { get; set; }

        public override TagTypeData[] ActionImpl(TagTypeData[] inputs)
        {
            if (inputs.Length == 0) return Array.Empty<TagTypeData>();

            List<TagTypeData> list = new List<TagTypeData>();

            using (var db = Enter())
            {
                foreach (var input in inputs)
                {

                    var tt = TagTypeRepository.Get(input.Name, true);

                    if (tt == null)
                    {
                        tt = input.ToEntity();
                        TagTypeRepository.Add(tt);
                    }
                    else
                        tt.Color = input.Color;

                    list.Add(new TagTypeData(tt));
                }

                db.SaveChanges();
            }

            return list.ToArray();
        }
    }
}