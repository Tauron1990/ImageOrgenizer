using System;

namespace Tauron.Application.ImageOrganizer.BL.Services
{
    public interface IContainerService
    {
        void Defrag(DefragInput defragInput);
        void Recuvery(RecuveryInput recuveryInput);
        SwitchContainerOutput SwitchContainer(SwitchContainerInput switchContainerInput);
        Exception ImportFiles(ImporterInput input);
    }
}