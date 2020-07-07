using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Mvc;

using SharedPluginFeatures;

using SmokeTest.Settings.Models;
using SmokeTest.Shared;
using SmokeTest.Shared.Classes;
using SmokeTest.Shared.Engine;


namespace SmokeTest.Settings.Controllers
{
    public partial class ConfigurationController
    {
        #region Public Action Methods

        [Route("/Configuration/TestNew/{testConfigurationId}/")]
        public IActionResult TestNew(string testConfigurationId)
        {
            TestConfiguration configuration = _testConfigurationProvider.Configurations
                .Where(c => c.UniqueId.Equals(testConfigurationId))
                .FirstOrDefault();

            if (configuration == null)
            {
                return RedirectToAction(nameof(Index));
            }

            return View("EditTest", CreateTestEditModel(configuration));
        }

        [HttpPost]
        public IActionResult TestEdit(TestEditModel model)
        {
            if (model == null)
                return RedirectToAction(nameof(Index));

            TestConfiguration configuration = _testConfigurationProvider.Configurations
                .Where(c => c.UniqueId.Equals(model.TestConfigurationId))
                .FirstOrDefault();

            if (configuration == null)
            {
                return RedirectToAction(nameof(Index));
            }

            TestEditModel newModel = ValidateTestModel(model, configuration);

            if (newModel == null || ModelState.IsValid)
            {
                return RedirectToAction(nameof(Edit), new { id = configuration.UniqueId });
            }

            return View("EditTest", newModel);
        }

        [Route("/Configuration/TestEdit/{testConfigurationId}/{testId}/")]
        public IActionResult TestAlter(string testConfigurationId, string testId)
        {
            TestConfiguration configuration = _testConfigurationProvider.Configurations
                .Where(c => c.UniqueId.Equals(testConfigurationId))
                .FirstOrDefault();

            if (configuration == null)
            {
                return RedirectToAction(nameof(Index));
            }

            foreach (WebSmokeTestItem test in configuration.Tests)
            {
                string uniqueTestId = Report.GenerateTestHash(test);

                if (testId.Equals(uniqueTestId))
                {
                    return View("EditTest", EditTestEditModel(configuration, test));
                }
            }

            return RedirectToAction(nameof(Edit), new { id = configuration.UniqueId });
        }

        [HttpPost]
        [Route("/Configuration/GetFormValues/{testConfigurationId}/{formId}/")]
        public IActionResult GetFormValues(string testConfigurationId, string formId)
        {
            TestConfiguration configuration = _testConfigurationProvider.Configurations
                .Where(c => c.UniqueId.Equals(testConfigurationId))
                .FirstOrDefault();

            if (configuration != null)
            {
                TestSchedule testSchedule = _scheduleHelper.Schedules.Where(s => s.TestId == configuration.UniqueId).FirstOrDefault();

                if (testSchedule != null)
                {
                    Report latestReport = _reportHelper.MostRecentReport(testSchedule.UniqueId);

                    if (latestReport != null)
                    {
                        FormAnalysis selectedForm = latestReport.FormsAnalysed.Where(f => f.Id.Equals(formId)).FirstOrDefault();

                        if (selectedForm != null)
                        {
                            string route = selectedForm.Action;

                            //string[] parms = { };

                            //if (!String.IsNullOrEmpty(formAction.Query))
                            //    formAction.Query.Substring(1).Split('&');

                            string parameters = String.Empty;

                            //foreach (string p in parms)
                            //    parameters += $"p\r\n";


                            string inputData = String.Empty;

                            foreach (FormInput field in selectedForm.Inputs)
                                if (InputAllowed(inputData, field.Name))
                                    inputData += $"{field.Name}=\r\n";

                            foreach (FormTextArea textArea in selectedForm.TextAreas)
                                if (InputAllowed(inputData, textArea.Name))
                                    inputData += $"{textArea.Name}=\r\n";

                            foreach (FormOption option in selectedForm.Options)
                                if (InputAllowed(inputData, option.Name))
                                    inputData += $"{option.Name}=\r\n";

                            return Json(new { inputData, parameters, route });
                        }
                    }
                }
            }

            return Json(new { inputData = "", parameters = "", route = "" });
        }

        #endregion Public Action Methods

        #region Private Methods

        private bool InputAllowed(in string current, in string value)
        {
            return !String.IsNullOrEmpty(value) &&
               !value.Equals("__RequestVerificationToken") &&
               !current.Contains(value);
        }

        private TestEditModel ValidateTestModel(TestEditModel model, in TestConfiguration configuration)
        {
            if (String.IsNullOrEmpty(model.Name))
                ModelState.AddModelError(nameof(model.Name), "Please enter a valid name");

            if (!HttpMethodTypes.Where(mt => mt.Value.Equals(model.Method)).Any())
                ModelState.AddModelError(nameof(model.Method), "Please enter a valid Method Type");

            if (!model.ResponseSelect.Equals("Other") && !HttpResponseTypes.Where(rt => rt.Value.Equals(model.ResponseSelect)).Any())
                ModelState.AddModelError(nameof(model.Response), "Please enter a valid response type");

            if (ModelState.IsValid)
            {
                if (model.Method == "GET")
                {
                    ValidateGetModel(model);
                }
                else
                {
                    switch (model.PostType)
                    {
                        case "Form":
                            model.PostData = model.FormInputData;

                            if (model.SelectedForm != "Other")
                                model.FormId = model.SelectedForm;

                            break;
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                TestEditModel Result = CreateTestEditModel(configuration);
                Result.IsNew = model.IsNew;
                Result.Name = model.Name;
                Result.Route = model.Route;
                Result.Method = model.Method;
                Result.Response = model.Response;
                Result.Position = model.Position;
                Result.FormInputData = model.FormInputData;
                Result.PostData = model.PostData;
                Result.ResponseData = model.ResponseData;
                Result.SubmitResponseData = model.SubmitResponseData;
                Result.FormId = model.FormId;
                Result.Parameters = model.Parameters;
                Result.ResponseUrl = model.ResponseUrl;
                Result.ResponseSelect = model.ResponseSelect;
                Result.PostType = model.PostType;

                return Result;
            }

            if (_testConfigurationProvider.ConfigurationExists(configuration.Name, configuration.UniqueId))
            {
                if (model.IsNew)
                {
                    WebSmokeTestItem newtest = new WebSmokeTestItem()
                    {
                        FormId = GetNonNullString(model.FormId),
                        InputData = GetNonNullString(model.PostData),
                        PostType = (PostType)Enum.Parse(typeof(PostType), model.PostType),
                        Method = model.Method,
                        Name = model.Name,
                        Parameters = GetNonNullString(model.Parameters),
                        Position = model.Position,
                        Response = model.Response,
                        ResponseData = GetNonNullString(model.ResponseData).Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList(),
                        ResponseUrl = GetNonNullString(model.ResponseUrl),
                        Route = GetNonNullString(model.Route),
                        SubmitResponseData = GetNonNullString(model.SubmitResponseData).Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList(),
                    };

                    configuration.Tests.Add(newtest);
                }
                else
                {
                    foreach (WebSmokeTestItem test in configuration.Tests)
                    {
                        string uniqueTestId = Report.GenerateTestHash(test);

                        if (model.TestId.Equals(uniqueTestId))
                        {
                            test.FormId = GetNonNullString(model.SelectedForm);
                            test.InputData = GetNonNullString(model.PostData);
                            test.PostType = (PostType)Enum.Parse(typeof(PostType), model.PostType);
                            test.Method = model.Method;
                            test.Name = model.Name;
                            test.Parameters = GetNonNullString(model.Parameters);
                            test.Position = model.Position;
                            test.Response = model.Response;
                            test.ResponseData = GetNonNullString(model.ResponseData).Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();
                            test.ResponseUrl = GetNonNullString(model.ResponseUrl);
                            test.Route = GetNonNullString(model.Route);
                            test.SubmitResponseData = GetNonNullString(model.SubmitResponseData).Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();

                            break;
                        }
                    }
                }


                _testConfigurationProvider.SaveConfiguration(configuration);
            }
            else
            {
                throw new InvalidOperationException("Could not find test configuration");
            }

            return null;
        }

        private string GetNonNullString(string s)
        {
            if (String.IsNullOrEmpty(s))
                return String.Empty;

            return s;
        }

        private void ValidateGetModel(TestEditModel model)
        {
            if (!Uri.TryCreate(model.Route, UriKind.Relative, out _))
                ModelState.AddModelError(nameof(model.Route), "Please enter a valid, relative route");
        }

        private TestEditModel EditTestEditModel(TestConfiguration configuration, WebSmokeTestItem smokeTest)
        {
            TestEditModel Result = CreateTestEditModel(configuration);

            Result.IsNew = false;

            Result.FormId = GetNonNullString(smokeTest.FormId);
            Result.PostData = GetNonNullString(smokeTest.InputData);
            Result.Method = smokeTest.Method;
            Result.Name = smokeTest.Name;
            Result.PostType = smokeTest.PostType.ToString();
            Result.Parameters = GetNonNullString(smokeTest.Parameters);
            Result.Position = smokeTest.Position;
            Result.Response = smokeTest.Response;
            Result.ResponseUrl = GetNonNullString(smokeTest.ResponseUrl);
            Result.Route = GetNonNullString(smokeTest.Route);
            Result.TestId = Report.GenerateTestHash(smokeTest);

            if (Result.PostType == PostType.Form.ToString())
            {
                Result.SelectedForm = Result.FormId;
            }

            if (Result.Method == "POST" && Result.PostType == PostType.Form.ToString())
            {
                Result.FormInputData = Result.PostData;
                Result.PostData = String.Empty;
            }

            if (smokeTest.SubmitResponseData != null)
                Result.SubmitResponseData = String.Join('\n', smokeTest.SubmitResponseData);

            if (smokeTest.ResponseData != null)
                Result.ResponseData = String.Join('\n', smokeTest.ResponseData);

            return Result;
        }

        private TestEditModel CreateTestEditModel(TestConfiguration configuration)
        {
            TestEditModel Result = new TestEditModel(GetModelData(), configuration.UniqueId);

            TestSchedule testSchedule = _scheduleHelper.Schedules.Where(s => s.TestId == configuration.UniqueId).FirstOrDefault();

            if (testSchedule != null)
            {
                Report latestReport = _reportHelper.MostRecentReport(testSchedule.UniqueId);

                if (latestReport != null)
                {
                    foreach (FormReport form in latestReport.Forms)
                    {
                        if (!string.IsNullOrEmpty(form.Id))
                            Result.FormIdList.Add(new NameValueModel(form.Id));
                    }

                    if (Result.FormIdList.Count > 0)
                    {
                        Result.FormIdList.Add(new NameValueModel("Other"));
                        Result.FormId = Result.FormIdList[0].Value;
                    }
                }
            }

            return Result;
        }

        #endregion Private Methods

        #region Static Properties

        public static List<NameValueModel> HttpMethodTypes
        {
            get
            {
                return new List<NameValueModel>()
                {
                    new NameValueModel("GET"),
                    new NameValueModel("POST"),
                };
            }
        }

        public static List<NameValueModel> HttpResponseTypes
        {
            get
            {
                return new List<NameValueModel>()
                {
                    new NameValueModel("Continue - 100", "100"),
                    new NameValueModel("Switching Protocols - 101", "101"),
                    new NameValueModel("Processing - 102", "102"),
                    new NameValueModel("Early Hints - 103", "103"),
                    new NameValueModel("OK - 200", "200"),
                    new NameValueModel("Created - 201", "201"),
                    new NameValueModel("Accepted - 202", "202"),
                    new NameValueModel("Non-Authoritative Information - 203", "203"),
                    new NameValueModel("No Content - 204", "204"),
                    new NameValueModel("Reset Content - 205", "205"),
                    new NameValueModel("Partial Content - 206", "206"),
                    new NameValueModel("Multi-Status - 207", "207"),
                    new NameValueModel("Already Reported - 208", "208"),
                    new NameValueModel("IM Used - 226", "226"),
                    new NameValueModel("Multiple Choices - 300", "300"),
                    new NameValueModel("Moved Permanently - 301", "301"),
                    new NameValueModel("Found - 302", "302"),
                    new NameValueModel("See Other - 303", "303"),
                    new NameValueModel("Not Modified - 304", "304"),
                    new NameValueModel("Use Proxy - 305", "305"),
                    new NameValueModel("Switch Proxy - 306", "306"),
                    new NameValueModel("Temporary Redirect - 307", "307"),
                    new NameValueModel("Permanent Redirect - 308", "308"),
                    new NameValueModel("Bad Request - 400", "400"),
                    new NameValueModel("Unauthorized - 401", "401"),
                    new NameValueModel("Payment Required - 402", "402"),
                    new NameValueModel("Forbidden - 403", "403"),
                    new NameValueModel("Not Found - 404", "404"),
                    new NameValueModel("Method Not Allowed - 405", "405"),
                    new NameValueModel("Not Acceptable - 406", "406"),
                    new NameValueModel("Proxy Authentication Required - 407", "407"),
                    new NameValueModel("Request Timeout - 408", "408"),
                    new NameValueModel("Conflict - 409", "409"),
                    new NameValueModel("Gone - 410", "410"),
                    new NameValueModel("Length Required - 411", "411"),
                    new NameValueModel("Precondition Failed - 412", "412"),
                    new NameValueModel("Payload Too Large - 413", "413"),
                    new NameValueModel("URI Too Long - 414", "414"),
                    new NameValueModel("Unsupported Media Type - 415", "415"),
                    new NameValueModel("Range Not Satisfiable - 416", "416"),
                    new NameValueModel("Expectation Failure - 417", "417"),
                    new NameValueModel("I'm a teapot - 418", "418"),
                    new NameValueModel("Misdirected Request - 421", "421"),
                    new NameValueModel("Unprocessable Entity - 422", "422"),
                    new NameValueModel("Locked - 423", "423"),
                    new NameValueModel("Failed Dependency - 424", "424"),
                    new NameValueModel("Too Early - 425", "425"),
                    new NameValueModel("Upgrade Required - 426", "426"),
                    new NameValueModel("Precondition Required - 428", "428"),
                    new NameValueModel("Too Many Requests", "429"),
                    new NameValueModel("Request Header Fields Too Large", "431"),
                    new NameValueModel("Unavailable For Legal Reasons", "451"),
                    new NameValueModel("Internal Server Error - 500", "500"),
                    new NameValueModel("Not Implemented - 501", "501"),
                    new NameValueModel("Bad Gateway - 502", "502"),
                    new NameValueModel("Service Unavailable - 503", "503"),
                    new NameValueModel("Gateway Timeout - 504", "504"),
                    new NameValueModel("HTTP Version Not Supported - 505", "505"),
                    new NameValueModel("Variant Also Negotiates - 506", "506"),
                    new NameValueModel("Insufficient Storage - 507", "507"),
                    new NameValueModel("Loop Detected - 508", "508"),
                    new NameValueModel("Not Extended - 510", "510"),
                    new NameValueModel("Network Authentication Required - 511", "511"),
                    new NameValueModel("Other"),
                };
            }
        }

        public static List<NameValueModel> HttpPostTypes
        {
            get
            {
                List<NameValueModel> Result = new List<NameValueModel>();

                foreach (string postType in Enum.GetNames(typeof(PostType)))
                    Result.Add(new NameValueModel(postType));

                return Result;
            }
        }

        #endregion Static Properties

        #region Private Methods


        #endregion Private Methods
    }
}
