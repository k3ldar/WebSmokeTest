
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using PluginManager.Abstractions;

using SmokeTest.Shared;

namespace SmokeTest.Classes
{
    public class ServiceConfigurator : IServiceConfigurator
    {
        public void RegisterServices(IServiceCollection services)
        {
            services.Replace(new ServiceDescriptor(typeof(ILicenseFactory), typeof(LicenseFactory), ServiceLifetime.Singleton));
            services.Replace(new ServiceDescriptor(typeof(ILicenseFactory), typeof(LicenseFactory), ServiceLifetime.Scoped));
            services.Replace(new ServiceDescriptor(typeof(ILicenseFactory), typeof(LicenseFactory), ServiceLifetime.Transient));
        }
    }
}
