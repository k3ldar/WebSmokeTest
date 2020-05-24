﻿using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using PluginManager.Abstractions;

using SharedPluginFeatures;

using SmokeTest.Shared;

namespace SmokeTest.Settings
{
    public class PluginInitialisation : IPlugin, IInitialiseEvents
    {
        #region IInitialiseEvents Methods

        public void AfterConfigure(in IApplicationBuilder app)
        {

        }

        public void AfterConfigureServices(in IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy(
                    STConsts.PolicyManageConfigurations,
                    policyBuilder => policyBuilder.RequireClaim(STConsts.ClaimManageConfiguration));

                options.AddPolicy(
                    STConsts.PolicyViewConfigurations,
                    policyBuilder => policyBuilder.RequireClaim(STConsts.ClaimViewConfiguration));
            });
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
