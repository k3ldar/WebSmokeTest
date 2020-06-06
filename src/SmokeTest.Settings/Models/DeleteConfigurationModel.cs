using System;
using SharedPluginFeatures;

namespace SmokeTest.Settings.Models
{
    public sealed class DeleteConfigurationModel : BaseModel
    {
        public DeleteConfigurationModel()
        {

        }

        public DeleteConfigurationModel(in BaseModelData modelData, in string uniqueId)
            : base (modelData)
        {
            if (string.IsNullOrEmpty(uniqueId))
                throw new ArgumentNullException(nameof(uniqueId));

            UniqueId = uniqueId;
        }

        public string UniqueId { get; set; }

        public string Confirm { get; set; }
    }
}
