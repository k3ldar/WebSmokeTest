
using System;

using Microsoft.AspNetCore.Mvc;

using SharedPluginFeatures;

using SmokeTest.SystemTests.Models;

namespace SmokeTest.SystemTests.Controllers
{
    public class SysTestController : BaseController
    {
        public const string Name = "SysTest";

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult SimpleInput()
        {
            return View(new SimpleTestModel(GetModelData()));
        }

        [HttpPost]
        [SmokeTest(400, PostType.Form, "frmSimpleInput", inputData: "Username=fred&Password=bloggs", searchData: "<h1>Simple Input Test Form</h1>")]
        public IActionResult SimpleInput(SimpleTestModel model)
        {
            Shared.Classes.UserSession user = base.GetUserSession();
            if (model.Username == null)
                throw new ArgumentNullException(nameof(model.Username));

            if (model.Password == null)
                throw new ArgumentNullException(nameof(model.Password));

            if (model.Username == "fred" && model.Password == "bloggs")
                HttpContext.Response.StatusCode = 400;

            return new ContentResult();
        }
    }
}
