using System;
using System.Collections.Generic;

using Middleware;
using Middleware.Accounts.Licences;

namespace SmokeTest.Middleware
{
    public class LicenseProvider : ILicenceProvider
    {
        public bool LicenceSendEmail(in long userId, in int licenceId)
        {
            throw new NotImplementedException();
        }

        public List<Licence> LicencesGet(in long userId)
        {
            throw new NotImplementedException();
        }

        public LicenceCreate LicenceTrialCreate(in long userId, in LicenceType licenceType)
        {
            throw new NotImplementedException();
        }

        public List<LicenceType> LicenceTypesGet()
        {
            return new List<LicenceType>();
        }

        public bool LicenceUpdateDomain(in long userId, in Licence licence, in string domain)
        {
            throw new NotImplementedException();
        }
    }
}
