// \\     |/\  /||
//  \\ \\ |/ \/ ||
//   \//\\/|  \ || 
// Copyright © Artem & Alexander Paskhin 2018. All rights reserved.
// Wallsmedia LTD 2018:{Artem & Alexander Paskhin}
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Dot Net Sample logger for Microsoft.Extensions.Logging.


using System;
using System.IO;
using DotNet.NLogger.NetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Config;

namespace RestWebApplication
{
    public class Program
    {

        public static void AppConfigure(WebHostBuilderContext hostingContext, ILoggingBuilder logging)
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

            logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
            //logging.AddConsole();
            //logging.AddDebug();
        }

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureLogging(AppConfigure);
                    webBuilder.UseStartup<Startup>();
                });
    }
}
