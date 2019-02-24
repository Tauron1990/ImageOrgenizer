using System;
using Tauron.Application.Common.MVVM.Dynamic;
using Tauron.Application.ImageOrganizer.BL.Operations;
using Tauron.Application.Ioc;

namespace Tauron.Application.ImageOrganizer.BL.Services
{
    [Export(typeof(IContainerService))]
    [CreateRuleCall]
    public abstract class ContainerService : IContainerService
    {
        [BindRule]
        public abstract void Defrag(DefragInput defragInput);
        [BindRule]
        public abstract void Recuvery(RecuveryInput recuveryInput);
        
        [BindRule]
        public abstract SwitchContainerOutput SwitchContainer(SwitchContainerInput switchContainerInput);

        [BindRule(RuleNames.FileImporter)]
        public abstract Exception ImportFiles(ImporterInput input);
    }
}