using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using Middleware;
using Middleware.Accounts;

using PluginManager.Abstractions;

using SharedPluginFeatures;

using SmokeTest.Middleware;
using SmokeTest.Shared;
using SmokeTest.Shared.Interfaces;

namespace SmokeTest.Classes
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

            services.AddSingleton<ISaveData>(saveData);
            services.AddSingleton<ILoadData>(loadData);
            services.AddSingleton<ILoginProvider>(userProvider);
            services.AddSingleton<IClaimsProvider>(userProvider);
            services.AddSingleton<IAccountProvider>(userProvider);
            services.AddSingleton<IUserSearch>(userProvider);
            services.AddSingleton<IDownloadProvider, DownloadProvider>();
            services.AddSingleton<ICountryProvider, CountryProvider>();
            services.AddSingleton<ILicenceProvider, LicenseProvider>();
            services.AddSingleton<IErrorManager, ErrorManagerProvider>();
            services.AddSingleton<ISmokeTestHelper, SmokeTestHelper>();
            services.AddSingleton<ISeoProvider, SeoProvider>();
            services.AddSingleton<ITestConfigurationProvider, ConfigurationProvider>();
            services.AddSingleton<IScheduleHelper, ScheduleHelper>();
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
