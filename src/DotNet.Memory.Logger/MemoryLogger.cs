// \\     |/\  /||
//  \\ \\ |/ \/ ||
//   \//\\/|  \ || 
// Copyright © Artem & Alexander Paskhin 2018. All rights reserved.
// Wallsmedia LTD 2018:{Artem & Alexander Paskhin}
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Dot Net Memory logger for Microsoft.Extensions.Logging.

using System;
using System.Collections.Concurrent;
using System.Text;
using Microsoft.Extensions.Logging;

namespace DotNet.Memory.Logger
{
    /// <summary>
    /// The memory logger that cache some messages into a queue.
    /// </summary>
    public class MemoryLogger : ILogger
    {
        [ThreadStatic]
        StringBuilder _logBuilder;
        IExternalScopeProvider _scopeProvider;
        MemoryLoggerSettings _settings;

        /// <summary>
        /// Logged messages buffer/queue 
        /// </summary>
        public ConcurrentQueue<MemoryLogEntry> LogMessages { get; } = new ConcurrentQueue<MemoryLogEntry>();

        /// <summary>
        /// The logger category name.
        /// </summary>
        public string CategoryName { get; }

        /// <summary>
        /// Constructs the instance of <see cref="MemoryLogger"/>
        /// </summary>
        /// <param name="categoryName">The category name.</param>
        /// <param name="memoryLoggerSettings">The memory logger settings.</param>
        /// <param name="externalScopeProvider">The scope data provider.</param>
        public MemoryLogger(string categoryName, MemoryLoggerSettings memoryLoggerSettings, IExternalScopeProvider externalScopeProvider)
        {
            CategoryName = categoryName;
            _settings = memoryLoggerSettings ?? throw new ArgumentNullException(nameof(memoryLoggerSettings));
            _scopeProvider = externalScopeProvider ?? throw new ArgumentNullException(nameof(externalScopeProvider));
        }

        /// <summary>
        /// Begins a logical operation scope.
        /// </summary>
        /// <param name="state">The identifier for the scope.</param>
        /// <returns>An IDisposable that ends the logical operation scope on dispose.</returns>
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
        /// <param name="mLogSettings">The memory logger settings</param>
        /// <param name="level">level to be checked.</param>
        /// <returns><c>true</c> if enabled.</returns>
        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel level, MemoryLoggerSettings mLogSettings)
        {
            if (mLogSettings.MinLevel != null && level < mLogSettings.MinLevel)
            {
                return false;
            }

            if (mLogSettings.Filter != null)
            {
                return mLogSettings.Filter(CategoryName, level);
            }

            return true;
        }

        /// <summary>
        /// Default simple message formatter for logger.
        /// </summary>
        /// <typeparam name="TState">The type of logging state object.</typeparam>
        /// <param name="state">The log state object.</param>
        /// <param name="exception">The system exception if exist</param>
        /// <returns>The formatted log string.</returns>
        public static string DefaulttMessageFormatter<TState>(TState state, Exception exception)
        {
            return state?.ToString();
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
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
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
            if (_settings.IncludeScopes)
            {
                int count = GetScopeInformation(logBuilder, _scopeProvider);
                if (count > 0)
                {
                    scopeString = logBuilder.ToString();
                }
            }

            if (!string.IsNullOrEmpty(message) || exception != null)
            {
                WriteMessage(logLevel, CategoryName, eventId, message, scopeString, exception);
            }

            logBuilder.Clear();
            if (logBuilder.Capacity > 1024)
            {
                logBuilder.Capacity = 1024;
            }
            _logBuilder = logBuilder;
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

        /// <summary>
        /// Stores the message log entry.
        /// </summary>
        /// <param name="entry">The log message entry.</param>
        private void PushIntoQueue(MemoryLogEntry entry)
        {
            if (LogMessages.Count >= _settings.MemoryCacheSize)
            {
                MemoryLogEntry result;
                LogMessages.TryDequeue(out result);
            }
            LogMessages.Enqueue(entry);
        }

        /// <summary>
        /// Write a message into the memory log.
        /// </summary>
        /// <param name="logLevel">Entry will be written on this level.</param>v
        /// <param name="logName">The category log name.</param>
        /// <param name="eventId">Id of the event.</param>
        /// <param name="message">The log message.</param>
        /// <param name="scope">The logging scope flag.</param>
        /// <param name="exception">The exception related to this entry.</param>
        public virtual void WriteMessage(LogLevel logLevel, string logName, EventId eventId, string message, string scope, Exception exception)
        {
            MemoryLogEntry logEntry = new MemoryLogEntry()
            {
                EventId = eventId,
                LogLevel = logLevel,
                LogName = logName,
                Scope = scope
            };

            if (!string.IsNullOrEmpty(message))
            {
                logEntry.Message = message;
            }

            if (exception != null)
            {
                logEntry.Exception = exception;
            }
            PushIntoQueue(logEntry);
        }
    }
}