using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Http;

using SharedPluginFeatures;

using SmokeTest.Configuration.Controllers;

namespace SmokeTest.Configuration.Models
{
    public sealed class ImportTestModel : BaseModel
    {
        #region Constructors

        public ImportTestModel()
        {
            Files = new List<IFormFile>();
        }

        public ImportTestModel(in BaseModelData modelData, in string configurationId, in string uniqueId, in string testName)
            : base(modelData)
        {
            if (String.IsNullOrEmpty(uniqueId))
                throw new ArgumentNullException(nameof(uniqueId));

            if (String.IsNullOrEmpty(testName))
                throw new ArgumentNullException(nameof(testName));

            UniqueId = uniqueId;
            TestName = testName;

            Breadcrumbs.Add(new BreadcrumbItem(ConfigurationController.Name, "/Configurations/Index", false));
            Breadcrumbs.Add(new BreadcrumbItem(nameof(ConfigurationController.Edit), $"/Configuration/Edit/{configurationId}/", false));
            Breadcrumbs.Add(new BreadcrumbItem(nameof(ConfigurationController.Import), $"/Configuration/Import/{configurationId}/", false));
        }

        public ImportTestModel(in BaseModelData modelData, in string configurationId, in string uniqueId, in string testName,
            in Dictionary<string, string> importResults)
            : this(modelData, configurationId, uniqueId, testName)
        {
            ImportResults = importResults ?? throw new ArgumentNullException(nameof(importResults));
        }

        #endregion Constructors

        #region Properties

        public string UniqueId { get; set; }

        public string TestName { get; set; }

        [Required(ErrorMessage = "Please select file.")]
        [Display(Name = "Browse File")]
        public List<IFormFile> Files { get; set; }

        public Dictionary<string, string> ImportResults { get; private set; }

        #endregion Properties
    }
}
