
using System;
using System.Text;

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
        [SmokeTest(400, "frmSimpleInput", inputData: "Username=fred&Password=bloggs", searchData: "<h1>Simple Input Test Form</h1>")]
        public IActionResult SimpleInput()
        {
            return View(new SimpleTestModel(GetModelData()));
        }

        [HttpPost]
        public IActionResult SimpleInput(SimpleTestModel model)
        {
            byte[] body = new byte[HttpContext.Request.ContentLength.Value];
            HttpContext.Request.Body.ReadAsync(body, 0, body.Length);
            string s = Encoding.UTF8.GetString(body);
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
