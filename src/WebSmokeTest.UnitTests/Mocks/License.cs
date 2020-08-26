using System;

using Microsoft.CodeAnalysis.CSharp.Syntax;

using SmokeTest.Shared;

namespace SmokeTest.UnitTests.Mocks
{
    public class License : ILicense
    {
        public int MaximumRunningTests => 50;

        public int MaximumConfigurations => 50;

        public int MaximumPageScans => 50;

        public int MaximumOpenEndpoints => 50;

        public int MaximumTestsToRun => 50;

        public DateTime Expires => DateTime.UtcNow.AddYears(10);

        public string RegisteredUser => "Scam License";

        public int MaximumTestSchedules => 50;

        public bool OptionAvailable(in string licenseOption)
        {
            return false;
        }

        public string OptionValue(in string licenseOption)
        {
            return String.Empty;
        }
    }
}
