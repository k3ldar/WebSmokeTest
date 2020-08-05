using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Middleware;
using Middleware.Accounts;
using Middleware.Users;

using Shared.Classes;

using SharedPluginFeatures;

using static Shared.Utilities;

namespace SmokeTest.Middleware
{
    public class SmokeTestProvider : ISmokeTestProvider
    {
        private readonly IUserSearch _userSearch;
        private readonly IAccountProvider _accountProvider;
        private readonly ILoginProvider _loginProvider;
        private readonly IClaimsProvider _claimsProvider;
        private readonly string _testAdminPassword;
        private readonly string _testAdminEmail;
        private readonly string _testUserEmail;
        private readonly string _testUserPassword;
        private long _adminUserId;
        private long _userId;
        private int _adminUserNameUsage = 0;
        private int _userNameUsage = 0;

        public SmokeTestProvider(IUserSearch userSearch, IAccountProvider accountProvider,
            ILoginProvider loginProvider, IClaimsProvider claimsProvider)
        {
            _userSearch = userSearch ?? throw new ArgumentNullException(nameof(userSearch));
            _accountProvider = accountProvider ?? throw new ArgumentNullException(nameof(accountProvider));
            _loginProvider = loginProvider ?? throw new ArgumentNullException(nameof(loginProvider));
            _claimsProvider = claimsProvider ?? throw new ArgumentNullException(nameof(claimsProvider));

            _testAdminEmail = $"test{DateTime.UtcNow.Ticks}@mail.com";
            _testAdminPassword = $"T{GetRandomPassword(50)}";
            Thread.Sleep(50);
            _testUserEmail = $"test_user{DateTime.UtcNow.Ticks}@mail.com";
            _testUserPassword = $"T{GetRandomPassword(50)}";
        }

        public void SmokeTestEnd()
        {
            _adminUserNameUsage--;

            if (_adminUserNameUsage == 0)
                _accountProvider.DeleteAccount(_adminUserId);

            _userNameUsage--;

            if (_userNameUsage == 0)
                _accountProvider.DeleteAccount(_userId);
        }

        public NVPCodec SmokeTestStart()
        {
            NVPCodec Result = new NVPCodec();

            SearchUser user = _userSearch.GetUsers(1, 1, "", "").Where(u => u.Email.Equals(_testAdminEmail)).FirstOrDefault();

            if (user == null)
            {
                _accountProvider.CreateAccount(_testAdminEmail, _testAdminEmail, _testAdminEmail,
                    _testAdminPassword, String.Empty, String.Empty, String.Empty, String.Empty,
                    String.Empty, String.Empty, String.Empty, String.Empty, String.Empty,
                    out long userid);

                user = _userSearch.GetUsers(1, 1, "", "").Where(u => u.Email.Equals(_testAdminEmail)).FirstOrDefault();

                List<string> claims = _claimsProvider.GetAllClaims();
                _claimsProvider.SetClaimsForUser(userid, claims);
            }

            if (user != null)
            {
                _adminUserNameUsage++;
                _adminUserId = user.Id;
                Result.Add("AdminUsername", user.Email);
                Result.Add("AdminUserPassword", _testAdminPassword);
            }

            user = _userSearch.GetUsers(1, 1, "", "").Where(u => u.Email.Equals(_testUserEmail)).FirstOrDefault();

            if (user == null)
            {
                _accountProvider.CreateAccount(_testUserEmail, _testUserEmail, _testUserEmail,
                    _testUserPassword, String.Empty, String.Empty, String.Empty, String.Empty,
                    String.Empty, String.Empty, String.Empty, String.Empty, String.Empty,
                    out long userid);

                user = _userSearch.GetUsers(1, 1, "", "").Where(u => u.Email.Equals(_testUserEmail)).FirstOrDefault();
            }

            if (user != null)
            {
                _userNameUsage++;
                _userId = user.Id;
                Result.Add("Username", user.Email);
                Result.Add("UserPassword", _testUserPassword);
            }

            return Result;
        }
    }
}
