using System;

namespace SmokeTest.Shared
{
    public interface ITestRunLogger
    {
        void Log(in string logData);

        void Log(in Exception exception);

        string RetrieveData(in long startPosition);
    }
}
