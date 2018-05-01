// \\     |/\  /||
//  \\ \\ |/ \/ ||
//   \//\\/|  \ || 
// Copyright © Artem Paskhin 2018. All rights reserved.
// Wallsmedia LTD 2018:{Artem Paskhin}
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Dot Net Memory logger for Microsoft.Extensions.Logging.

using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System;
using Microsoft.Extensions.Configuration;
using DotNet.Memory.Logger;
using Microsoft.Extensions.DependencyInjection;

namespace DotNet.Memory.NetCore
{
    /// <summary>
    /// Setups Memory Log configuration and other settings.
    /// </summary>
    public static class MemoryLoggerExtentions
    {

        /// <summary>
        /// Adds a Memory logger named 'MemoryLogger' to the factory.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
        public static ILoggingBuilder AddMemoryLogger(this ILoggingBuilder builder)
        {
            var provider = new MemoryLoggerProvider();
            builder.AddProvider(provider);
            return builder;
        }

        /// <summary>
        /// Adds a Memory logger named 'MemoryLogger' to the factory.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
        /// <param name="acceptedCategoryNames">The list of accepted category names.</param>
        /// <param name="minLevel">The logging severity level.</param>
        /// <param name="filter">The filter based on the log level and category name.</param>
        public static ILoggingBuilder AddMemoryLogger(this ILoggingBuilder builder, List<string> acceptedCategoryNames,  LogLevel? minLevel = null, Func<string, LogLevel, bool> filter = null)
        {
            MemoryLoggerSettings mLogSettings = new MemoryLoggerSettings()
            {
                AcceptedCategoryNames = new List<string>(acceptedCategoryNames),
                MinLevel = minLevel,
                Filter = filter
            };
            var provider = new MemoryLoggerProvider(mLogSettings);
            builder.AddProvider(provider);
            return builder;
        }

        /// <summary>
        /// Adds a Memory logger named 'MemoryLogger' to the factory.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
        /// <param name="configurationSection">The configuration section that maps to <see cref="MemoryLoggerSettings"/></param>
        public static ILoggingBuilder AddMemoryLogger(this ILoggingBuilder builder, IConfigurationSection configurationSection)
        {
            MemoryLoggerSettings mLogSettings = configurationSection != null ? configurationSection.Get<MemoryLoggerSettings>() : new MemoryLoggerSettings();
            var provider = new MemoryLoggerProvider(mLogSettings);
            builder.AddProvider(provider);
            return builder;
        }

        /// <summary>
        /// Adds a Memory logger named 'MemoryLogger' to the service collection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to use.</param>
        public static IServiceCollection AddMemoryLogger(this IServiceCollection services)
        {
            var provider = new MemoryLoggerProvider();
            services.AddSingleton(provider);
            return services;
        }

        /// <summary>
        /// Adds a Memory logger named 'MemoryLogger' to the service collection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to use.</param>
        /// <param name="acceptedCategoryNames">The list of accepted category names.</param>
        /// <param name="minLevel">The logging severity level.</param>
        /// <param name="filter">The filter based on the log level and category name.</param>
        public static IServiceCollection AddMemoryLogger(this IServiceCollection services, List<string> acceptedCategoryNames, LogLevel? minLevel = null, Func<string, LogLevel, bool> filter = null)
        {
            MemoryLoggerSettings mLogSettings = new MemoryLoggerSettings()
            {
                AcceptedCategoryNames = new List<string>(acceptedCategoryNames),
                MinLevel = minLevel,
                Filter = filter
            };
            var provider = new MemoryLoggerProvider(mLogSettings);
            services.AddSingleton(provider);
            return services;
        }

        /// <summary>
        /// Adds a Memory logger named 'MemoryLogger' to the service collection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to use.</param>
        /// <param name="configurationSection">The configuration section that maps to <see cref="MemoryLoggerSettings"/></param>
        public static IServiceCollection AddMemoryLogger(this IServiceCollection services, IConfigurationSection configurationSection)
        {
            MemoryLoggerSettings mLogSettings = configurationSection != null ? configurationSection.Get<MemoryLoggerSettings>() : new MemoryLoggerSettings();
            var provider = new MemoryLoggerProvider(mLogSettings);
            services.AddSingleton(provider);
            return services;
        }

    }
}
