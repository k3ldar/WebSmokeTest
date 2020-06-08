using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

using PluginManager.Abstractions;

using SmokeTest.Engine;

namespace SmokeTest.Internal
{
    internal class SmokeTestBackgroundScheduler : BackgroundService
    {
        #region Private Members

        private readonly ILogger _logger;
        private readonly ITestRunManager _testRunManager;

        #endregion Private Members

        public SmokeTestBackgroundScheduler(ILogger logger, ITestRunManager testRunManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _testRunManager = testRunManager ?? throw new ArgumentNullException(nameof(testRunManager));
        }

        #region Overridden Methods

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.AddToLog(PluginManager.LogLevel.Information, "SmokeTest Background Scheduler Starting");
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    _testRunManager.ProcessTests();
                    await Task.Delay(250, stoppingToken);
                }
            }
            finally
            {
                _testRunManager.CancelAll();
                _logger.AddToLog(PluginManager.LogLevel.Information, "SmokeTest Background Scheduler Stopping");
            }
        }

        #endregion Overridden Methods
    }
}
