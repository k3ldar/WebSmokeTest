using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

using Newtonsoft.Json;

using PluginManager.Abstractions;

using Shared.Classes;

using SmokeTest.Shared;

namespace SmokeTest.Middleware
{
    public class ConfigurationProvider : ITestConfigurationProvider
    {
        #region Private Members

        private readonly object _lockObject = new object();
        private readonly List<TestConfiguration> _configurations;
        private readonly string _dataPath;
        private readonly ILogger _logger;
        private readonly ISaveData _saveData;

        #endregion Private Members

        #region Constructors

        public ConfigurationProvider(ILogger logger, ISaveData saveData)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _saveData = saveData ?? throw new ArgumentNullException(nameof(saveData));
            _configurations = new List<TestConfiguration>();
            _dataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SmokeTest", "Configurations");

            if (!Directory.Exists(_dataPath))
                Directory.CreateDirectory(_dataPath);

            string[] configurations = Directory.GetFiles(_dataPath, "*.test");

            foreach (string s in configurations)
            {
                _configurations.Add(JsonConvert.DeserializeObject<TestConfiguration>(File.ReadAllText(s)));
            }
        }

        #endregion Constructors

        #region ITestConfigurationProvider Properties

        public List<TestConfiguration> Configurations
        {
            get
            {
                return _configurations;
            }
        }

        #endregion ITestConfigurationProvider Properties

        #region ITestConfigurationProvider Methods

        public bool ConfigurationExists(string name, string currentId)
        {
            return _configurations.Where(c => c.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase) && c.UniqueId != currentId).Count() > 0;
        }

        public bool SaveConfiguration(in string name, in string url, in int crawlDepth, in int maxPages,
            in int millisecondsBetweenRequest, in string userAgent, in string uniqueId, in bool checkImages,
            in bool clearHtmlData, in bool clearImageData,
            in int minimumLoadTime, in bool scanSite,
            in List<string> additionalUrls, in NVPCodec headers)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            if (String.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));

            if (maxPages < -1 || maxPages == 0)
                throw new ArgumentOutOfRangeException(nameof(maxPages));

            if (millisecondsBetweenRequest < 50 || millisecondsBetweenRequest > 2500)
                throw new ArgumentOutOfRangeException(nameof(millisecondsBetweenRequest));

            if (String.IsNullOrEmpty(userAgent))
                throw new ArgumentNullException(nameof(userAgent));

            if (userAgent.Length < 10 || userAgent.Length > 150)
                throw new ArgumentOutOfRangeException(nameof(userAgent));

            if (additionalUrls == null)
                throw new ArgumentNullException(nameof(additionalUrls));

            if (headers == null)
                throw new ArgumentNullException(nameof(headers));

            if (File.Exists(Path.Combine(_dataPath, uniqueId, ".test")))
                throw new InvalidOperationException($"Test already exists: {uniqueId}");

            TestConfiguration configuration = new TestConfiguration(name, url, crawlDepth, maxPages,
                millisecondsBetweenRequest, userAgent, uniqueId, checkImages, clearHtmlData, clearImageData,
                minimumLoadTime, scanSite,
                additionalUrls, headers);

            bool Result = SaveConfigurationToFile(configuration);

            if (Result)
            {
                TestConfiguration previousConfig = _configurations.Where(c => c.UniqueId.Equals(configuration.UniqueId)).FirstOrDefault();

                if (previousConfig != null)
                    _configurations.Remove(previousConfig);

                _configurations.Add(configuration);
            }

            return Result;
        }

        public bool SaveConfiguration(in TestConfiguration configuration)
        {
            return SaveConfigurationToFile(configuration);
        }

        public bool Delete(in TestConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            string scheduleFile = Path.Combine(_dataPath, $"{configuration.UniqueId}.test");

            if (!File.Exists(scheduleFile))
            {
                return false;
            }

            File.Delete(scheduleFile);

            bool Result = !File.Exists(scheduleFile);

            if (Result)
            {
                _configurations.Remove(configuration);
            }

            return Result;
        }

        #endregion ITestConfigurationProvider Methods

        #region Private Methods

        private bool SaveConfigurationToFile(in TestConfiguration testConfiguration)
        {
            return _saveData.Save<TestConfiguration>(testConfiguration, _dataPath, $"{testConfiguration.UniqueId}.test");
        }

        #endregion Private Methods
    }
}
