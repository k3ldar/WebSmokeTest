using System.Collections.Generic;

using SharedPluginFeatures;

namespace SmokeTest.Configuration.Models
{
    public class ViewConfigurationViewModel : BaseModel
    {
        #region Constructors

        public ViewConfigurationViewModel(in BaseModelData modelData)
            : base(modelData)
        {
            Configurations = new List<TestConfigurationViewDetailsModel>();
        }

        #endregion Constructors

        #region Properties

        public List<TestConfigurationViewDetailsModel> Configurations { get; }
        #endregion Properties
    }
}
