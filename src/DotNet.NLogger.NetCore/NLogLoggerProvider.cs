// \\     |/\  /||
//  \\ \\ |/ \/ ||
//   \//\\/|  \ || 
// Copyright © Artem & Alexander Paskhin 2018. All rights reserved.
// Wallsmedia LTD 2018:{Artem & Alexander Paskhin}
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
    /// The match pattern results.
    /// </summary>
    public enum MatchPatternResult
    {
        /// <summary>
        /// Not match
        /// </summary>
        None,
        /// <summary>
        /// Has exact match
        /// </summary>
        Exact,
        /// <summary>
        /// Has endwith match
        /// </summary>
        EndWith,
        /// <summary>
        /// Has  startwith match
        /// </summary>
        StartWith,
        /// <summary>
        /// Has contains match
        /// </summary>
        Contains,
        /// <summary>
        /// Has wild char match 
        /// </summary>
        WildMatch
    }

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

            (MatchPatternResult result, string pattern) = MatchToPatternList(categoryName, _nLogSettings.AcceptedCategoryNames);
            if (result != MatchPatternResult.None)
            {
                (MatchPatternResult mapResult2, string mapPattern2, string mapTo2) = MatchToMappingPattern(categoryName, _nLogSettings.AcceptedAliasesCategoryNames);
                if (mapResult2 != MatchPatternResult.None)
                {
                    var logger = new NLogLogger(mapTo2, _nLogSettings, _scopeProvider ?? new LoggerExternalScopeProvider());
                    NLogLoggers[categoryName] = new WeakReference<NLogLogger>(logger);
                    return logger;
                }
                else
                {
                    var logger = new NLogLogger(categoryName, _nLogSettings, _scopeProvider ?? new LoggerExternalScopeProvider());
                    NLogLoggers[categoryName] = new WeakReference<NLogLogger>(logger);
                    return logger;
                }
            }
            (MatchPatternResult mapResult, string mapPattern, string mapTo) = MatchToMappingPattern(categoryName, _nLogSettings.AcceptedAliasesCategoryNames);
            if (mapResult != MatchPatternResult.None)
            {
                var logger = new NLogLogger(mapTo, _nLogSettings, _scopeProvider ?? new LoggerExternalScopeProvider());
                NLogLoggers[categoryName] = new WeakReference<NLogLogger>(logger);
                return logger;
            }

            return Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        }



        /// <summary>
        /// Selects the proper category name which will be used as NLog name. 
        /// The category name should be exact match or like "*endwith", or like "startwith*" , or like "*" (all to match).
        /// If pattern "*", so the category name will be returned. 
        /// If pattern "*endwith", so the "endwith" will be returned. 
        /// If pattern "startwith*", so the "startwith" will be returned. 
        /// </summary>
        /// <param name="categoryName">The input category name.</param>
        /// <param name="namePatterns">The list of matching patterns.</param>
        /// <returns>Returns the NLog name or empty string and matching result.</returns>
        public (MatchPatternResult, string) MatchToPatternList(string categoryName, List<string> namePatterns)
        {

            foreach (var pattern in namePatterns)
            {
                var namePattern = pattern.Trim();
                if (string.Compare(namePattern, categoryName, true) == 0)
                {
                    return (MatchPatternResult.Exact, namePattern);
                }
                else if (namePattern.Length > 1)
                {
                    if ((namePattern.Length > 2) && (namePattern[0] == '*') && (namePattern[namePattern.Length - 1] == '*'))
                    {
                        string tmp = namePattern.Replace("*", "");
                        if (categoryName.ToLower().Contains(tmp.ToLower()))
                        {
                            return (MatchPatternResult.Contains, namePattern);
                        }
                    }
                    else if (namePattern[0] == '*')
                    {
                        string tmp = namePattern.Replace("*", "");
                        if (categoryName.EndsWith(tmp, StringComparison.InvariantCultureIgnoreCase))
                        {
                            return (MatchPatternResult.EndWith, namePattern);
                        }
                    }
                    else if (namePattern[namePattern.Length - 1] == '*')
                    {
                        string tmp = namePattern.Replace("*", "");
                        if (categoryName.StartsWith(tmp, StringComparison.InvariantCultureIgnoreCase))
                        {
                            return (MatchPatternResult.StartWith, namePattern);
                        }
                    }
                }
                else if (namePattern.Length == 1 && namePattern[0] == '*')
                {
                    return (MatchPatternResult.WildMatch, "*");
                }
            }
            return (MatchPatternResult.None, string.Empty);
        }

        /// <summary>
        /// Selects the proper category name which will be used as NLog name. 
        /// The category name should be exact match or like "*endwith", or like "startwith*" , or like "*" (all to match).
        /// If pattern "*", so the category name will be returned. 
        /// If pattern "*endwith", so the "endwith" will be returned. 
        /// If pattern "startwith*", so the "startwith" will be returned. 
        /// </summary>
        /// <param name="categoryName">The input category name.</param>
        /// <param name="mapPatterns">The list of matching patterns.</param>
        /// <returns>Returns the NLog name or empty string and matching result.</returns>
        public (MatchPatternResult, string, string) MatchToMappingPattern(string categoryName, Dictionary<string, string> mapPatterns)
        {

            foreach (var mapKvPattern in mapPatterns)
            {
                var pattern = mapKvPattern.Key;
                var mapTo = mapKvPattern.Value;

                if (string.Compare(pattern, categoryName, true) == 0)
                {
                    return (MatchPatternResult.Exact, pattern, mapTo);
                }
                else if (pattern.Length > 1)
                {
                    if ((pattern.Length > 2) && (pattern[0] == '*') && (pattern[pattern.Length - 1] == '*'))
                    {
                        string tmp = pattern.Replace("*", "");
                        if (categoryName.ToLower().Contains(tmp.ToLower()))
                        {
                            return (MatchPatternResult.Contains, pattern, mapTo);
                        }
                    }
                    else if (pattern[0] == '*')
                    {
                        string tmp = pattern.Replace("*", "");
                        if (categoryName.EndsWith(tmp, StringComparison.InvariantCultureIgnoreCase))
                        {
                            return (MatchPatternResult.EndWith, pattern, mapTo);
                        }
                    }
                    else if (pattern[pattern.Length - 1] == '*')
                    {
                        string tmp = pattern.Replace("*", "");
                        if (categoryName.StartsWith(tmp, StringComparison.InvariantCultureIgnoreCase))
                        {
                            return (MatchPatternResult.StartWith, pattern, mapTo);
                        }
                    }
                }
                else if (pattern.Length == 1 && pattern[0] == '*')
                {
                    return (MatchPatternResult.WildMatch, pattern, mapTo);
                }
            }
            return (MatchPatternResult.None, string.Empty, string.Empty);
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
