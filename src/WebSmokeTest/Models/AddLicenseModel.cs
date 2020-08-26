using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SharedPluginFeatures;

namespace SmokeTest.Models
{
    public sealed class AddLicenseModel : BaseModel
    {
        #region Constructors

        public AddLicenseModel()
        {

        }

        public AddLicenseModel(in BaseModelData modelData)
            : base (modelData)
        {

        }

        #endregion Constructors

        #region Properties

        [Required(ErrorMessage = "Please select a license file.")]
        [Display(Name = "Browse File")]
        public IFormFile LicenseFile { get; set; }

        #endregion Properties
    }
}
