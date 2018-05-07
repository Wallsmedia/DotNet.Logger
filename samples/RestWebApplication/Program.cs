// \\     |/\  /||
//  \\ \\ |/ \/ ||
//   \//\\/|  \ || 
// Copyright © Artem Paskhin 2018. All rights reserved.
// Wallsmedia LTD 2018:{Artem Paskhin}
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Dot Net Sample logger for Microsoft.Extensions.Logging.


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DotNet.Memory.Logger;
using DotNet.Memory.NetCore;
using DotNet.NLogger.NetCore;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
//using Microsoft.AspNetCore.HostFiltering;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog.Config;

namespace RestWebApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                CreateWebHostBuilder(args).Build().Run();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            CreateDefaultBuilder(args)
                .UseStartup<Startup>();

        private static IWebHostBuilder CreateDefaultBuilder(string[] args)
        {

            var builder = new WebHostBuilder()
             .UseKestrel(
             /* ver 2.1.
             (builderContext, options) =>
          {
              options.Configure(builderContext.Configuration.GetSection("Kestrel"));
          }*/
             )
             .UseContentRoot(Directory.GetCurrentDirectory())
             .ConfigureAppConfiguration((hostingContext, config) =>
             {
                 var env = hostingContext.HostingEnvironment;


                 config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                       .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);


                 if (env.IsDevelopment())
                 {
                     var appAssembly = Assembly.Load(new AssemblyName(env.ApplicationName));
                     if (appAssembly != null)
                     {
                         config.AddUserSecrets(appAssembly, optional: true);
                     }
                 }


                 config.AddEnvironmentVariables();


                 if (args != null)
                 {
                     config.AddCommandLine(args);
                 }
             })
             .ConfigureLogging((hostingContext, logging) =>
             {

                 // ** Add DotNet.NLogger.NetCore

                 string logPath = Path.Combine(hostingContext.HostingEnvironment.ContentRootPath, $"nlog.{hostingContext.HostingEnvironment.EnvironmentName}.config");
                 if (!File.Exists(logPath))
                 {
                     throw new MissingMemberException($"Missing NLog configuration file '{logPath}'");
                 }
                 var nLoggingConfiguration = new XmlLoggingConfiguration(logPath);

                 var logJsonCgf = hostingContext.Configuration.GetSection(nameof(NLogLoggerSettings));
                 if (!logJsonCgf.Exists())
                 {
                     throw new MissingMemberException($"Missing configuration section '{nameof(NLogLoggerSettings)}'");
                 }

                 logging.AddNLogLogger(logJsonCgf, nLoggingConfiguration);

                 // ** Add DotNet.Memory.Logger

                 logJsonCgf = hostingContext.Configuration.GetSection(nameof(MemoryLoggerSettings));
                 if (!logJsonCgf.Exists())
                 {
                     throw new MissingMemberException($"Missing configuration section '{nameof(MemoryLoggerSettings)}'");
                 }
                 logging.AddMemoryLogger(logJsonCgf);

                 //logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                 //logging.AddConsole();
                 //logging.AddDebug();
             })
             .ConfigureServices((hostingContext, services) =>
             {
                 /* for ver 2.1
                 // Fallback 
                 services.PostConfigure<HostFilteringOptions>(options =>
                             {
                                 if (options.AllowedHosts == null || options.AllowedHosts.Count == 0)
                                 {
                                     // "AllowedHosts": "localhost;127.0.0.1;[::1]" 
                                     var hosts = hostingContext.Configuration["AllowedHosts"]?.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                                     // Fall back to "*" to disable. 
                                     options.AllowedHosts = (hosts?.Length > 0 ? hosts : new[] { "*" });
                                 }
                             });

                 // Change notification 
                 services.AddSingleton<IOptionsChangeTokenSource<HostFilteringOptions>>(
                                 new ConfigurationChangeTokenSource<HostFilteringOptions>(hostingContext.Configuration));

                 // Hosting filter to the startip
                 services.AddTransient<IStartupFilter, HostFilteringStartupFilter>();
                 */
             })
             .UseIISIntegration()
             .UseDefaultServiceProvider((context, options) =>
             {
                 options.ValidateScopes = context.HostingEnvironment.IsDevelopment();
             });


            if (args != null)
            {
                builder.UseConfiguration(new ConfigurationBuilder().AddCommandLine(args).Build());
            }


            return builder;
        }
    }
    /* ver 2.1
    internal class HostFilteringStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                app.UseHostFiltering();
                next(app);
            };
        }
    }
    */
}
