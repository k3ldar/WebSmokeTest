using System;

using SharedPluginFeatures;

namespace SmokeTest.Models
{
    public class LicenseViewModel : BaseModel
    {
        public LicenseViewModel(in BaseModelData baseModelData)
            : base(baseModelData)
        {

        }

        public DateTime Expires { get; internal set; }

        public string RegisteredUser { get; internal set; }
    }
}
