using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Middleware;
using Middleware.Accounts;

using PluginManager.Abstractions;

using SharedPluginFeatures;

using SmokeTest.Engine;
using SmokeTest.Middleware;
using SmokeTest.Shared;
using SmokeTest.Shared.Interfaces;

namespace SmokeTest.Internal
{
    public class PluginInitialisation : IPlugin, IInitialiseEvents
    {
        #region Private Members

        private static ILogger _logger;

        #endregion Private Members

        #region IInitialiseEvents Methods

        public void AfterConfigure(in IApplicationBuilder app)
        {

        }

        public void AfterConfigureServices(in IServiceCollection services)
        {
            services.AddSingleton<ITestRunManager, TestRunManager>();

            SaveData saveData = new SaveData(_logger);
            LoadData loadData = new LoadData(_logger);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            UserProvider userProvider = new UserProvider(
                serviceProvider.GetRequiredService<IPluginClassesService>(),
                _logger,
                saveData,
                loadData);

            services.TryAddSingleton<ISaveData>(saveData);
            services.TryAddSingleton<ILoadData>(loadData);
            services.TryAddSingleton<ILoginProvider>(userProvider);
            services.TryAddSingleton<IClaimsProvider>(userProvider);
            services.TryAddSingleton<IAccountProvider>(userProvider);
            services.TryAddSingleton<IUserSearch>(userProvider);
            services.TryAddSingleton<IDownloadProvider, DownloadProvider>();
            services.TryAddSingleton<ICountryProvider, CountryProvider>();
            services.TryAddSingleton<ILicenceProvider, LicenseProvider>();
            services.TryAddSingleton<IErrorManager, ErrorManagerProvider>();
            services.TryAddSingleton<ISmokeTestHelper, SmokeTestHelper>();
            services.TryAddSingleton<ISeoProvider, SeoProvider>();
            services.TryAddSingleton<ITestConfigurationProvider, ConfigurationProvider>();
            services.TryAddSingleton<IScheduleHelper, ScheduleHelper>();
            services.TryAddSingleton<IIdManager, IdManager>();
        }

        public void BeforeConfigure(in IApplicationBuilder app)
        {

        }

        public void BeforeConfigureServices(in IServiceCollection services)
        {

        }

        public void Configure(in IApplicationBuilder app)
        {

        }

        #endregion IInitialiseEvents Methods

        #region IPlugin Methods

        public void ConfigureServices(IServiceCollection services)
        {

        }

        public void Finalise()
        {

        }

        public ushort GetVersion()
        {
            return 1;
        }

        public void Initialise(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion IPlugin Methods
    }
}
