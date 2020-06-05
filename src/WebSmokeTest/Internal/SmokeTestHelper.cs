using System;
using System.Collections.Generic;

using PluginManager.Abstractions;

using Shared.Classes;

using SharedPluginFeatures;

using SmokeTest.Shared;

namespace SmokeTest.Internal
{
    internal class SmokeTestHelper : ISmokeTestHelper
    {
        private const string HomeCards = "HomeCards";
        private readonly IMemoryCache _memoryCache;
        private readonly IPluginClassesService _pluginClassesService;

        public SmokeTestHelper(IMemoryCache memoryCache, IPluginClassesService pluginClassesService)
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _pluginClassesService = pluginClassesService ?? throw new ArgumentNullException(nameof(pluginClassesService));
        }

        public List<HomeCard> HomeCardsGet()
        {
            CacheItem cacheItem = _memoryCache.GetCache().Get(HomeCards);

            if (cacheItem == null)
            {
                cacheItem = new CacheItem(HomeCards, _pluginClassesService.GetPluginClasses<HomeCard>());
                _memoryCache.GetCache().Add(HomeCards, cacheItem);
            }

            return (List<HomeCard>)cacheItem.Value;
        }
    }
}
