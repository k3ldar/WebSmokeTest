﻿using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using SharedPluginFeatures;

namespace SmokeTest.Settings.Models
{
    public class TestConfigurationViewModel : BaseModel
    {
        #region Constructors

        public TestConfigurationViewModel()
        {

        }

        public TestConfigurationViewModel(BaseModelData modelData)
            : base(modelData)
        {

        }

        #endregion Constructors

        #region Properties

        [Required(ErrorMessage = nameof(Languages.LanguageStrings.Name))]
        [StringLength(40, MinimumLength = 3)]
        [DisplayName("Configuration name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please enter the Url of the website to be tested")]
        [DisplayName("Url of website to be scanned")]
        public string Url { get; set; }

        [Range(minimum: 1, maximum: 20)]
        [DisplayName("Crawl depth")]
        public int CrawlDepth { get; set; }

        [DisplayName("Check images")]
        public bool CheckImages { get; set; }

        [StringLength(120, MinimumLength = 10)]
        [DisplayName("User agent to be sent with each request")]
        public string UserAgent { get; set; }

        [Range(minimum: 50, maximum: 2500)]
        [DisplayName("Time between requests (ms)")]
        public int MillisecondsBetweenRequests { get; set; }

        [DisplayName("Maximum pages to scan")]
        public int MaximumPages { get; set; }

        [DisplayName("Header values to be sent to the website with each request")]
        public string Headers { get; set; }

        [DisplayName("Additional, relative, Urls to be searched within the site")]
        public string AdditionalUrls { get; set; }

        [DisplayName("Clear image data")]
        public bool ClearImageData { get; set; }

        [DisplayName("Clear Html data")]
        public bool ClearHtmlData { get; set; }

        [DisplayName("Basic authentication username")]
        public string BasicAuthUsername { get; set; }

        [DisplayName("Basic authentication password")]
        public string BasicAuthPassword { get; set; }

        [DisplayName("Unique site id")]
        public string UniqueId { get; set; }

        [Range(50, 2000)]
        [Required(ErrorMessage = "Please specify a value between 50 and 2000 milliseconds")]
        public int MinimumLoadTime { get; set; }

        public bool SiteScan { get; set; }

        #endregion Properties
    }
}
