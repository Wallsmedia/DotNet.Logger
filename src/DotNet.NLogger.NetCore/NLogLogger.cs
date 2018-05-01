// \\     |/\  /||
//  \\ \\ |/ \/ ||
//   \//\\/|  \ || 
// Copyright © Artem Paskhin 2018. All rights reserved.
// Wallsmedia LTD 2018:{Artem Paskhin}
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
//
//  Dot Net NLog  logger wrapper for Microsoft.Extensions.Logging.

using System;
using System.Text;
using Microsoft.Extensions.Logging;
using NLog;

namespace DotNet.NLogger.NetCore
{

    /// <summary>
    /// A logger that writes messages to NLog Logger.
    /// </summary>
    public class NLogLogger : Microsoft.Extensions.Logging.ILogger
    {

        public string CategoryName { get; }
        public Logger Logger { get; }

        /// <summary>
        /// /
        /// </summary>
        /// <param name="nlogger"></param>
        public static implicit operator NLog.Logger(NLogLogger nlogger)
        {
            return nlogger.Logger;
        }

        [ThreadStatic]
        StringBuilder _logBuilder;
        IExternalScopeProvider _scopeProvider;
        NLogLoggerSettings _settings;

        /// <summary>
        /// /
        /// </summary>
        /// <param name="categoryName"></param>
        /// <param name="nLogSettings"></param>
        /// <param name="externalScopeProvider">The scope data provider.</param>
        public NLogLogger(string categoryName, NLogLoggerSettings nLogSettings, IExternalScopeProvider externalScopeProvider)
        {
            CategoryName = categoryName;
            _settings = nLogSettings ?? throw new ArgumentNullException(nameof(nLogSettings));
            _scopeProvider = externalScopeProvider ?? throw new ArgumentNullException(nameof(externalScopeProvider));
            Logger = LogManager.GetLogger(categoryName);
        }

        /// <inheritdoc />
        public IDisposable BeginScope<TState>(TState state)
        {
            return _scopeProvider?.Push(state);
        }

        /// <summary>
        /// Checks if the given <paramref name="logLevel"/> is enabled.
        /// </summary>
        /// <param name="logLevel">level to be checked.</param>
        /// <returns><c>true</c> if enabled.</returns>
        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
        {
            if (logLevel == Microsoft.Extensions.Logging.LogLevel.None)
            {
                return false;
            }

            return IsEnabled(logLevel, _settings);
        }

        /// <summary>
        /// Checks if the given <paramref name="level"/> is enabled.
        /// </summary>
        /// <param name="nLogSettings">The memory logger settings</param>
        /// <param name="level">level to be checked.</param>
        /// <returns><c>true</c> if enabled.</returns>

        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel level, NLogLoggerSettings nLogSettings)
        {
            if (nLogSettings.MinLevel != null && level < nLogSettings.MinLevel)
            {
                return false;
            }

            if (nLogSettings.Filter != null)
            {
                return nLogSettings.Filter(CategoryName, level);
            }

            return true;
        }

        /// <summary>
        /// Writes a log entry.
        /// Usually it has not been used directly.
        /// There are lots of extension methods from "Microsoft.Extensions.Logging" nuget package.
        /// </summary>
        /// <param name="logLevel">Entry will be written on this level.</param>v
        /// <param name="eventId">Id of the event.</param>
        /// <param name="state">The entry to be written. Can be also an object.</param>
        /// <param name="exception">The exception related to this entry.</param>
        /// <param name="formatter">Function to create a <c>string</c> message of the <paramref name="state"/> and <paramref name="exception"/>.</param>
        public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var nLogLogLevel = MapMsLogLevelToNLog(logLevel);
            if (!IsEnabled(logLevel))
            {
                return;
            }

            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            var logBuilder = _logBuilder;
            _logBuilder = null;

            if (logBuilder == null)
            {
                logBuilder = new StringBuilder();
            }

            var message = formatter(state, exception);

            string scopeString = String.Empty;
            int count = GetScopeInformation(logBuilder, _scopeProvider);
            if (count > 0)
            {
                logBuilder.Append(message);
                message = logBuilder.ToString();
            }

            LogEventInfo logEventInfo = LogEventInfo.Create(nLogLogLevel, Logger.Name, exception, null, message);

            Logger.Log(logEventInfo);

            logBuilder.Clear();
            if (logBuilder.Capacity > 1024)
            {
                logBuilder.Capacity = 1024;
            }
            _logBuilder = logBuilder;
        }

        /// <summary>
        /// Maps <see cref="Microsoft.Extensions.Logging.LogLevel"/> to <see cref="NLog.LogLevel"/>.
        /// </summary>
        /// <param name="logLevel">level to be converted.</param>
        /// <returns>The mapped value of <see cref="NLog.LogLevel"/>.</returns>
        private NLog.LogLevel MapMsLogLevelToNLog(Microsoft.Extensions.Logging.LogLevel logLevel)
        {
            switch (logLevel)
            {
                case Microsoft.Extensions.Logging.LogLevel.Trace:
                    return NLog.LogLevel.Trace;
                case Microsoft.Extensions.Logging.LogLevel.Debug:
                    return NLog.LogLevel.Debug;
                case Microsoft.Extensions.Logging.LogLevel.Information:
                    return NLog.LogLevel.Info;
                case Microsoft.Extensions.Logging.LogLevel.Warning:
                    return NLog.LogLevel.Warn;
                case Microsoft.Extensions.Logging.LogLevel.Error:
                    return NLog.LogLevel.Error;
                case Microsoft.Extensions.Logging.LogLevel.Critical:
                    return NLog.LogLevel.Fatal;
                case Microsoft.Extensions.Logging.LogLevel.None:
                    return NLog.LogLevel.Off;
                default:
                    return NLog.LogLevel.Debug;
            }
        }

        /// <summary>
        /// Generates the string representation of a scope.
        /// </summary>
        /// <param name="stringBuilder">The output scope in the builder.</param>
        /// <param name="scopeProvider">The scope data provider.</param>
        /// <returns>The length of the scope.</returns>
        private int GetScopeInformation(StringBuilder stringBuilder, IExternalScopeProvider scopeProvider)
        {
            var initialLength = stringBuilder.Length;
            if (scopeProvider != null)
            {
                scopeProvider.ForEachScope(
                    callback: (scope, state) =>
                    {
                        var (builder, length) = state;
                        var first = length == builder.Length;
                        builder.Append(first ? "=> " : " => ").Append(scope);
                    },
                    state: (stringBuilder, initialLength));

                if (stringBuilder.Length > initialLength)
                {
                    stringBuilder.Append(" |");
                }
            }
            return stringBuilder.Length - initialLength;
        }
    }
}
