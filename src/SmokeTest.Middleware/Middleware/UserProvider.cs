using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;

using Microsoft.AspNetCore.Authentication;

using Middleware;
using Middleware.Accounts;
using Middleware.Accounts.Invoices;
using Middleware.Accounts.Orders;
using Middleware.Users;

using PluginManager.Abstractions;

using SharedPluginFeatures;

using SmokeTest.Data;
using SmokeTest.Shared;
using SmokeTest.Shared.Interfaces;

using static Shared.Utilities;
using static SharedPluginFeatures.Constants;

using ILoadData = SmokeTest.Shared.Interfaces.ILoadData;
using ISaveData = SmokeTest.Shared.ISaveData;

namespace SmokeTest.Middleware
{
    public class UserProvider : ILoginProvider, IClaimsProvider, IAccountProvider, IUserSearch
    {
        #region Private Members

        private const string ClaimIdentityGeneric = "Generic";

        private const string Key = "ASDFasdf8uq43w foasi034q257uwei";

        private readonly IPluginClassesService _pluginClassesService;
        private readonly List<User> _users;
        private readonly string _dataPath;
        private readonly ISaveData _saveData;
        private readonly ILoadData _loadData;
        private readonly IIdManager _idManager;

        #endregion Private Members

        #region Constructors

        public UserProvider(IPluginClassesService pluginClassesService,
            ISaveData saveData, ILoadData loadData, IIdManager idManager)
        {
            _pluginClassesService = pluginClassesService ?? throw new ArgumentNullException(nameof(pluginClassesService));
            _saveData = saveData ?? throw new ArgumentNullException(nameof(saveData));
            _loadData = loadData ?? throw new ArgumentNullException(nameof(loadData));
            _idManager = idManager ?? throw new ArgumentNullException(nameof(idManager));

            _dataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SmokeTest", "Data");

            if (!Directory.Exists(_dataPath))
                Directory.CreateDirectory(_dataPath);

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
            User foundUser = null;
            long userId = loginDetails.UserId;

            if (loginDetails.RememberMe)
                foundUser = _users.Where(u => u.UserId == userId).FirstOrDefault();
            else
                foundUser = _users.Where(u => u.Email.Equals(loginEmail) && u.Password.Equals(Encrypt(pword, Key))).FirstOrDefault();

            if (foundUser != null)
            {
                loginDetails.Email = foundUser.Email;
                loginDetails.RememberMe = true;
                loginDetails.UserId = foundUser.UserId;
                loginDetails.Username = foundUser.FirstName;

                if (Decrypt(foundUser.Password, Key).Equals("password"))
                {
                    return LoginResult.PasswordChangeRequired;
                }

                return LoginResult.Success;
            }

            return LoginResult.InvalidCredentials;
        }

        public bool UnlockAccount(in string username, in string unlockCode)
        {
            return true;
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
                userClaims.Add(new Claim(ClaimNameUsername, user.FirstName));
                userClaims.Add(new Claim(ClaimNameUserEmail, user.Email));
                userClaims.Add(new Claim(ClaimNameUserId, user.UserId.ToString()));
                Result.Add(new ClaimsIdentity(userClaims, ClaimIdentityUser));

                List<Claim> genericClaims = new List<Claim>();

                foreach (string claimName in user.Claims)
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
                return SaveUsers();
            }

            return false;
        }

        #endregion IClaimsProvider Methods

        #region IAccountProvider Methods

        public bool AddDeliveryAddress(in long userId, in DeliveryAddress deliveryAddress)
        {
            throw new InvalidOperationException("Delivery address not supported");
        }

        public bool ChangePassword(in long userId, in string newPassword)
        {
            int id = Convert.ToInt32(userId);

            User user = _users.Where(u => u.UserId.Equals(id)).FirstOrDefault();

            if (user == null)
                return false;

            user.Password = Encrypt(newPassword, Key);

            return SaveUsers();
        }

        public bool ConfirmEmailAddress(in long userId, in string confirmationCode)
        {
            throw new InvalidOperationException("Email confirmation not supported");
        }

        public bool ConfirmTelephoneNumber(in long userId, in string confirmationCode)
        {
            throw new InvalidOperationException("Telephone confirmation not supported");
        }

        public bool CreateAccount(in string email, in string firstName, in string surname, in string password,
            in string telephone, in string businessName, in string addressLine1, in string addressLine2,
            in string addressLine3, in string city, in string county, in string postcode, in string countryCode, out long userId)
        {
            userId = -1;
            string emailAddress = email;

            if (_users.Where(u => u.Email.Equals(emailAddress, StringComparison.InvariantCultureIgnoreCase)).Any())
                return false;

            User user = new User
            {
                AccountLocked = false,
                Email = email,
                FirstName = firstName,
                Surname = surname,
                Password = password,
                UserId = _idManager.GenerateId(),
                ForceChangePassword = false
            };

            _users.Add(user);
            userId = user.UserId;
            return true;
        }

        public bool DeleteAccount(in Int64 userId)
        {
            for (int i = 0; i < _users.Count; i++)
            {
                if (_users[i].UserId == userId)
                {
                    _users.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public bool AccountLock(in Int64 userId)
        {
            throw new NotImplementedException();
        }

        public bool AccountUnlock(in Int64 userId)
        {
            throw new NotImplementedException();
        }

        public bool DeleteDeliveryAddress(in long userId, in DeliveryAddress deliveryAddress)
        {
            throw new InvalidOperationException("Delivery address not supported");
        }

        public AddressOptions GetAddressOptions(in AddressOption addressOption)
        {
            return new AddressOptions();
        }

        public Address GetBillingAddress(in long userId)
        {
            throw new InvalidOperationException("Address not supported");
        }

        public DeliveryAddress GetDeliveryAddress(in long userId, in int deliveryAddressId)
        {
            throw new InvalidOperationException("Address not supported");
        }

        public List<DeliveryAddress> GetDeliveryAddresses(in long userId)
        {
            throw new InvalidOperationException("Address not supported");
        }

        public MarketingOptions GetMarketingOptions()
        {
            throw new InvalidOperationException("Marketing not supported");
        }

        public Marketing GetMarketingPreferences(in long userId)
        {
            throw new InvalidOperationException("Marketing not supported");
        }

        public bool GetUserAccountDetails(in long userId, out string firstName, out string lastName, out string email,
            out bool emailConfirmed, out string telephone, out bool telephoneConfirmed)
        {
            firstName = String.Empty;
            lastName = String.Empty;
            email = String.Empty;
            emailConfirmed = false;
            telephone = String.Empty;
            telephoneConfirmed = false;

            int id = Convert.ToInt32(userId);

            User user = _users.Where(u => u.UserId.Equals(id)).FirstOrDefault();

            if (user == null)
                return false;

            firstName = user.FirstName;
            lastName = user.Surname;
            email = user.Email;

            return true;
        }

        public List<Invoice> InvoicesGet(in long userId)
        {
            throw new InvalidOperationException("Invoices not supported");
        }

        public void OrderPaid(in Order order, in PaymentStatus paymentStatus, in string message)
        {
            throw new InvalidOperationException("Orders not supported");
        }

        public List<Order> OrdersGet(in long userId)
        {
            throw new InvalidOperationException("Orders not supported");
        }

        public bool SetBillingAddress(in long userId, in Address billingAddress)
        {
            throw new InvalidOperationException("Address not supported");
        }

        public bool SetDeliveryAddress(in long userId, in DeliveryAddress deliveryAddress)
        {
            throw new InvalidOperationException("Address not supported");
        }

        public bool SetMarketingPreferences(in long userId, in Marketing marketing)
        {
            throw new InvalidOperationException("Marketing not supported");
        }

        public bool SetUserAccountDetails(in long userId, in string firstName, in string lastName, in string email, in string telephone)
        {
            int id = Convert.ToInt32(userId);

            User user = _users.Where(u => u.UserId.Equals(id)).FirstOrDefault();

            if (user == null)
                return false;

            user.FirstName = firstName;
            user.Surname = lastName;
            user.Email = email;

            return SaveUsers();
        }

        #endregion IAccountProvider Methods

        #region IUserSearch Methods

        public List<SearchUser> GetUsers(in int pageNumber, in int pageSize, string searchField, string searchOrder)
        {
            List<SearchUser> Result = new List<SearchUser>();

            foreach (User user in _users)
                Result.Add(new SearchUser(user.UserId, user.Email, user.Email));

            return Result;
        }

        #endregion IUserSearch Methods

        #region Private Methods

        private bool TryFindUser(long id, out User user)
        {
            user = _users.Where(u => u.UserId == id).FirstOrDefault();
            return user != null;
        }

        private bool SaveUsers()
        {
            return _saveData.Save<List<User>>(_users, _dataPath, "Users.dat");
        }

        private List<User> LoadUsers()
        {
            List<User> Result = _loadData.Load<List<User>>(_dataPath, "Users.dat");

            if (Result == null)
                Result = new List<User>();

            if (Result.Count == 0)
                Result.Add(CreateDefaultUser());

            return Result;
        }

        private User CreateDefaultUser()
        {
            return new User()
            {
                FirstName = "Admin",
                Email = "admin",
                ForceChangePassword = true,
                Password = Encrypt("password", Key),
                Claims = new List<string>()
                {
                    "StaffMember",
                    "Administrator",
                    "UserPermissions",
                    "ViewConfiguration",
                    "AddConfiguration",
                    "EditConfiguration",
                    "ViewSchedules",
                    "ManageConfiguration",
                    "ManageSchedules",
                    "ClaimManageLicense"
                },
            };

        }

        #endregion Private Methods
    }
}
