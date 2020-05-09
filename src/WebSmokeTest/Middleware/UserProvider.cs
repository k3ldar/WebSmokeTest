using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

using Microsoft.AspNetCore.Authentication;

using Middleware;

using PluginManager.Abstractions;

using SharedPluginFeatures;
using static SharedPluginFeatures.Constants;

using WebSmokeTest.Data;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;

namespace WebSmokeTest.Middleware
{
    public class UserProvider : ILoginProvider, IClaimsProvider
    {
        #region Private Members

        private const string ClaimIdentityGeneric = "Generic";

        [Obsolete]
        private const string Key = "ASDFasdf8uq43w foasi034q257uwei";

        private readonly IPluginClassesService _pluginClassesService;
        private readonly ISettingsProvider _settingsProvider;
        private readonly List<User> _users;
        private readonly string _dataPath;

        #endregion Private Members

        #region Constructors

        public UserProvider(IPluginClassesService pluginClassesService,
            ISettingsProvider settingsProvider,
            IConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            _pluginClassesService = pluginClassesService ?? throw new ArgumentNullException(nameof(pluginClassesService));
            _settingsProvider = settingsProvider ?? throw new ArgumentNullException(nameof(settingsProvider));
            _dataPath = Path.Combine(configuration.GetValue<string>(WebHostDefaults.ContentRootKey), "Data");
            _users = LoadUsers();
        }

        #endregion Constructors

        #region ILoginProvider Methods

        public bool ForgottenPassword(in string username)
        {
            throw new NotImplementedException();
        }

        public LoginResult Login(in string username, in string password, in string ipAddress, in byte attempts, ref UserLoginDetails loginDetails)
        {
            string loginEmail = username;
            string pword = password;
            User foundUser = _users.Where(u => u.Email.Equals(loginEmail) && u.Password.Equals(Shared.Utilities.Encrypt(pword, Key))).FirstOrDefault();

            if (foundUser != null)
            {
                loginDetails.Email = foundUser.Email;
                loginDetails.RememberMe = true;
                loginDetails.UserId = foundUser.UserId;
                loginDetails.Username = foundUser.Username;

                if (Shared.Utilities.Decrypt(foundUser.Password, Key).Equals("password"))
                {
                    return LoginResult.PasswordChangeRequired;
                }

                return LoginResult.Success;
            }

            return LoginResult.InvalidCredentials;
        }

        public bool UnlockAccount(in string username, in string unlockCode)
        {
            throw new NotImplementedException();
        }

        #endregion ILoginProvider Methods

        #region IClaimsProvider Methods

        public List<string> GetAllClaims()
        {
            List<string> Result = new List<string>();

            foreach (IClaimsService claimsService in _pluginClassesService.GetPluginClasses<IClaimsService>())
            {
                foreach (string claim in claimsService.GetClaims())
                {
                    if (!Result.Contains(claim))
                        Result.Add(claim);
                }
            }

            return Result;
        }

        public AuthenticationProperties GetAuthenticationProperties()
        {
            return new AuthenticationProperties()
            {
                AllowRefresh = true,
                IsPersistent = true,
            };
        }

        public List<string> GetClaimsForUser(in long id)
        {
            if (TryFindUser(id, out User user))
            {
                return user.Claims;
            }

            return new List<string>();
        }

        public List<ClaimsIdentity> GetUserClaims(in long userId)
        {
            if (TryFindUser(userId, out User user))
            {
                List<ClaimsIdentity> Result = new List<ClaimsIdentity>();

                List<Claim> userClaims = new List<Claim>();
                userClaims.Add(new Claim(ClaimNameUsername, user.Username));
                userClaims.Add(new Claim(ClaimNameUserEmail, user.Email));
                userClaims.Add(new Claim(ClaimNameUserId, user.UserId.ToString()));
                Result.Add(new ClaimsIdentity(userClaims, ClaimIdentityUser));

                List<Claim> webClaims = new List<Claim>();
                webClaims.Add(new Claim(ClaimNameAdministrator, "true"));
                webClaims.Add(new Claim(ClaimNameStaff, "true"));
                webClaims.Add(new Claim(ClaimNameManageSeo, "true"));
                Result.Add(new ClaimsIdentity(webClaims, ClaimIdentityWebsite));

                List<Claim> genericClaims = new List<Claim>();

                foreach (string claimName in GetAllClaims())
                {
                    genericClaims.Add(new Claim(claimName, "true"));
                }

                Result.Add(new ClaimsIdentity(genericClaims, ClaimIdentityGeneric));

                return Result;
            }

            return new List<ClaimsIdentity>();
        }

        public bool SetClaimsForUser(in long id, in List<string> claims)
        {
            if (TryFindUser(id, out User user))
            {
                user.Claims = claims;
                SaveUsers();
                return true;
            }

            return false;
        }

        #endregion IClaimsProvider Methods

        #region Private Methods

        private bool TryFindUser(long id, out User user)
        {
            user = _users.Where(u => u.UserId == id).FirstOrDefault();
            return user != null;
        }

        private void SaveUsers()
        {
            string userFile = Path.Combine(_dataPath, "Users.dat");

            if (File.Exists(userFile))
            {
                File.Delete(userFile);
            }

            File.WriteAllText(userFile, JsonConvert.SerializeObject(_users));
        }

        private List<User> LoadUsers()
        {
            string userFile = Path.Combine(_dataPath, "Users.dat");
            
            if (File.Exists(userFile))
            {
                return JsonConvert.DeserializeObject<List<User>>(File.ReadAllText(userFile));
            }
            else
            {
                List<User> Result = new List<User>();
                Result.Add(CreateDefaultUser());
                return Result;
            }
        }

        private User CreateDefaultUser()
        {
            return new User()
            {
                Username = "Admin",
                Email = "admin",
                ForceChangePassword = true,
                Password = Shared.Utilities.Encrypt("password", Key),
                Claims = new List<string>(),
            };

        }

        #endregion Private Methods
    }
}
