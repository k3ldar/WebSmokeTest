using System;
using System.IO;
using System.Text;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SharedPluginFeatures;

using SmokeTest.Models;
using SmokeTest.Shared;

namespace SmokeTest.Controllers
{
    [LoggedIn]
    [Authorize(Policy = STConsts.PolicyManageLicense)]
    public class LicenseController : BaseController
    {
        public const string Name = "License";

        #region Private Members

        private readonly ILicenseFactory _licenseFactory;
        private readonly string _licenseFile;

        #endregion Private Members

        #region Constructors

        public LicenseController(ILicenseFactory licenseFactory)
        {
            _licenseFactory = licenseFactory ?? throw new ArgumentNullException(nameof(licenseFactory));

            _licenseFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SmokeTest", "Data", "ActiveLicense.lic");
        }

        #endregion Constructors

        #region Public Action Methods

        [Breadcrumb(Name)]
        public IActionResult Index()
        {
            ILicense license = _licenseFactory.GetActiveLicense();

            LicenseViewModel model = new LicenseViewModel(GetModelData());

            if (license.Expires > DateTime.Now)
                model.Expires = license.Expires;

            if (!String.IsNullOrEmpty(license.RegisteredUser))
                model.RegisteredUser = license.RegisteredUser;

            return View(model);
        }

        [HttpGet]
        [Breadcrumb(nameof(AddLicense), Name, nameof(Index))]
        public IActionResult AddLicense()
        {
            return View(new AddLicenseModel(GetModelData()));
        }

        [HttpPost]
        public IActionResult AddLicense(AddLicenseModel model)
        {
            if (model.LicenseFile == null)
                ModelState.AddModelError(String.Empty, "Please select a licenseFile");

            if (ModelState.IsValid)
            {
                string fileName = Path.GetFileName(model.LicenseFile.FileName);

                using (MemoryStream stream = new MemoryStream())
                {
                    model.LicenseFile.CopyTo(stream);
                    stream.Position = 0;
                    byte[] contents = new byte[stream.Length];
                    stream.ReadAsync(contents);

                    try
                    {
                        ILicense newLicense = _licenseFactory.LoadLicense(Encoding.ASCII.GetString(contents));

                        if (_licenseFactory.LicenseIsValid(newLicense))
                        {
                            _licenseFactory.SetActiveLicense(newLicense);
                            System.IO.File.WriteAllBytes(_licenseFile, contents);
                        }
                        else
                        {
                            ModelState.AddModelError(String.Empty, "License is not valid");
                        }
                    }
                    catch (Exception err)
                    {
                        ModelState.AddModelError(String.Empty, err.Message);
                    }
                }
            }


            if (ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(new AddLicenseModel(GetModelData()));
        }

        #endregion Public Action Methods
    }
}
