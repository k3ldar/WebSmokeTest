using System;

using AspNetCore.PluginManager;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using PluginManager;

using Shared.Classes;

using SmokeTest.Internal;

namespace SmokeTest
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            ThreadManager.Initialise();
            ThreadManager.AllowThreadPool = true;
            ThreadManager.MaximumPoolSize = 20000;

            System.Net.ServicePointManager.DefaultConnectionLimit = 100;
            System.Net.ServicePointManager.ReusePort = true;
            System.Net.ServicePointManager.MaxServicePoints = 5;
            System.Net.ServicePointManager.ServerCertificateValidationCallback = (message, cert, chain, sslPolicyErrors) =>
            {
                return true;
            };

            Logger logger = new Logger();

            AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) =>
            {
                if (eventArgs.Exception.Message.Equals("Unable to read data from the transport connection: The I/O operation has been aborted because of either a thread exit or an application request"))
                {

                }
                else if (eventArgs.Exception.Message.StartsWith("The response ended prematurely."))
                {

                }
                else
                {
                    logger.AddToLog(LogLevel.Critical, eventArgs.Exception);
                }
            };

            PluginManagerService.UsePlugin(typeof(ErrorManager.Plugin.PluginInitialisation));
            PluginManagerService.UsePlugin(typeof(RestrictIp.Plugin.PluginInitialisation));
            PluginManagerService.UsePlugin(typeof(UserSessionMiddleware.Plugin.PluginInitialisation));
            PluginManagerService.UsePlugin(typeof(CacheControl.Plugin.PluginInitialisation));
            PluginManagerService.UsePlugin(typeof(MemoryCache.Plugin.PluginInitialisation));
            PluginManagerService.UsePlugin(typeof(Localization.Plugin.PluginInitialisation));
            PluginManagerService.UsePlugin(typeof(Breadcrumb.Plugin.PluginInitialisation));

            PluginManagerConfiguration configuration = new PluginManagerConfiguration(logger);

            PluginManagerService.Initialise(configuration);

            PluginManagerService.UsePlugin(typeof(DocumentationPlugin.PluginInitialisation));
            PluginManagerService.UsePlugin(typeof(UserAccount.Plugin.PluginInitialisation));
            PluginManagerService.UsePlugin(typeof(LoginPlugin.PluginInitialisation));
            PluginManagerService.UsePlugin(typeof(SearchPlugin.PluginInitialisation));
            PluginManagerService.UsePlugin(typeof(SystemAdmin.Plugin.PluginInitialisation));
            PluginManagerService.UsePlugin(typeof(Settings.PluginInitialisation));
            PluginManagerService.UsePlugin(typeof(Scheduler.PluginInitialisation));
            PluginManagerService.UsePlugin(typeof(Reports.PluginInitialisation));
            PluginManagerService.UsePlugin(typeof(WebSmokeTest.Plugin.PluginInitialisation));
            PluginManagerService.UsePlugin(typeof(SmokeTest.SystemTests.PluginInitialisation));

            try
            {
                CreateHostBuilder(args).Build().Run();
            }
            finally
            {
                ThreadManager.CancelAll();
                PluginManagerService.Finalise();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<SmokeTestBackgroundScheduler>();
                    services.Configure<KestrelServerOptions>(
                        hostContext.Configuration.GetSection("Kestrel"));
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
