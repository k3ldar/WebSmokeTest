using System;
using System.IO;

using Newtonsoft.Json;

using PluginManager.Abstractions;

using Shared.Classes;

using SmokeTest.Shared;

namespace SmokeTest.Middleware
{
    public class SaveData : ISaveData
    {
        #region Private Members

        private readonly object _lockObject = new object();
        private readonly ILogger _logger;

        #endregion Private Members

        #region Constructors

        public SaveData(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion Constructors

        #region ISaveData Methods

        public bool Save<T>(T data, in string path, in string fileName)
        {
            using (TimedLock timedLock = TimedLock.Lock(_lockObject))
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                string dataFile = Path.Combine(path, fileName);
                string tempCopy = Path.ChangeExtension(dataFile, ".tmp");
                try
                {
                    if (File.Exists(dataFile))
                    {
                        File.Move(dataFile, tempCopy, true);
                    }

                    File.WriteAllText(dataFile, JsonConvert.SerializeObject(data));

                    if (File.Exists(tempCopy))
                    {
                        File.Delete(tempCopy);
                    }

                    return true;
                }
                catch (Exception err)
                {
                    if (File.Exists(tempCopy))
                    {
                        File.Move(tempCopy, dataFile, true);
                    }

                    _logger.AddToLog(PluginManager.LogLevel.Error, err);

                    return false;
                }
            }
        }

        #endregion ISaveData Methods
    }
}
