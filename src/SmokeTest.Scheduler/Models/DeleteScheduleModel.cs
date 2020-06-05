using SharedPluginFeatures;

namespace SmokeTest.Scheduler.Models
{
    public sealed class DeleteScheduleModel : BaseModel
    {
        public DeleteScheduleModel()
        {

        }

        public DeleteScheduleModel(in BaseModelData modelData, in long uniqueId)
            : base(modelData)
        {
            UniqueId = uniqueId;
        }


        public long UniqueId { get; set; }

        public string Confirm { get; set; }
    }
}
