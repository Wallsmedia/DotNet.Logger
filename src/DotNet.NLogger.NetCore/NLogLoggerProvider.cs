// \\     |/\  /||
//  \\ \\ |/ \/ ||
//   \//\\/|  \ || 
// Copyright © Artem Paskhin 2018. All rights reserved.
// Wallsmedia LTD 2018:{Artem Paskhin}
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
//
//  Dot Net NLog  logger wrapper for Microsoft.Extensions.Logging.

using System;
using NLog.Config;
using Microsoft.Extensions.Logging;
using NLog;
using System.Collections.Generic;

namespace DotNet.NLogger.NetCore
{

    /// <summary>
    /// The provider for the <see cref="NLogLoggerProvider"/>.
    /// </summary>
    [ProviderAlias("NLog")]
    public class NLogLoggerProvider : ILoggerProvider, ISupportExternalScope
    {
        NLogLoggerSettings _nLogSettings;

        IExternalScopeProvider _scopeProvider;

        /// <summary>
        /// Contains the created loggers
        /// </summary>
        public Dictionary<string, WeakReference<NLogLogger>> NLogLoggers { get; } = new Dictionary<string, WeakReference<NLogLogger>>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// Initializes a new instance of the <see cref="NLogLoggerProvider"/> class.
        /// </summary>
        public NLogLoggerProvider() : this(new NLogLoggerSettings())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NLogLoggerProvider"/> class.
        /// </summary>
        /// <param name="nLogSettings">The logger settings <see cref="NLogLoggerSettings"/></param>
        public NLogLoggerProvider(NLogLoggerSettings nLogSettings)
        {
            _nLogSettings = nLogSettings ?? throw new ArgumentNullException(nameof(nLogSettings));
        }

        /// <summary>
        /// Setups xml logging configuration 
        /// </summary>
        /// <param name="loggingCfg">The configuration.</param>
        public void SetupLoggingConfiguration(LoggingConfiguration loggingCfg)
        {
            LogManager.Configuration = loggingCfg;
        }

        /// <inheritdoc />
        public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
        {
            if (_nLogSettings.AcceptedCategoryNames.Count == 0 && _nLogSettings.AcceptedAliasesCategoryNames.Count == 0)
            {
                var logger = new NLogLogger(categoryName, _nLogSettings, _scopeProvider ?? new LoggerExternalScopeProvider());
                NLogLoggers[categoryName] = new WeakReference<NLogLogger>(logger);
                return logger;
            }

            string tmpName = MatchToPattern(categoryName, _nLogSettings.AcceptedCategoryNames);
            if (tmpName != string.Empty)
            {
                var tmpAliasName = MatchToPattern(categoryName, _nLogSettings.AcceptedAliasesCategoryNames.Keys);
                if (tmpAliasName == string.Empty)
                {
                    var logger = new NLogLogger(categoryName, _nLogSettings, _scopeProvider ?? new LoggerExternalScopeProvider());
                    NLogLoggers[categoryName] = new WeakReference<NLogLogger>(logger);
                    return logger;
                }
                else
                {
                    tmpAliasName = _nLogSettings.AcceptedAliasesCategoryNames[tmpAliasName];
                    var logger = new NLogLogger(tmpAliasName, _nLogSettings, _scopeProvider ?? new LoggerExternalScopeProvider());
                    NLogLoggers[categoryName] = new WeakReference<NLogLogger>(logger);
                    return logger;
                }
            }

            tmpName = MatchToPattern(categoryName, _nLogSettings.AcceptedAliasesCategoryNames.Keys);
            if (tmpName != string.Empty)
            {
                var tmpAliasName = _nLogSettings.AcceptedAliasesCategoryNames[tmpName];
                var logger = new NLogLogger(tmpAliasName, _nLogSettings, _scopeProvider ?? new LoggerExternalScopeProvider());
                NLogLoggers[categoryName] = new WeakReference<NLogLogger>(logger);
                return logger;
            }

            return Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        }

        public string MatchToPattern(string categoryName, IEnumerable<string> namePatterns)
        {

            foreach (var namePattern in namePatterns)
            {
                if (string.Compare(namePattern, categoryName, true) == 0)
                {
                    return namePattern;
                }
                else if (namePattern.Length > 1)
                {
                    if (namePattern[0] == '*')
                    {
                        string tmp = namePattern.Replace("*", "");
                        if (categoryName.EndsWith(tmp, StringComparison.InvariantCultureIgnoreCase))
                        {
                            return namePattern;
                        }
                    }
                    else if (namePattern[namePattern.Length - 1] == '*')
                    {
                        string tmp = namePattern.Replace("*", "");
                        if (categoryName.StartsWith(tmp, StringComparison.InvariantCultureIgnoreCase))
                        {
                            return namePattern;
                        }
                    }
                }
            }
            return string.Empty;
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
