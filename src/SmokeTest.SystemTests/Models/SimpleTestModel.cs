
using SharedPluginFeatures;

namespace SmokeTest.SystemTests.Models
{
    public class SimpleTestModel : BaseModel
    {
        public SimpleTestModel()
            : base()
        {

        }

        public SimpleTestModel(BaseModelData modelData)
            : base(modelData)
        {

        }

        public string Username { get; set; }

        public string Password { get; set; }
    }
}
