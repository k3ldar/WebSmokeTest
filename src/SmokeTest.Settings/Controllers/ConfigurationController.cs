﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Differencing;
using Shared.Classes;
using SharedPluginFeatures;

using SmokeTest.Settings.Models;
using SmokeTest.Shared;

namespace SmokeTest.Settings.Controllers
{
    [LoggedIn]
    public class ConfigurationController : BaseController
    {
        public const string Name = "Configuration";
        public const string DefaultUseragent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; .NET CLR 2.0.50727) SmokeTest/v1.0";

        #region Private Members

        private readonly ITestConfigurationProvider _testConfigurationProvider;

        #endregion Private Members

        #region Constructors

        public ConfigurationController(ITestConfigurationProvider testConfigurationProvider)
        {
            _testConfigurationProvider = testConfigurationProvider ?? throw new ArgumentNullException(nameof(testConfigurationProvider));
        }

        #endregion Constructors

        #region Public Action Methods

        [Breadcrumb(Name)]
        [Authorize(Policy = STConsts.PolicyViewConfigurations)]
        public IActionResult Index()
        {
            ViewConfigurationViewModel model = new ViewConfigurationViewModel(GetModelData());

            List<TestConfiguration> configurations = _testConfigurationProvider.Configurations;

            configurations.ForEach(c => model.Configurations.Add(new TestConfigurationViewDetailsModel(c.Name, c.UniqueId)));

            return View(model);
        }

        [HttpGet]
        [Authorize(Policy = STConsts.PolicyViewConfigurations)]
        [Breadcrumb(nameof(New), Name, nameof(Index))]

        public IActionResult New()
        {
            return base.View(CreateTestConfigurationViewModel((TestConfigurationViewModel)null));
        }

        [HttpPost]
        [Authorize(Policy = STConsts.PolicyManageConfigurations)]
        public IActionResult New(TestConfigurationViewModel model)
        {
            ValidateTestConfigurationViewModel(model, out string[] headers, out List<Uri> additionalUrls);

            if (ModelState.IsValid)
            {
                NVPCodec headerCodec = new NVPCodec();
                headerCodec.Decode(String.Join("&", headers));
                List<string> urlList = new List<string>();
                additionalUrls.ForEach(uri => urlList.Add(uri.ToString()));

                if (!String.IsNullOrWhiteSpace(model.BasicAuthUsername) && !String.IsNullOrWhiteSpace(model.BasicAuthPassword))
                {
                    string encoded = Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1")
                        .GetBytes($"{model.BasicAuthUsername}:{model.BasicAuthPassword}"));
                    headerCodec.Add("Authorization", "Basic " + encoded);
                }

                _testConfigurationProvider.SaveConfiguration(model.Name, model.Url, model.CrawlDepth,
                    model.MaximumPages, model.MillisecondsBetweenRequests, model.UserAgent,
                    model.UniqueId, model.CheckImages, model.ClearHtmlData, model.ClearImageData,
                    urlList, headerCodec);

                return RedirectToAction(nameof(Index));
            }

            return View(CreateTestConfigurationViewModel(model));
        }

        [HttpGet]
        [Authorize(Policy = STConsts.PolicyManageConfigurations)]
        [Breadcrumb(nameof(Edit), Name, nameof(Index), HasParams = true)]
        public IActionResult Edit(string id)
        {
            if (String.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));

            TestConfiguration configuration = _testConfigurationProvider.Configurations.Where(c => c.UniqueId.Equals(id)).FirstOrDefault();

            if (configuration == null)
            {
                return NotFound();
            }

            return View(CreateTestConfigurationViewModel(configuration));
        }

        [HttpPost]
        [Authorize(Policy = STConsts.PolicyManageConfigurations)]
        public IActionResult Edit(TestConfigurationViewModel model)
        {
            ValidateTestConfigurationViewModel(model, out string[] headers, out List<Uri> additionalUrls);

            if (ModelState.IsValid)
            {
                NVPCodec headerCodec = new NVPCodec();
                headerCodec.Decode(String.Join("&", headers));
                List<string> urlList = new List<string>();
                additionalUrls.ForEach(uri => urlList.Add(uri.ToString()));

                if (!String.IsNullOrWhiteSpace(model.BasicAuthUsername) && !String.IsNullOrWhiteSpace(model.BasicAuthPassword))
                {
                    string encoded = Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1")
                        .GetBytes($"{model.BasicAuthUsername}:{model.BasicAuthPassword}"));
                    headerCodec.Add("Authorization", "Basic " + encoded);
                }

                _testConfigurationProvider.SaveConfiguration(model.Name, model.Url, model.CrawlDepth,
                    model.MaximumPages, model.MillisecondsBetweenRequests, model.UserAgent,
                    model.UniqueId, model.CheckImages, model.ClearHtmlData, model.ClearImageData,
                    urlList, headerCodec);

                return RedirectToAction(nameof(Index));
            }

            return View(CreateTestConfigurationViewModel(model));
        }

        #endregion Public Action Methods

        #region Private Methods

        private void SaveConfiguration()
        {

        }

        private TestConfigurationViewModel CreateTestConfigurationViewModel(TestConfigurationViewModel model)
        {
            return new TestConfigurationViewModel(GetModelData())
            {
                Name = model == null ? String.Empty : model.Name,
                Url = model == null ? String.Empty : model.Url,
                MillisecondsBetweenRequests = model == null ? 250 : model.MillisecondsBetweenRequests,
                UniqueId = model == null ? DateTime.Now.Ticks.ToString("X") : model.UniqueId,
                UserAgent = model == null ? DefaultUseragent : model.UserAgent,
                CrawlDepth = model == null ? 10 : model.CrawlDepth,
                MaximumPages = model == null ? 10000 : model.MaximumPages,
                CheckImages = model == null ? true : model.CheckImages,
                ClearHtmlData = model == null ? true : model.ClearHtmlData,
                ClearImageData = model == null ? true : model.ClearImageData,
                AdditionalUrls = model == null ? String.Empty : model.AdditionalUrls,
                Headers = model == null ? String.Empty : model.Headers,
                BasicAuthUsername = model == null ? String.Empty : model.BasicAuthUsername,
                BasicAuthPassword = model == null ? String.Empty : model.BasicAuthPassword,
            };
        }

        private TestConfigurationViewModel CreateTestConfigurationViewModel(TestConfiguration testConfiguration)
        {
            NVPCodec headers = new NVPCodec();
            headers.Decode(testConfiguration.Headers);

            string basicAuthName = String.Empty;
            string basicAuthPassword = String.Empty;

            if (headers.HasKeys() && headers.AllKeys.Contains("Authorization"))
            {
                try
                {
                    string authData = System.Text.Encoding.GetEncoding("ISO-8859-1").GetString(
                        Convert.FromBase64String(headers["Authorization"].Substring(6)));
                    string[] authParts = authData.Split(':', StringSplitOptions.RemoveEmptyEntries);

                    if (authParts.Length == 2)
                    {
                        basicAuthName = authParts[0];
                        basicAuthPassword = authParts[1];
                    }

                    headers.Remove("Authorization");
                }
                catch (FormatException)
                {

                }

            }

            return new TestConfigurationViewModel(GetModelData())
            {
                Name = testConfiguration.Name,
                Url = testConfiguration.Url,
                MillisecondsBetweenRequests = testConfiguration.MillisecondsBetweenRequests,
                UniqueId = testConfiguration.UniqueId,
                UserAgent = testConfiguration.UserAgent,
                CrawlDepth = testConfiguration.CrawlDepth,
                MaximumPages = testConfiguration.MaximumPages < 1 || testConfiguration.MaximumPages == Int32.MaxValue ? -1 : testConfiguration.MaximumPages,
                CheckImages = testConfiguration.CheckImages,
                ClearHtmlData = testConfiguration.ClearHtmlData,
                ClearImageData = testConfiguration.ClearImageData,
                AdditionalUrls = String.Join("\r\n", testConfiguration.AdditionalUrls.Split(';')),
                Headers = String.Join("\r\n", headers.Encode().Split("&")),
                BasicAuthUsername = basicAuthName,
                BasicAuthPassword = basicAuthPassword,
            };
        }

        private void ValidateTestConfigurationViewModel(TestConfigurationViewModel model, out string[] headers, out List<Uri> additionalUrls)
        {
            if (String.IsNullOrWhiteSpace(model.Url))
                ModelState.AddModelError(nameof(model.Url), "Url can not be empty");

            if (!ModelState.IsValid && !Uri.TryCreate(model.Url, UriKind.Absolute, out Uri _))
                ModelState.AddModelError(nameof(model.Url), "Url must be a valid web site address");

            if (_testConfigurationProvider.ConfigurationExists(model.Name))
                ModelState.AddModelError(String.Empty, $"A test configuration with the name {model.Name} already exists.");

            if (model.MaximumPages < 1)
                model.MaximumPages = Int32.MaxValue;

            if (!Uri.TryCreate(model.Url, UriKind.Absolute, out Uri _))
            {
                ModelState.AddModelError(nameof(model.Url), "Url must be an absolute url not a relative url");
            }

            if (!String.IsNullOrEmpty(model.BasicAuthUsername) && String.IsNullOrEmpty(model.BasicAuthPassword))
                ModelState.AddModelError(nameof(model.BasicAuthPassword), "You must specify a basic auth password if a basic auth user name is specified");

            if (!String.IsNullOrEmpty(model.Headers))
                headers = model.Headers.Replace("\r\n", "\n").Split('\n', StringSplitOptions.RemoveEmptyEntries);
            else
                headers = new string[] { };

            foreach (string header in headers)
            {
                if (!header.Contains("="))
                    ModelState.AddModelError(String.Empty, $"Header {header} must be in the form of name=value");
            }

            string[] urls;
            additionalUrls = new List<Uri>();

            if (!String.IsNullOrWhiteSpace(model.AdditionalUrls))
                urls = model.AdditionalUrls.Replace("\r\n", "\n").Split('\n', StringSplitOptions.RemoveEmptyEntries);
            else
                urls = new string[] { };

            foreach (string s in urls)
            {
                if (Uri.TryCreate(s, UriKind.Relative, out Uri uri))
                {
                    additionalUrls.Add(uri);
                }
                else
                {
                    ModelState.AddModelError(String.Empty, $"{s} is must be a relative url");
                }
            }
        }

        #endregion Private Methods
    }
}
