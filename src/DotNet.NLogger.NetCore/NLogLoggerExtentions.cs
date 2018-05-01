// \\     |/\  /||
//  \\ \\ |/ \/ ||
//   \//\\/|  \ || 
// Copyright © Artem Paskhin 2018. All rights reserved.
// Wallsmedia LTD 2018:{Artem Paskhin}
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
//
//  Dot Net NLog logger wrapper for Microsoft.Extensions.Logging.

using NLog.Config;
using NLog.Targets;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotNet.NLogger.NetCore
{
    /// <summary>
    /// Setups NLog configuration and other settings.
    /// </summary>
    public static class NLogLoggerExtentions
    {

        /// <summary>
        /// Creates new or updates an existing configuration with the console target name "ConsoleTarget".
        /// It will be like the [target xsi:type="Console" name="ConsoleTarget" layout="${date:universalTime=true:format=HH\:mm\:ss}|${level:uppercase=true}|${message}" ] 
        /// The rule will be set to like  [logger name="*" minlevel="Info" writeTo="ConsoleTarget"]
        /// </summary>
        /// <param name="nLogSettings">The settings for <see cref="NLogLogger"/>.</param>
        /// <param name="configuration">The NLog logging configuration.</param>
        /// <returns></returns>
        public static LoggingConfiguration GenerateUpdateConsoleLoggingConfiguration(NLogLoggerSettings nLogSettings, LoggingConfiguration configuration = null)
        {
            LoggingConfiguration nlogConfiguration = configuration ?? new LoggingConfiguration();
            var consoleTarget = new ConsoleTarget();
            consoleTarget.Name = nameof(ConsoleTarget);
            consoleTarget.Layout = @"${date:universalTime=true:format=HH\:mm\:ss}|${level:uppercase=true}|${message}";
            nlogConfiguration.RemoveTarget(consoleTarget.Name);
            nlogConfiguration.AddTarget(consoleTarget.Name, consoleTarget);

            if (nLogSettings.AcceptedCategoryNames.Count == 0)
            {
                var rule = new LoggingRule("*", NLog.LogLevel.Info, NLog.LogLevel.Fatal, consoleTarget);
                nlogConfiguration.LoggingRules.Add(rule);
            }
            else
            {
                // Create rules for all categories in the settings.
                foreach (var loggerNamePattern in nLogSettings.AcceptedCategoryNames)
                {
                    var rule = new LoggingRule(loggerNamePattern, NLog.LogLevel.Info, NLog.LogLevel.Fatal, consoleTarget);
                    nlogConfiguration.LoggingRules.Add(rule);
                }
            }
            return nlogConfiguration;
        }

        /// <summary>
        /// Creates new or updates an existing configuration with the console target "ConsoleTarget".
        /// It will be like the [target xsi:type="Console" name="ConsoleTarget" layout="${date:universalTime=true:format=HH\:mm\:ss}|${level:uppercase=true}|${message}"] 
        /// The rule will be set to [like  logger name="*" minlevel="Info" writeTo="ConsoleTarget"] 
        /// </summary>
        /// <param name="loggerNamePattern">The NLog rule name/>.</param>
        /// <param name="configuration">The NLog logging configuration.</param>
        /// <returns></returns>
        public static LoggingConfiguration GenerateUpdateConsoleLoggingConfiguration(string loggerNamePattern, LoggingConfiguration configuration = null)
        {
            LoggingConfiguration nlogConfiguration = configuration ?? new LoggingConfiguration();
            var consoleTarget = new ConsoleTarget();
            consoleTarget.Name = nameof(ConsoleTarget);
            consoleTarget.Layout = @"${date:universalTime=true:format=HH\:mm\:ss}|${level:uppercase=true}|${message}";
            nlogConfiguration.RemoveTarget(consoleTarget.Name);
            nlogConfiguration.AddTarget(consoleTarget.Name, consoleTarget);
            var rule = new LoggingRule(loggerNamePattern, NLog.LogLevel.Info, NLog.LogLevel.Fatal, consoleTarget);
            nlogConfiguration.LoggingRules.Add(rule);
            return nlogConfiguration;
        }

        /// <summary>
        /// Adds a NLog logger named 'NLogLogger' to the factory.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
        /// <param name="nLoggingConfiguration">Add NLog logging configuration.</param>
        public static ILoggingBuilder AddNLogLogger(this ILoggingBuilder builder, LoggingConfiguration nLoggingConfiguration = null)
        {
            var provider = new NLogLoggerProvider();
            if(nLoggingConfiguration != null)
            {
                provider.SetupLoggingConfiguration(nLoggingConfiguration);
            }
            builder.AddProvider(provider);
            return builder;
        }

        /// <summary>
        /// Adds a NLog logger named 'NLogLogger' to the factory.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
        /// <param name="nLoggingConfiguration">Add NLog logging configuration.</param>
        /// <param name="acceptedCategoryNames">The list of accepted category names.</param>
        /// <param name="minLevel">The logging severity level.</param>
        /// <param name="filter">The filter based on the log level and category name.</param>
        public static ILoggingBuilder AddNLogLogger(this ILoggingBuilder builder, List<string> acceptedCategoryNames, LoggingConfiguration nLoggingConfiguration = null, LogLevel? minLevel = null, Func<string, LogLevel, bool> filter = null)
        {
            NLogLoggerSettings nLogSettings = new NLogLoggerSettings()
            {
                AcceptedCategoryNames = new List<string>(acceptedCategoryNames),
                MinLevel = minLevel,
                Filter = filter
            };
            var provider = new NLogLoggerProvider(nLogSettings);
            if (nLoggingConfiguration != null)
            {
                provider.SetupLoggingConfiguration(nLoggingConfiguration);
            }
            builder.AddProvider(provider);
            return builder;
        }

        /// <summary>
        /// Adds a NLog logger named 'NLogLogger' to the factory.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to use.</param>
        /// <param name="nLoggingConfiguration">Add NLog logging configuration.</param>
        /// <param name="acceptedCategoryNames">The list of accepted category names.</param>
        /// <param name="minLevel">The logging severity level.</param>
        /// <param name="filter">The filter based on the log level and category name.</param>
        public static IServiceCollection AddNLogLogger(this IServiceCollection services, List<string> acceptedCategoryNames, LoggingConfiguration nLoggingConfiguration = null, LogLevel? minLevel = null, Func<string, LogLevel, bool> filter = null)
        {
            NLogLoggerSettings nLogSettings = new NLogLoggerSettings()
            {
                AcceptedCategoryNames = new List<string>(acceptedCategoryNames),
                MinLevel = minLevel,
                Filter = filter
            };
            var provider = new NLogLoggerProvider(nLogSettings);
            if (nLoggingConfiguration != null)
            {
                provider.SetupLoggingConfiguration(nLoggingConfiguration);
            }
            services.AddSingleton(provider);
            return services;
        }

        /// <summary>
        /// Adds a NLog logger named 'NLogLogger' to the factory.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
        /// <param name="nLoggingConfiguration">Add NLog logging configuration.</param>
        /// <param name="configurationSection">The configuration section that maps to <see cref="NLogLoggerSettings"/></param>
        public static ILoggingBuilder AddNLogLogger(this ILoggingBuilder builder, IConfigurationSection configurationSection, LoggingConfiguration nLoggingConfiguration = null)
        {
            NLogLoggerSettings nLogSettings = configurationSection != null ? configurationSection.Get<NLogLoggerSettings>():new NLogLoggerSettings();
            var provider = new NLogLoggerProvider(nLogSettings);
            if (nLoggingConfiguration != null)
            {
                provider.SetupLoggingConfiguration(nLoggingConfiguration);
            }
            builder.AddProvider(provider);
            return builder;
        }

        /// <summary>
        /// Adds a NLog logger named 'NLogLogger' to the factory.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to use.</param>
        /// <param name="nLoggingConfiguration">Add NLog logging configuration.</param>
        /// <param name="configurationSection">The configuration section that maps to <see cref="NLogLoggerSettings"/></param>
        public static IServiceCollection AddNLogLogger(this IServiceCollection services, IConfigurationSection configurationSection, LoggingConfiguration nLoggingConfiguration = null)
        {
            NLogLoggerSettings nLogSettings = configurationSection != null ? configurationSection.Get<NLogLoggerSettings>() : new NLogLoggerSettings();
            var provider = new NLogLoggerProvider(nLogSettings);
            if (nLoggingConfiguration != null)
            {
                provider.SetupLoggingConfiguration(nLoggingConfiguration);
            }
            services.AddSingleton(provider);
            return services;
        }
    }
}
