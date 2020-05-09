
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Middleware;

using PluginManager.Abstractions;

using SharedPluginFeatures;

using WebSmokeTest.Middleware;

namespace WebSmokeTest.Classes
{
    public class PluginInitialisation : IPlugin, IInitialiseEvents
    {
        #region IInitialiseEvents Methods

        public void AfterConfigure(in IApplicationBuilder app)
        {

        }

        public void AfterConfigureServices(in IServiceCollection services)
        {
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            UserProvider loginProvider = new UserProvider(
                serviceProvider.GetRequiredService<IPluginClassesService>(),
                serviceProvider.GetRequiredService<ISettingsProvider>(),
                serviceProvider.GetRequiredService<IConfiguration>());

            services.AddSingleton<ILoginProvider>(loginProvider);
            services.AddSingleton<IClaimsProvider>(loginProvider);

            services.AddSingleton<IErrorManager, ErrorManagerProvider>();
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

        }

        #endregion IPlugin Methods
    }
}
