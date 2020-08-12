using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using PluginManager.Abstractions;

using SmokeTest.Shared;
using SmokeTest.Shared.Classes;
using SmokeTest.Shared.Interfaces;

namespace SmokeTest.Middleware
{
    public sealed class ScheduleHelper : IScheduleHelper
    {
        #region Private Members

        private readonly List<TestSchedule> _testSchedules;
        private readonly string _dataPath;
        private readonly ILogger _logger;
        private readonly ISaveData _saveData;
        private readonly ILoadData _loadData;
        private readonly IIdManager _idManager;

        #endregion Private Members

        #region Constructors

        public ScheduleHelper(ILogger logger, ISaveData saveData, ILoadData loadData, IIdManager idManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _saveData = saveData ?? throw new ArgumentNullException(nameof(saveData));
            _loadData = loadData ?? throw new ArgumentNullException(nameof(loadData));
            _idManager = idManager ?? throw new ArgumentNullException(nameof(idManager));

            _testSchedules = new List<TestSchedule>();

            _dataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SmokeTest", "Schedules");

            LoadTestSchedules();
        }

        #endregion Constructors

        #region IScheduleHelper Properties

        public List<TestSchedule> Schedules
        {
            get
            {
                return _testSchedules.Where(ts => ts.ScheduleType != ScheduleType.PressureTest).ToList();
            }
        }

        #endregion IScheduleHelper Properties

        #region IScheduleHelper Methods

        public bool Create(in string name, in string testId, in DateTime startTime)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            if (String.IsNullOrEmpty(testId))
                throw new ArgumentOutOfRangeException(nameof(testId));

            TestSchedule testSchedule = new TestSchedule(_idManager.GenerateId(), name, testId, startTime);

            if (SaveSchedule(testSchedule))
            {
                _testSchedules.Add(testSchedule);
                return true;
            }

            return false;
        }

        public bool Create(in string name, in string testId, in DateTime startTime, in DateTime? expires, in int frequency, in ScheduleType scheduleType)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            if (String.IsNullOrEmpty(testId))
                throw new ArgumentOutOfRangeException(nameof(testId));

            TestSchedule testSchedule = new TestSchedule(_idManager.GenerateId(), name, testId, startTime, expires, frequency, scheduleType);

            if (SaveSchedule(testSchedule))
            {
                _testSchedules.Add(testSchedule);
                return true;
            }

            return false;
        }

        public bool Create(in string name, in string testId, in DateTime startTime, in DateTime? expires,
            in int frequency, in ScheduleDay scheduleDay)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            if (String.IsNullOrEmpty(testId))
                throw new ArgumentOutOfRangeException(nameof(testId));

            TestSchedule testSchedule = new TestSchedule(_idManager.GenerateId(), name, testId, startTime, expires, frequency, scheduleDay);

            if (SaveSchedule(testSchedule))
            {
                _testSchedules.Add(testSchedule);
                return true;
            }

            return false;
        }

        public bool Update(in TestSchedule testSchedule)
        {
            if (testSchedule == null)
                throw new ArgumentNullException(nameof(testSchedule));

            if (SaveSchedule(testSchedule))
            {
                return true;
            }

            return false;
        }

        public bool Delete(in TestSchedule testSchedule)
        {
            if (testSchedule == null)
                throw new ArgumentNullException(nameof(testSchedule));

            string scheduleFile = Path.Combine(_dataPath, $"{testSchedule.UniqueId}.sch");

            if (!File.Exists(scheduleFile))
            {
                return false;
            }

            File.Delete(scheduleFile);

            bool Result = !File.Exists(scheduleFile);

            if (Result)
            {
                _testSchedules.Remove(testSchedule);
            }

            return Result;
        }

        #endregion IScheduleHelper Methods

        #region Private Methods

        private void LoadTestSchedules()
        {
            _testSchedules.Clear();
            _loadData.Load<TestSchedule>(_testSchedules, _dataPath, "*.sch");
        }

        private bool SaveSchedule(in TestSchedule testSchedule)
        {
            return _saveData.Save<TestSchedule>(testSchedule, _dataPath, $"{testSchedule.UniqueId}.sch");
        }

        #endregion Private Methods
    }
}
