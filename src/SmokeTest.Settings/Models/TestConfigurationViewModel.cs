using System;
using System.ComponentModel.DataAnnotations;

using SharedPluginFeatures;

namespace SmokeTest.Settings.Models
{
    public class TestConfigurationViewModel : BaseModel
    {
        public TestConfigurationViewModel()
        {

        }

        public TestConfigurationViewModel(BaseModelData modelData)
            : base(modelData)
        {

        }

        [Required(ErrorMessage = nameof(Languages.LanguageStrings.Name))]
        [StringLength(40, MinimumLength = 3)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please enter the Url of the website to be tested")]
        public string Url { get; set; }

        [Range(minimum: 1, maximum: 20)]
        public int CrawlDepth { get; set; }

        public bool CheckImages { get; set; }

        [StringLength(120, MinimumLength = 10)]
        public string UserAgent { get; set; }

        [Range(minimum: 50, maximum: 2500)]
        public int MillisecondsBetweenRequests { get; set; }

        public int MaximumPages { get; set; }

        public string Headers { get; set; }

        public string AdditionalUrls { get; set; }

        public bool ClearImageData { get; set; }

        public bool ClearHtmlData { get; set; }

        public string BasicAuthUsername { get; set; }

        public string BasicAuthPassword { get; set; }

        public string UniqueId { get; set; }
    }
}
