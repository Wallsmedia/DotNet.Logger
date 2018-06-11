// \\     |/\  /||
//  \\ \\ |/ \/ ||
//   \//\\/|  \ || 
// Copyright © Artem Paskhin 2018. All rights reserved.
// Wallsmedia LTD 2018:{Artem Paskhin}
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Dot Net Memory logger for Microsoft.Extensions.Logging.

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace DotNet.Memory.Logger
{
    /// <summary>
    /// The provider for the <see cref="MemoryLogger"/>.
    /// </summary>
    [ProviderAlias("MLog")]
    public class MemoryLoggerProvider : ILoggerProvider, ISupportExternalScope
    {

        MemoryLoggerSettings _mLogSettings;
        IExternalScopeProvider _scopeProvider;

        /// <summary>
        /// Contains the created loggers
        /// </summary>
        public Dictionary<string, WeakReference<MemoryLogger>> MemoryLoggers { get; } = new Dictionary<string, WeakReference<MemoryLogger>>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryLoggerProvider"/> class.
        /// </summary>
        public MemoryLoggerProvider() : this(new MemoryLoggerSettings())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryLogger"/> class.
        /// </summary>
        /// <param name="memoryLoggerSettings">The logger settings <see cref="MemoryLoggerSettings"/></param>
        public MemoryLoggerProvider(MemoryLoggerSettings memoryLoggerSettings)
        {
            _mLogSettings = memoryLoggerSettings ?? throw new ArgumentNullException(nameof(memoryLoggerSettings));
        }

        /// <inheritdoc />
        public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
        {
            if (_mLogSettings.AcceptedCategoryNames.Count == 0)
            {
                var logger = new MemoryLogger(categoryName, _mLogSettings, _scopeProvider ?? new LoggerExternalScopeProvider());
                MemoryLoggers[categoryName] = new WeakReference<MemoryLogger>(logger);
                return logger;
            }

            foreach (var namePattern in _mLogSettings.AcceptedCategoryNames)
            {
                bool match = false;
                if (string.Compare(namePattern, categoryName, true) == 0)
                {
                    match = true;
                }
                else if (namePattern.Length > 1)
                {
                    if ((namePattern.Length > 2) && (namePattern[0] == '*') && (namePattern[namePattern.Length - 1] == '*'))
                    {
                        string tmp = namePattern.Replace("*", "");
                        match = categoryName.ToLower().Contains(tmp.ToLower());
                    }
                    else if (namePattern[0] == '*')
                    {
                        string tmp = namePattern.Replace("*", "");
                        match = categoryName.StartsWith(tmp, StringComparison.InvariantCultureIgnoreCase);
                    }
                    else if (namePattern[namePattern.Length - 1] == '*')
                    {
                        string tmp = namePattern.Replace("*", "");
                        match = categoryName.StartsWith(tmp, StringComparison.InvariantCultureIgnoreCase);
                    }

                }
                if (match)
                {
                    var logger = new MemoryLogger(categoryName, _mLogSettings, _scopeProvider ?? new LoggerExternalScopeProvider());
                    MemoryLoggers[categoryName] = new WeakReference<MemoryLogger>(logger);
                    return logger;
                }
            }

            return Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }

        /// <summary>
        /// Sets the new <see cref="IExternalScopeProvider"/> instance for new, but not for already created loggers.
        /// </summary>
        /// <param name="scopeProvider"></param>
        public void SetScopeProvider(IExternalScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

    }
}
