using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharedPluginFeatures;
using SmokeTest.Reports.Controllers;

namespace SmokeTest.Reports.Models
{
    public sealed class TestRunProgressModel : BaseModel
    {
        public TestRunProgressModel(in BaseModelData baseModelData, in long uniqueId)
            : base (baseModelData)
        {
            Url = $"/{ReportsController.Name}/{nameof(ReportsController.Running)}/{uniqueId}/";
            AutoUpdatePage = true;
            AutoUpdateFrequency = 5000;
        }

        public string Url { get; private set; }

        public bool AutoUpdatePage { get; private set; }

        public int AutoUpdateFrequency { get; private set; }
    }
}
