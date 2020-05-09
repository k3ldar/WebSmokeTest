using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

using PluginManager.Abstractions;

using Shared.Classes;

using WebSmokeTest.Engine;
using AspNetCore.PluginManager;

using PluginManager;

namespace WebSmokeTest
{
    public class SmokeTestBackgroundScheduler : BackgroundService
    {
        #region Private Members

        private static readonly object _lockObject = new object();
        private static readonly Queue<SmokeTestProperties> _scheduledTests = new Queue<SmokeTestProperties>();
        private readonly ILogger _logger;

        #endregion Private Members

        public SmokeTestBackgroundScheduler(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Overridden Methods

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.AddToLog(PluginManager.LogLevel.Information, "SmokeTest Background Scheduler Starting");

            while (!stoppingToken.IsCancellationRequested)
            {
                using (TimedLock timedLock = TimedLock.Lock(_lockObject))
                {
                    if (_scheduledTests.TryDequeue(out SmokeTestProperties smokeTest))
                    {
                        ThreadWebsiteScan websiteScan = new ThreadWebsiteScan(smokeTest);
                        ThreadManager.ThreadStart(websiteScan, $"Smoke Test: {smokeTest.Url}", ThreadPriority.Lowest);
                    }
                }

                await Task.Delay(2500, stoppingToken);
            }

            _logger.AddToLog(PluginManager.LogLevel.Information, "SmokeTest Background Scheduler Stoping");
        }

        #endregion Overridden Methods

        #region Internal Static Methods

        internal static void ScheduleSmokeTest(in SmokeTestProperties smokeTestProperties)
        {
            using (TimedLock timedLock = TimedLock.Lock(_lockObject))
            {
                _scheduledTests.Enqueue(smokeTestProperties);
            }
        }

        #endregion Internal Static Methods
    }
}
