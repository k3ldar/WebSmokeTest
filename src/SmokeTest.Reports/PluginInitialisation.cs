﻿using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using PluginManager.Abstractions;

using SharedPluginFeatures;

using SmokeTest.Reports.Internal;
using SmokeTest.Shared;

namespace SmokeTest.Reports
{
    public class testconfigurator : ILicenseFactory
    {
        public ILicense GetActiveLicense()
        {
            throw new System.NotImplementedException();
        }

        public bool LicenseIsValid(in ILicense license)
        {
            throw new System.NotImplementedException();
        }

        public ILicense LoadLicense(in string license)
        {
            throw new System.NotImplementedException();
        }

        public string SaveLicense(in ILicense license)
        {
            throw new System.NotImplementedException();
        }

        public void SetActiveLicense(in ILicense license)
        {
            throw new System.NotImplementedException();
        }
    }

    public class PluginInitialisation : IPlugin, IInitialiseEvents
    {
        #region IInitialiseEvents Methods

        public void AfterConfigure(in IApplicationBuilder app)
        {

        }

        public void AfterConfigureServices(in IServiceCollection services)
        {
            services.AddSingleton<ILicenseFactory, testconfigurator>();
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
            services.AddSingleton<IReportHelper, ReportHelper>();
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
