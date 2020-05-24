using System.Collections.Generic;

using SharedPluginFeatures;

namespace SmokeTest.Settings.Classes
{
    public class ClaimsService : IClaimsService
    {
        #region IClaimsService Methods

        public List<string> GetClaims()
        {
            return new List<string>()
            {
                "ViewConfiguration",
                "ManageConfiguration",
            };
        }

        #endregion IClaimsService Methods
    }
}
