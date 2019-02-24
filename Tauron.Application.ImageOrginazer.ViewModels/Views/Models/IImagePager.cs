using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tauron.Application.ImageOrganizer.BL;
using Tauron.Application.ImageOrganizer.BL.Services;

namespace Tauron.Application.ImageOrginazer.ViewModels.Views.Models
{
    public interface IImagePager
    {
        string Name { get; }

        (Task<PagerOutput> Current, Task<PagerOutput> Previous, Task<PagerOutput> Next) Initialize(ProfileData profile);

        PagerOutput GetCurrent(ProfileData data);

        Task<PagerOutput> GetPage(int next, bool favorite);

        void SetFilter(Func<IEnumerable<string>> filter);

        void IncreaseViewCount(ImageData data);

        IImageService Operator { get; set; }
    }
}