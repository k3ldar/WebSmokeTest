﻿using System.Collections.Generic;

using Shared.Classes;

namespace SmokeTest.Shared
{
    public interface ITestConfigurationProvider
    {
        List<TestConfiguration> Configurations { get; }

        bool ConfigurationExists(string name, string currentId);

        bool SaveConfiguration(in string name, in string url, in int crawlDepth, in int maxPages,
            in int millisecondsBetweenRequest, in string userAgent, in string uniqueId,
            in bool checkImages, in bool clearHtmlData, in bool clearImageData,
            in int minimumLoadTime, in bool scanSite, in string encryptionKey,
            in List<string> additionalUrls, in NVPCodec headers);

        bool SaveConfiguration(in TestConfiguration configuration);

        bool Delete(in TestConfiguration configuration);
    }
}
