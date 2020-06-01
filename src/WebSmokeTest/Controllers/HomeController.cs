
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using PluginManager.Abstractions;
using Shared.Classes;

using SharedPluginFeatures;
using SmokeTest.Classes;
using SmokeTest.Models;
using SmokeTest.Shared;

namespace SmokeTest.Controllers
{
    public class HomeController : BaseController
    {
        #region Private Members

        private readonly ISmokeTestHelper _smokeTestHelper;
        private readonly ISettingsProvider _settingsProvider;

        #endregion Private Members

        #region Constructors

        public HomeController(ISmokeTestHelper smokeTestHelper, ISettingsProvider settingsProvider)
        {
            _smokeTestHelper = smokeTestHelper ?? throw new ArgumentNullException(nameof(smokeTestHelper));
            _settingsProvider = settingsProvider ?? throw new ArgumentNullException(nameof(settingsProvider));
        }

        #endregion Constructors

        #region Public Action Methods

        public IActionResult Index()
        {
            UserSession session = GetUserSession();

            HomeViewModel model = new HomeViewModel(GetModelData(), _smokeTestHelper.HomeCardsGet().OrderBy(o => o.SortOrder).ToList());

            SmokeTestSettings homeSettings = _settingsProvider.GetSettings<SmokeTestSettings>("SmokeTest");
            model.AutoUpdateFrequency = homeSettings.AutoUpdateFrequency;
            model.AutoUpdatePage = homeSettings.AutoUpdatePage;

            return View(model);
        }

        #endregion Public Action Methods
    }
}
