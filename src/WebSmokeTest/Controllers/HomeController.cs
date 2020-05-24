
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

using Shared.Classes;

using SharedPluginFeatures;

using SmokeTest.Models;
using SmokeTest.Shared;

namespace SmokeTest.Controllers
{
    public class HomeController : BaseController
    {
        #region Private Members

        private readonly ISmokeTestHelper _smokeTestHelper;

        #endregion Private Members

        #region Constructors

        public HomeController(ISmokeTestHelper smokeTestHelper)
        {
            _smokeTestHelper = smokeTestHelper ?? throw new ArgumentNullException(nameof(smokeTestHelper));
        }

        #endregion Constructors

        #region Public Action Methods

        public IActionResult Index()
        {
            UserSession session = GetUserSession();

            HomeViewModel model = new HomeViewModel(GetModelData(), _smokeTestHelper.HomeCardsGet().OrderBy(o => o.SortOrder).ToList());

            return View(model);
        }

        #endregion Public Action Methods
    }
}
