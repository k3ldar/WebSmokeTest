using System;
using System.Collections.Generic;

using Middleware;
using Middleware.Accounts;

using Shared.Classes;
using Middleware.Users;

using SmokeTest.Data;
using System.Linq;

namespace SmokeTest.Middleware
{
    public class SmokeTestProvider : ISmokeTestProvider
    {
        private const string TestAdminEmail = "testuseradmin";
        private const string TestAdminPassword = "Edfas;lkd5r049jfdsadsa.kf";
        private readonly IUserSearch _userSearch;
        private readonly IAccountProvider _accountProvider;
        private readonly ILoginProvider _loginProvider;

        public SmokeTestProvider(IUserSearch userSearch, IAccountProvider accountProvider, ILoginProvider loginProvider)
        {
            _userSearch = userSearch ?? throw new ArgumentNullException(nameof(userSearch));
            _accountProvider = accountProvider ?? throw new ArgumentNullException(nameof(accountProvider));
            _loginProvider = loginProvider ?? throw new ArgumentNullException(nameof(loginProvider));
        }

        public void SmokeTestEnd()
        {
            _loginProvider.
        }

        public NVPCodec SmokeTestStart()
        {
            NVPCodec Result = new NVPCodec();

            SearchUser user = _userSearch.GetUsers(1, 1, "", "").Where(u => u.Email.Equals(TestAdminEmail)).FirstOrDefault();

            if (user == null)
            {
                _accountProvider.CreateAccount(TestAdminEmail, TestAdminEmail, TestAdminEmail,
                    TestAdminPassword, String.Empty, String.Empty, String.Empty, String.Empty, 
                    String.Empty, String.Empty, String.Empty, String.Empty, String.Empty, 
                    out long userid);
            }

            if (users.Count > 0)
            {
                Result.Add("Username", users[0].Email);
            }


            return Result;
        }
    }
}
