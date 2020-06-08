using System;
using System.Collections.Generic;

using SmokeTest.Shared;
using SmokeTest.Shared.Classes;

namespace SmokeTest.UnitTests.Mocks
{
    class ScheduleHelperMock : IScheduleHelper
    {
        public List<TestSchedule> Schedules => throw new NotImplementedException();

        public bool Create(in string name, in string testId, in DateTime startTime)
        {
            throw new NotImplementedException();
        }

        public bool Create(in string name, in string testId, in DateTime startTime, in DateTime? expires, in int frequency, in ScheduleType scheduleType)
        {
            throw new NotImplementedException();
        }

        public bool Create(in string name, in string testId, in DateTime startTime, in DateTime? expires, in int frequency, in ScheduleDay scheduleDay)
        {
            throw new NotImplementedException();
        }

        public bool Delete(in TestSchedule testSchedule)
        {
            throw new NotImplementedException();
        }

        public bool Update(in TestSchedule testSchedule)
        {
            throw new NotImplementedException();
        }
    }
}
