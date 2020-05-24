using System.Collections.Generic;

using SharedPluginFeatures;

namespace SmokeTest.Scheduler.Models
{
    public class ScheduleListViewModel : BaseModel
    {
        public ScheduleListViewModel(in BaseModelData baseModelData, in List<ScheduleModel> schedules)
            : base(baseModelData)
        {
            Schedules = schedules;
        }

        public List<ScheduleModel> Schedules { get; private set; }
    }
}
