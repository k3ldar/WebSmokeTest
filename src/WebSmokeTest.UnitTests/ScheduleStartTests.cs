using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using PluginManager.Abstractions;

using SmokeTest.Classes;
using SmokeTest.Middleware;
using SmokeTest.Shared;
using SmokeTest.Shared.Classes;
using SmokeTest.Shared.Interfaces;

namespace SmokeTest.UnitTests
{
    [TestClass]
    public class ScheduleStartTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ScheduleHelper_Create_NullLogger_ThrowsException()
        {
            ISaveData saveData = new Mocks.SaveDataMock();
            ILoadData loadData = new Mocks.LoadDataMock();

            new ScheduleHelper(null, saveData, loadData);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ScheduleHelper_Create_NullSaveData_ThrowsException()
        {
            ILogger logger = new Logger();
            ILoadData loadData = new Mocks.LoadDataMock();
            new ScheduleHelper(logger, null, loadData);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ScheduleHelper_Create_NullLoadData_ThrowsException()
        {
            ILogger logger = new Logger();
            ISaveData saveData = new Mocks.SaveDataMock();
            new ScheduleHelper(logger, saveData, null);
        }

        [TestMethod]
        public void ScheduleHelper_Create_Success()
        {
            ILogger logger = new Logger();
            ISaveData saveData = new Mocks.SaveDataMock();
            ILoadData loadData = new Mocks.LoadDataMock();
            IScheduleHelper sut = new ScheduleHelper(logger, saveData, loadData);

            Assert.IsNotNull(sut);
        }

        [TestMethod]
        public void ScheduleHelper_Disabled_ExpireDateExceeded()
        {
            ILogger logger = new Logger();
            ISaveData saveData = new Mocks.SaveDataMock();
            ILoadData loadData = new Mocks.LoadDataMock();

            IScheduleHelper sut = new ScheduleHelper(logger, saveData, loadData);

            sut.Create("test", "123", DateTime.Now.AddDays(-1), DateTime.Now.AddHours(-1), 1);


            Assert.IsNotNull(sut);

            TestSchedule item = sut.Schedules.Where(s => s.TestId.Equals("123")).FirstOrDefault();

            Assert.IsNotNull(item);

            Assert.IsFalse(item.Enabled);
        }

        [TestMethod]
        public void ScheduleHelper_NextRun_ScheduledOnce()
        {
            ILogger logger = new Logger();
            ISaveData saveData = new Mocks.SaveDataMock();
            ILoadData loadData = new Mocks.LoadDataMock();

            IScheduleHelper sut = new ScheduleHelper(logger, saveData, loadData);

            DateTime runDate = DateTime.Now.AddDays(1);
            sut.Create("test", "123", runDate);


            Assert.IsNotNull(sut);

            TestSchedule item = sut.Schedules.Where(s => s.TestId.Equals("123")).FirstOrDefault();

            Assert.IsNotNull(item);

            Assert.IsTrue(item.Enabled);
            TimeSpan nextRun = item.NextRun();
            Assert.IsTrue(Convert.ToInt32(-nextRun.TotalMinutes) >= 1439);
            Assert.IsTrue(Convert.ToInt32(-nextRun.TotalMinutes) <= 1440);
        }

        [TestMethod]
        public void ScheduleHelper_NextRun_ScheduledOnce_NeverRan()
        {
            ILogger logger = new Logger();
            ISaveData saveData = new Mocks.SaveDataMock();
            ILoadData loadData = new Mocks.LoadDataMock();

            IScheduleHelper sut = new ScheduleHelper(logger, saveData, loadData);

            DateTime runDate = DateTime.Now.AddDays(1);
            sut.Create("test", "123", runDate);


            Assert.IsNotNull(sut);

            TestSchedule item = sut.Schedules.Where(s => s.TestId.Equals("123")).FirstOrDefault();

            Assert.IsNotNull(item);

            Assert.IsTrue(item.Enabled);

            TimeSpan nextRun = item.NextRun();
            Assert.IsTrue(nextRun.TotalMinutes > -1440);
            Assert.IsTrue(nextRun.TotalMinutes < -1439);
        }

        [TestMethod]
        public void ScheduleHelper_NextRun_ScheduledOnce_Ran23HoursAgo()
        {
            ILogger logger = new Logger();
            ISaveData saveData = new Mocks.SaveDataMock();
            ILoadData loadData = new Mocks.LoadDataMock();

            IScheduleHelper sut = new ScheduleHelper(logger, saveData, loadData);

            DateTime runDate = DateTime.Now.AddDays(-1);
            sut.Create("test", "123", runDate);


            Assert.IsNotNull(sut);

            TestSchedule item = sut.Schedules.Where(s => s.TestId.Equals("123")).FirstOrDefault();
            item.LastRun = DateTime.Now.AddHours(-23);

            Assert.IsNotNull(item);

            Assert.IsFalse(item.Enabled);
        }

        [TestMethod]
        public void ScheduleHelper_NextRun_ScheduledOnce_RunInOneHour()
        {
            ILogger logger = new Logger();
            ISaveData saveData = new Mocks.SaveDataMock();
            ILoadData loadData = new Mocks.LoadDataMock();

            IScheduleHelper sut = new ScheduleHelper(logger, saveData, loadData);

            DateTime runDate = DateTime.Now.AddHours(1);
            sut.Create("test", "123", runDate);


            Assert.IsNotNull(sut);

            TestSchedule item = sut.Schedules.Where(s => s.TestId.Equals("123")).FirstOrDefault();

            Assert.IsNotNull(item);

            Assert.IsTrue(item.Enabled);

            TimeSpan nextRun = item.NextRun();
            Assert.IsTrue(nextRun.TotalMinutes > -60);
            Assert.IsTrue(nextRun.TotalMinutes < -59);
        }

        [TestMethod]
        public void ScheduleHelper_NextRun_ScheduledOnce_ShouldHaveRanOneHourAgo()
        {
            ILogger logger = new Logger();
            ISaveData saveData = new Mocks.SaveDataMock();
            ILoadData loadData = new Mocks.LoadDataMock();

            IScheduleHelper sut = new ScheduleHelper(logger, saveData, loadData);

            DateTime runDate = DateTime.Now.AddHours(-1);
            sut.Create("test", "123", runDate);


            Assert.IsNotNull(sut);

            TestSchedule item = sut.Schedules.Where(s => s.TestId.Equals("123")).FirstOrDefault();

            Assert.IsNotNull(item);

            Assert.IsTrue(item.Enabled);

            TimeSpan nextRun = item.NextRun();
            Assert.IsTrue(nextRun.TotalMinutes > 60);
            Assert.IsTrue(nextRun.TotalMinutes < 61);
        }

        [TestMethod]
        public void ScheduleHelper_NextRun_ScheduledWeeklyEveryTuesday_OneHourTime()
        {
            ILogger logger = new Logger();
            ISaveData saveData = new Mocks.SaveDataMock();
            ILoadData loadData = new Mocks.LoadDataMock();

            IScheduleHelper sut = new ScheduleHelper(logger, saveData, loadData);

            DateTime runDate = DateTime.Now.AddDays(1);

            while (runDate.DayOfWeek != DayOfWeek.Tuesday)
                runDate = runDate.AddDays(1);

            runDate = runDate.Date.AddHours(17);

            sut.Create("test", "123", runDate, null, 1, ScheduleDay.Tuesday);


            Assert.IsNotNull(sut);

            TestSchedule item = sut.Schedules.Where(s => s.TestId.Equals("123")).FirstOrDefault();

            Assert.IsNotNull(item);

            Assert.AreEqual(ScheduleDay.Tuesday, item.ScheduleDays);
            Assert.AreEqual(ScheduleType.Weekly, item.ScheduleType);

            Assert.IsTrue(item.Enabled);

            TimeSpan nextRun = item.NextRun();
            DateTime nextRuntime = new DateTime(DateTime.Now.Ticks - nextRun.Ticks);

            nextRun = runDate - nextRuntime;
            Assert.IsTrue(nextRun.TotalSeconds < 2);
            Assert.IsTrue(nextRun.TotalSeconds > -2);
        }

        [TestMethod]
        public void ScheduleHelper_NextRun_ScheduledWeeklyEveryTuesday_OneWeekTime()
        {
            ILogger logger = new Logger();
            ISaveData saveData = new Mocks.SaveDataMock();
            ILoadData loadData = new Mocks.LoadDataMock();

            IScheduleHelper sut = new ScheduleHelper(logger, saveData, loadData);

            DateTime runDate = DateTime.Now.AddDays(1);

            while (runDate.DayOfWeek != DayOfWeek.Tuesday)
                runDate = runDate.AddDays(1);

            runDate = runDate.Date.AddHours(17);

            sut.Create("test", "123", runDate, null, 1, ScheduleDay.Tuesday);


            Assert.IsNotNull(sut);

            TestSchedule item = sut.Schedules.Where(s => s.TestId.Equals("123")).FirstOrDefault();

            Assert.IsNotNull(item);

            item.LastRun = runDate;

            Assert.AreEqual(ScheduleDay.Tuesday, item.ScheduleDays);
            Assert.AreEqual(ScheduleType.Weekly, item.ScheduleType);

            Assert.IsTrue(item.Enabled);

            TimeSpan nextRun = item.NextRun();
            DateTime nextRuntime = new DateTime(DateTime.Now.Ticks - nextRun.Ticks);

            nextRun = runDate - nextRuntime;
            Assert.AreEqual(0, nextRun.Minutes);
            Assert.AreEqual(-7, nextRun.Days);
        }

        [TestMethod]
        public void ScheduleHelper_NextRun_ScheduledWeeklyEveryTuesdayAndThursday_2DaysTime()
        {
            ILogger logger = new Logger();
            ISaveData saveData = new Mocks.SaveDataMock();
            ILoadData loadData = new Mocks.LoadDataMock();

            IScheduleHelper sut = new ScheduleHelper(logger, saveData, loadData);

            DateTime runDate = DateTime.Now.AddDays(1);

            while (runDate.DayOfWeek != DayOfWeek.Tuesday)
                runDate = runDate.AddDays(1);

            runDate = runDate.Date.AddHours(17);

            sut.Create("test", "123", runDate, null, 1, ScheduleDay.Tuesday | ScheduleDay.Thursday);


            Assert.IsNotNull(sut);

            TestSchedule item = sut.Schedules.Where(s => s.TestId.Equals("123")).FirstOrDefault();

            Assert.IsNotNull(item);

            item.LastRun = runDate;

            Assert.AreEqual(ScheduleDay.Tuesday | ScheduleDay.Thursday, item.ScheduleDays);
            Assert.AreEqual(ScheduleType.Weekly, item.ScheduleType);

            Assert.IsTrue(item.Enabled);

            TimeSpan nextRun = item.NextRun();
            DateTime nextRuntime = new DateTime(DateTime.Now.Ticks - nextRun.Ticks);

            nextRun = runDate - nextRuntime;
            Assert.AreEqual(0, nextRun.Minutes);
            Assert.AreEqual(-2, nextRun.Days);
        }

        //[TestMethod]
        //public void ScheduleHelper_NextRun_ScheduledMonthly_2DaysTime()
        //{
        //    ILogger logger = new Logger();
        //    ISaveData saveData = new Mocks.SaveDataMock();
        //    ILoadData loadData = new Mocks.LoadDataMock();

        //    IScheduleHelper sut = new ScheduleHelper(logger, saveData, loadData);

        //    DateTime runDate = DateTime.Now.Date.AddMonths(-1).AddHours(6);
            
        //    sut.Create("test", "123", runDate, null, 1);


        //    Assert.IsNotNull(sut);

        //    TestSchedule item = sut.Schedules.Where(s => s.TestId.Equals("123")).FirstOrDefault();

        //    Assert.IsNotNull(item);

        //    item.LastRun = runDate;

        //    Assert.AreEqual(ScheduleDay.NotSet, item.ScheduleDays);
        //    Assert.AreEqual(ScheduleType.Monthly, item.ScheduleType);

        //    Assert.IsTrue(item.Enabled);

        //    TimeSpan nextRun = item.NextRun();
        //    DateTime nextRuntime = new DateTime(-nextRun.Ticks);

        //    nextRun = runDate - nextRuntime;
        //    Assert.AreEqual(0, nextRun.Minutes);
        //    Assert.AreEqual(-2, nextRun.Days);
        //}
    }
}
