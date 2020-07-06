using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using SharedPluginFeatures;

using SmokeTest.Shared;

namespace SmokeTest.Settings.Models
{
    public class TestEditModel : BaseModel
    {
        #region Constructors

        public TestEditModel()
        {

        }

        public TestEditModel(BaseModelData modelData, in string testConfigurationId)
            : base(modelData)
        {
            if (String.IsNullOrEmpty(testConfigurationId))
                throw new ArgumentNullException(nameof(testConfigurationId));

            FormIdList = new List<NameValueModel>();
            IsNew = true;
            Method = "POST";
            TestConfigurationId = testConfigurationId;
            Position = 5000;
            Response = 200;
            ResponseSelect = "200";
            PostType = "Json";
        }

        #endregion Constructors

        #region Properties

        public string TestConfigurationId { get; set; }

        public bool IsNew { get; set; }

        [Required(ErrorMessage = "Please enter the name of the test")]
        [StringLength(40, MinimumLength = 3)]
        [DisplayName(nameof(Languages.LanguageStrings.Name))]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please enter the Url for the test")]
        [StringLength(500, MinimumLength = 1)]
        [DisplayName("Url")]
        public string Route { get; set; }

        [DisplayName("Http Method")]
        public string Method { get; set; }

        [DisplayName("Response")]
        public int Response { get; set; }

        [Range(minimum: Int32.MinValue, maximum: Int32.MaxValue)]
        public int Position { get; set; }

        [DisplayName("Form Input Data")]
        public string FormInputData { get; set; }

        public string SelectedForm { get; set; }

        [DisplayName("Post Data")]
        public string PostData { get; set; }

        public string ResponseData { get; set; }

        public string SubmitResponseData { get; set; }

        public string FormId { get; set; }

        public string Parameters { get; set; }

        public string ResponseUrl { get; set; }

        public string ResponseSelect { get; set; }

        public string PostType { get; set; }

        public List<NameValueModel> FormIdList { get; set; }

        public string TestId { get; set; }

        #endregion Properties
    }
}
