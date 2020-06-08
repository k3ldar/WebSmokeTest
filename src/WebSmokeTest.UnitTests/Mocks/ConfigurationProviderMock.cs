using System;
using System.Collections.Generic;
using System.Linq;

using Shared.Classes;

using SmokeTest.Shared;

namespace SmokeTest.UnitTests.Mocks
{
    class ConfigurationProviderMock : ITestConfigurationProvider
    {
        public ConfigurationProviderMock()
        {
            Configurations = new List<TestConfiguration>();
        }

        public List<TestConfiguration> Configurations { get; private set; }

        public bool ConfigurationExists(string name)
        {
            return Configurations.Where(c => c.Name.Equals(name)).Any();
        }

        public bool ConfigurationExists(string name, string currentId)
        {
            throw new NotImplementedException();
        }

        public bool Delete(in TestConfiguration configuration)
        {
            throw new NotImplementedException();
        }

        public bool SaveConfiguration(in string name, in string url, in int crawlDepth, in int maxPages,
            in int millisecondsBetweenRequest, in string userAgent, in string uniqueId, in bool checkImages,
            in bool clearHtmlData, in bool clearImageData, in List<string> additionalUrls, in NVPCodec headers)
        {
            throw new NotImplementedException();
        }

        public bool SaveConfiguration(in TestConfiguration configuration)
        {
            throw new NotImplementedException();
        }
    }
}
