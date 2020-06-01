using System;
using System.ComponentModel.DataAnnotations;
using SharedPluginFeatures;
using SmokeTest.Shared;

namespace SmokeTest.Scheduler.Models
{
    public class ScheduleModel : BaseModel
    {
        #region Constructors

        public ScheduleModel()
        {

        }

        public ScheduleModel(in BaseModelData baseModelData)
            : base(baseModelData)
        {

        }

        public ScheduleModel(in long uniqueId, in string name, in string testId, 
            in ScheduleType scheduleType, in DateTime? expires, in bool enabled)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            if (String.IsNullOrEmpty(testId))
                throw new ArgumentNullException(nameof(testId));

            UniqueId = uniqueId;
            Name = name;
            TestId = testId;
            ScheduleType = scheduleType.ToString();
            Expires = expires;
            Enabled = enabled;
        }

        #endregion Constructors

        #region Properties

        public long UniqueId { get; set; }

        public string Name { get; set; }

        public string ScheduleType { get; set; }

        public DateTime StartTime { get; set; }

        [Display(Name = "Test Name")]
        public string TestId { get; set; }

        public string TestName { get; set; }

        public int Frequency { get; set; }

        public DateTime? Expires { get; set; }

        public bool DayMonday { get; set; }

        public bool DayTuesday { get; set; }

        public bool DayWednesday { get; set; }

        public bool DayThursday { get; set; }

        public bool DayFriday { get; set; }

        public bool DaySaturday { get; set; }

        public bool DaySunday { get; set; }

        public bool Enabled { get; private set; }

        #endregion Properties
    }
}
