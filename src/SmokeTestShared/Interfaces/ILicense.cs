using System;

namespace SmokeTest.Shared
{
    public interface ILicense
    {
        /// <summary>
        /// Determines whether a license option is available or not
        /// </summary>
        /// <param name="licenseOption">License option being queried</param>
        /// <returns>bool</returns>
        bool OptionAvailable(in string licenseOption);

        string OptionValue(in string licenseOption);

        /// <summary>
        /// Maximum number of tests that can be run at any one time
        /// </summary>
        int MaximumRunningTests { get; }

        /// <summary>
        /// Maximum number of configurations allowed on the installation
        /// </summary>
        int MaximumConfigurations { get; }

        /// <summary>
        /// Maximum number of pages that can be scanned
        /// </summary>
        int MaximumPageScans { get; }

        /// <summary>
        /// Maximum number of open endpoints
        /// </summary>
        int MaximumOpenEndpoints { get; }

        /// <summary>
        /// Maximum number of tests allowed to run
        /// </summary>
        int MaximumTestsToRun { get; }

        /// <summary>
        /// Maximum number of test schedules
        /// </summary>
        int MaximumTestSchedules { get; }

        /// <summary>
        /// Date/time license expires
        /// </summary>
        DateTime Expires { get; }

        /// <summary>
        /// Registered user or organisation
        /// </summary>
        string RegisteredUser { get; }
    }
}
