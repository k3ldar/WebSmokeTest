using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SharedPluginFeatures;

using SmokeTest.Scheduler.Models;
using SmokeTest.Shared;
using SmokeTest.Shared.Classes;

namespace SmokeTest.Scheduler.Controllers
{
    public class ScheduleController : BaseController
    {
        #region Constants

        public const string Name = "Schedule";

        #endregion Constants

        #region Private Members

        private readonly IScheduleHelper _scheduleHelper;
        private readonly ITestRunManager _testRunManager;
        private readonly IReportHelper _reportHelper;
        private static ITestConfigurationProvider _testConfigurationProvider;
        private readonly IIdManager _idManager;

        #endregion Private Members

        #region Constructors

        public ScheduleController(IScheduleHelper scheduleHelper,
            ITestConfigurationProvider testConfigurationProvider,
            ITestRunManager testRunManager,
            IReportHelper reportHelper,
            IIdManager idManager)
        {
            _scheduleHelper = scheduleHelper ?? throw new ArgumentNullException(nameof(scheduleHelper));
            _testRunManager = testRunManager ?? throw new ArgumentNullException(nameof(testRunManager));
            _reportHelper = reportHelper ?? throw new ArgumentNullException(nameof(reportHelper));
            _idManager = idManager ?? throw new ArgumentNullException(nameof(idManager));

            if (_testConfigurationProvider == null)
            {
                _testConfigurationProvider = testConfigurationProvider ?? throw new ArgumentNullException(nameof(testConfigurationProvider));
            }
        }

        #endregion Constructors

        #region Public Action Methods

        [HttpGet]
        [Breadcrumb(Name)]
        public IActionResult Index()
        {
            List<ScheduleModel> schedules = new List<ScheduleModel>();
            _scheduleHelper.Schedules.ForEach(s => schedules.Add(new ScheduleModel(s.UniqueId, s.Name, s.TestId, s.ScheduleType, s.Expires, s.Enabled)));
            schedules.ForEach(s => s.TestName = _testConfigurationProvider.Configurations.Where(tc => tc.UniqueId.Equals(s.TestId)).First().Name);

            ScheduleListViewModel model = new ScheduleListViewModel(GetModelData(), schedules);

            return View(model);
        }

        [HttpGet]
        [Breadcrumb(nameof(New), Name, nameof(Index))]
        [Authorize(Policy = STConsts.PolicyManageSchedules)]
        public IActionResult New()
        {
            return View(CreateScheduleModel(null));
        }

        [HttpPost]
        [Authorize(Policy = STConsts.PolicyManageSchedules)]
        public IActionResult New(ScheduleModel model)
        {
            if (!ValidateScheduleModel(model))
                return View(CreateScheduleModel(model));

            ScheduleType scheduleType = (ScheduleType)Enum.Parse(typeof(ScheduleType), model.ScheduleType);
            bool created = false;

            switch (scheduleType)
            {
                case ScheduleType.Once:
                    created = _scheduleHelper.Create(model.Name, model.TestId, model.StartTime);
                    break;

                case ScheduleType.Minutes:
                case ScheduleType.Hours:
                case ScheduleType.Daily:
                    created = _scheduleHelper.Create(model.Name, model.TestId, model.StartTime,
                        model.Expires, model.Frequency, scheduleType);
                    break;

                case ScheduleType.Weekly:
                    created = _scheduleHelper.Create(model.Name, model.TestId, model.StartTime,
                        model.Expires, model.Frequency, GetScheduledDays(model));
                    break;

                default:
                    throw new InvalidOperationException();
            }

            if (!created)
                ModelState.AddModelError(String.Empty, "Failed to save schedule");

            if (ModelState.IsValid)
                return RedirectToAction(nameof(Index));

            return View(CreateScheduleModel(model));
        }

        [HttpGet]
        [Breadcrumb(nameof(Edit), Name, nameof(Index))]
        public IActionResult Edit(string id)
        {
            if (Int64.TryParse(id, out long scheduleId))
            {
                TestSchedule schedule = _scheduleHelper.Schedules.Where(s => s.UniqueId == scheduleId).FirstOrDefault();

                if (schedule != null)
                {
                    return View(CreateScheduleModelFromTestSchedule(schedule));
                }
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Policy = STConsts.PolicyManageSchedules)]

        public IActionResult Edit(ScheduleModel model)
        {
            if (!ValidateScheduleModel(model))
                return View(CreateScheduleModel(model));

            TestSchedule testSchedule = _scheduleHelper.Schedules.Where(s => s.UniqueId == model.UniqueId).FirstOrDefault();
            bool updated = UpdateTestScheduleFromModel(model, testSchedule);

            if (!updated)
                ModelState.AddModelError(String.Empty, "Failed to save schedule");

            if (ModelState.IsValid)
                return RedirectToAction(nameof(Index));

            return View(CreateScheduleModel(model));
        }

        [HttpGet]
        [Authorize(Policy = STConsts.PolicyManageSchedules)]
        [Breadcrumb(nameof(Delete), Name, nameof(Index))]
        public IActionResult Delete(string id)
        {
            if (Int64.TryParse(id, out long scheduleId))
            {
                TestSchedule schedule = _scheduleHelper.Schedules.Where(s => s.UniqueId == scheduleId).FirstOrDefault();

                if (schedule != null)
                {
                    return View(new DeleteScheduleModel(GetModelData(), schedule.UniqueId));
                }
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Policy = STConsts.PolicyManageSchedules)]
        public IActionResult Delete(DeleteScheduleModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            TestSchedule schedule = _scheduleHelper.Schedules.Where(s => s.UniqueId == model.UniqueId).FirstOrDefault();

            if (schedule == null)
            {
                return RedirectToAction(nameof(Index));
            }

            if (String.IsNullOrEmpty(model.Confirm) || !model.Confirm.Equals("delete", StringComparison.InvariantCultureIgnoreCase))
            {
                ModelState.AddModelError(nameof(model.Confirm), "Please type confirm to delete the test schedule");
            }

            if (ModelState.IsValid && !_scheduleHelper.Delete(schedule))
            {
                ModelState.AddModelError(String.Empty, "Failed to delete test schedule");
            }

            if (!ModelState.IsValid)
            {
                return View(new DeleteScheduleModel(GetModelData(), model.UniqueId));
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult RunSchedules()
        {
            return PartialView("_Scheduled", BuildRunSchedule());
        }

        [HttpGet]
        public IActionResult RunTest(string id)
        {
            TestSchedule testSchedule = null;

            if (Int64.TryParse(id, out long scheduleId))
            {
                testSchedule = _scheduleHelper.Schedules.Where(s => s.UniqueId.Equals(scheduleId)).FirstOrDefault();
            }

            if (testSchedule == null)
            {
                HttpContext.Response.StatusCode = 404;
            }
            else
            {
                _testRunManager.RunTest(testSchedule);
            }

            return new EmptyResult();
        }

        #endregion Public Action Methods

        #region Privte Methods

        private TestRunViewModels BuildRunSchedule()
        {
            List<TestRunViewModel> runModels = new List<TestRunViewModel>();

            foreach (TestSchedule schedule in _scheduleHelper.Schedules)
            {
                TestConfiguration test = TestConfigurations.Where(tc => tc.UniqueId.Equals(schedule.TestId)).FirstOrDefault();

                if (test == null)
                    continue;

                ReportSummary[] reportSummaries = _reportHelper.ReportSummary(schedule.UniqueId, 10);

                runModels.Add(new TestRunViewModel(schedule.Name,
                    test.Name,
                    schedule.UniqueId,
                    reportSummaries.Length > 0 ? reportSummaries[0].RunResult : schedule.LastRunResult,
                    schedule.NextRun,
                    reportSummaries.Length > 0 ? reportSummaries[0].EndTime : schedule.LastRun,
                    _testRunManager.QueuePositions(schedule.UniqueId),
                    _testRunManager.ActiveTests(schedule.UniqueId),
                    reportSummaries));
            }

            return new TestRunViewModels(runModels);
        }

        private ScheduleDay GetScheduledDays(in ScheduleModel model)
        {
            ScheduleDay Result = ScheduleDay.NotSet;

            if (model.DayMonday)
                Result |= ScheduleDay.Monday;

            if (model.DayTuesday)
                Result |= ScheduleDay.Tuesday;

            if (model.DayWednesday)
                Result |= ScheduleDay.Wednesday;

            if (model.DayThursday)
                Result |= ScheduleDay.Thursday;

            if (model.DayFriday)
                Result |= ScheduleDay.Friday;

            if (model.DaySaturday)
                Result |= ScheduleDay.Saturday;

            if (model.DaySunday)
                Result |= ScheduleDay.Sunday;

            if ((int)Result > 0)
                Result &= ~ScheduleDay.NotSet;

            return Result;
        }

        private bool ValidateScheduleModel(in ScheduleModel model)
        {
            ScheduleType scheduleType = (ScheduleType)Enum.Parse(typeof(ScheduleType), model.ScheduleType);

            if (String.IsNullOrEmpty(model.Name))
            {
                ModelState.AddModelError(nameof(model.Name), "Please enter a name for the scheduled test");
            }

            if (String.IsNullOrEmpty(model.TestId))
            {
                ModelState.AddModelError(nameof(model.TestId), "Please select the test to run");
            }

            if (scheduleType == ScheduleType.Weekly)
            {
                if (!model.DayMonday && !model.DayTuesday && !model.DayWednesday && !model.DayThursday &&
                    !model.DayFriday && !model.DaySaturday && !model.DaySunday)
                {
                    ModelState.AddModelError(String.Empty, "Please choose at least one day on which this test will run");
                }
            }
            else if (scheduleType == ScheduleType.Daily)
            {
                if (model.Frequency < 1)
                {
                    ModelState.AddModelError(nameof(model.Frequency), "Please enter a valid frequency for the scheduled test");
                }
            }

            return ModelState.IsValid;
        }

        private bool UpdateTestScheduleFromModel(in ScheduleModel model, in TestSchedule testSchedule)
        {
            if (model == null)
            {
                ModelState.AddModelError(String.Empty, "Failed to save schedule");
                return false;
            }

            if (testSchedule == null)
            {
                ModelState.AddModelError(String.Empty, "Failed to find schedule to update");
                return false;
            }

            ScheduleType scheduleType = (ScheduleType)Enum.Parse(typeof(ScheduleType), model.ScheduleType);

            testSchedule.Name = model.Name;
            testSchedule.UniqueId = model.UniqueId;
            testSchedule.ScheduleType = scheduleType;
            testSchedule.StartTime = model.StartTime;
            testSchedule.Frequency = model.Frequency;
            testSchedule.Expires = model.Expires;
            testSchedule.ScheduleDays = GetScheduledDays(model);

            return _scheduleHelper.Update(testSchedule);
        }

        private ScheduleModel CreateScheduleModelFromTestSchedule(TestSchedule testSchedule)
        {
            if (testSchedule == null)
                throw new ArgumentNullException(nameof(testSchedule));

            ScheduleModel Result = new ScheduleModel(GetModelData());

            Result.Name = testSchedule.Name;
            Result.UniqueId = testSchedule.UniqueId;
            Result.ScheduleType = testSchedule.ScheduleType.ToString();
            Result.StartTime = testSchedule.StartTime;
            Result.Frequency = testSchedule.Frequency;
            Result.DayMonday = testSchedule.ScheduleDays.HasFlag(ScheduleDay.Monday);
            Result.DayTuesday = testSchedule.ScheduleDays.HasFlag(ScheduleDay.Tuesday);
            Result.DayWednesday = testSchedule.ScheduleDays.HasFlag(ScheduleDay.Wednesday);
            Result.DayThursday = testSchedule.ScheduleDays.HasFlag(ScheduleDay.Thursday);
            Result.DayFriday = testSchedule.ScheduleDays.HasFlag(ScheduleDay.Friday);
            Result.DaySaturday = testSchedule.ScheduleDays.HasFlag(ScheduleDay.Saturday);
            Result.DaySunday = testSchedule.ScheduleDays.HasFlag(ScheduleDay.Sunday);
            Result.Expires = testSchedule.Expires;

            return Result;
        }

        private ScheduleModel CreateScheduleModel(in ScheduleModel model)
        {
            ScheduleModel Result = new ScheduleModel(GetModelData());

            Result.Name = model == null ? String.Empty : model.Name;
            Result.UniqueId = model == null ? _idManager.GenerateId() : model.UniqueId;
            Result.ScheduleType = model == null ? String.Empty : model.ScheduleType;
            Result.StartTime = model == null ? DateTime.UtcNow.AddHours(1) : model.StartTime;
            Result.Frequency = model == null ? 1 : model.Frequency;
            Result.DayMonday = model == null ? false : model.DayMonday;
            Result.DayTuesday = model == null ? false : model.DayTuesday;
            Result.DayWednesday = model == null ? false : model.DayWednesday;
            Result.DayThursday = model == null ? false : model.DayThursday;
            Result.DayFriday = model == null ? false : model.DayFriday;
            Result.DaySaturday = model == null ? false : model.DaySaturday;
            Result.DaySunday = model == null ? false : model.DaySunday;
            Result.Expires = model == null ? null : model.Expires;

            return Result;
        }

        #endregion Private Methods

        #region Static Properties

        public static List<ScheduleTypeModel> ScheduleTypes
        {
            get
            {
                List<ScheduleTypeModel> Result = new List<ScheduleTypeModel>();

                foreach (ScheduleType item in Enum.GetValues(typeof(ScheduleType)))
                {
                    if (item != ScheduleType.PressureTest)
                        Result.Add(new ScheduleTypeModel(item.ToString()));
                }

                return Result;
            }
        }

        public static List<TestConfiguration> TestConfigurations
        {
            get
            {
                return _testConfigurationProvider.Configurations;
            }
        }


        #endregion Static Properties
    }
}