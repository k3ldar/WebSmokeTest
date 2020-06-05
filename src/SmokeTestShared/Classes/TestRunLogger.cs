using System;
using System.IO;
using System.Text;

using Shared.Classes;

namespace SmokeTest.Shared
{
    public class TestRunLogger : ITestRunLogger
    {
        #region Private Members

        private readonly string _dataFile;
        private readonly static object _lockObject = new object();

        #endregion Private Members

        #region Constructors

        public TestRunLogger(in long uniqueId)
        {
            string commonData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            _dataFile = Path.Combine(commonData, "SmokeTest", "Reports", uniqueId.ToString("X"), $"{uniqueId}.log");

            if (!Directory.Exists(Path.GetDirectoryName(_dataFile)))
                Directory.CreateDirectory(Path.GetDirectoryName(_dataFile));
        }

        #endregion Constructors

        #region ITestRunLogger Methods

        public void Log(in string logData)
        {
            using (TimedLock tl = TimedLock.Lock(_lockObject))
            {
                File.AppendAllText(_dataFile, $"{DateTime.UtcNow.ToLocalTime().ToString("HH:mm:ss")}\t{logData}\r\n");
            }
        }

        public void Log(in Exception exception)
        {
            using (TimedLock tl = TimedLock.Lock(_lockObject))
            {
                Log(exception.Message);
            }
        }

        public string RetrieveData(in long startPosition)
        {
            using (TimedLock tl = TimedLock.Lock(_lockObject))
            {
                StringBuilder sb = new StringBuilder(4096);

                using (StreamReader sr = new StreamReader(_dataFile))
                {
                    sr.BaseStream.Position = startPosition;
                    sb.Append(sr.ReadToEnd());
                }

                return sb.ToString();
            }
        }

        #endregion ITestRunLogger Methods
    }
}
