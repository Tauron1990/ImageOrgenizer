using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ImageOrganizer.BL;

namespace ImageOrganizer.Views.Models
{
    public interface IImagePager
    {
        (Task<PagerOutput> Current, Task<PagerOutput> Previous, Task<PagerOutput> Next) Initialize(ProfileData profile);

        PagerOutput GetCurrent(ProfileData data);

        Task<PagerOutput> GetPage(PageType type, int next, bool favorite);

        void SetFilter(Func<IEnumerable<string>> filter);

        void IncreaseViewCount(ImageData data);

        Operator Operator { get; set; }
    }
}