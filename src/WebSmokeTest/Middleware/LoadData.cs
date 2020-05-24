using System;
using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json;
using PluginManager.Abstractions;
using Shared.Classes;

using SmokeTest.Shared.Interfaces;

namespace SmokeTest.Middleware
{
    public class LoadData : ILoadData
    {
        private readonly object _lockObject = new object();
        private readonly ILogger _logger;

        #region Constructors

        public LoadData(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion Constructors

        public T Load<T>(in string path, in string fileName)
        {
            using (TimedLock timedLock = TimedLock.Lock(_lockObject))
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                string userFile = Path.Combine(path, fileName);

                if (File.Exists(userFile))
                {
                    return JsonConvert.DeserializeObject<T>(File.ReadAllText(userFile));
                }

                return default;
            }
        }

        public void Load<T>(in List<T> dataList, in string path, in string fileExtension)
        {
            using (TimedLock timedLock = TimedLock.Lock(_lockObject))
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                string[] files = Directory.GetFiles(path, fileExtension);

                foreach (string file in files)
                {
                    dataList.Add(JsonConvert.DeserializeObject<T>(File.ReadAllText(file)));
                }
            }
        }
    }
}
