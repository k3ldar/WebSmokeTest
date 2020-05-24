using System.Collections.Generic;

using SharedPluginFeatures;

namespace SmokeTest.Scheduler.Classes
{
    public class ClaimsService : IClaimsService
    {
        #region IClaimsService Methods

        public List<string> GetClaims()
        {
            return new List<string>()
            {
                "ManageSchedules",
                "ViewSchedules",
            };
        }

        #endregion IClaimsService Methods
    }
}
