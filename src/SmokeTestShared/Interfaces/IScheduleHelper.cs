using System;
using System.Collections.Generic;

using SmokeTest.Shared.Classes;

namespace SmokeTest.Shared
{
    public interface IScheduleHelper
    {
        List<TestSchedule> Schedules { get; }

        bool Create(in string name, in string testId, in DateTime startTime);

        bool Create(in string name, in string testId, in DateTime startTime, in DateTime? expires, in int frequency);

        bool Create(in string name, in string testId, in DateTime startTime, in DateTime? expires, in int frequency, in ScheduleDay scheduleDay);

        bool Update(in TestSchedule testSchedule);
    }
}
