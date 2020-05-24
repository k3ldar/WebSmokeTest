using System;
using System.Collections.Generic;

using SharedPluginFeatures;

using SmokeTest.Shared;

namespace SmokeTest.Models
{
    public sealed class HomeViewModel : BaseModel
    {
        public HomeViewModel(in BaseModelData baseModelData, in List<HomeCard> homeCards)
            : base(baseModelData)
        {
            HomeCards = homeCards ?? throw new ArgumentNullException(nameof(homeCards));
        }

        public List<HomeCard> HomeCards { get; private set; }
    }
}
