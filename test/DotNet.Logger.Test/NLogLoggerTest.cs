// \\     |/\  /||
//  \\ \\ |/ \/ ||
//   \//\\/|  \ || 
// Copyright © Artem Paskhin 2013-2017. All rights reserved.
// Wallsmedia LTD 2018:{Artem Paskhin}
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Dot NET NLog Logger Test

using DotNet.NLogger.NetCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DotNet.Memory.Logger.Test
{
    [TestClass]
    public class NLogLoggerTest
    {

        [TestMethod]
        public void CreateNullLogger()
        {
            LoggerFactory factory = new LoggerFactory();
            NLogLoggerSettings nLogLoggerSettings = new NLogLoggerSettings
            {
                AcceptedCategoryNames = new System.Collections.Generic.List<string> { nameof(NLogLoggerTest) }
            };
            NLogLoggerProvider nLogLoggerProvider = new NLogLoggerProvider(nLogLoggerSettings);
            factory.AddProvider(nLogLoggerProvider);

            // Create fake logger
            ILogger logger = factory.CreateLogger("Fake");
            Assert.AreEqual(0, nLogLoggerProvider.NLogLoggers.Count);
        }

        [TestMethod]
        public void CreateAnyLogger()
        {
            LoggerFactory factory = new LoggerFactory();
            NLogLoggerSettings nLogLoggerSettings = new NLogLoggerSettings
            {
            };
            NLogLoggerProvider nLogLoggerProvider = new NLogLoggerProvider(nLogLoggerSettings);
            factory.AddProvider(nLogLoggerProvider);

            // Create fake logger
            ILogger logger = factory.CreateLogger("Fake");
            Assert.AreEqual(1, nLogLoggerProvider.NLogLoggers.Count);
        }

        [TestMethod]
        public void CreateMemoryLogger()
        {
            LoggerFactory factory = new LoggerFactory();
            string category = String.Empty;
            LogLevel logLevel = LogLevel.None;
            NLogLoggerSettings nLogLoggerSettings = new NLogLoggerSettings
            {

                AcceptedCategoryNames = new System.Collections.Generic.List<string> { nameof(NLogLoggerTest) },
                Filter = (s, l) => { category = s; logLevel = l; return true; }
            };
            NLogLoggerProvider nLogLoggerProvider = new NLogLoggerProvider(nLogLoggerSettings);
            factory.AddProvider(nLogLoggerProvider);

            // Create real logger
            ILogger logger = factory.CreateLogger(nameof(NLogLoggerTest));
            Assert.AreEqual(1, nLogLoggerProvider.NLogLoggers.Count);

            // Send the log message
            EventId eventId = new EventId(1, nameof(EventId));
            Exception exception = new Exception();
            string message = "Message-{param}";
            object[] args = new object[] { "param" };

            logger.LogDebug(eventId, exception, message, args);

            NLogLogger lst;
            Assert.IsTrue(nLogLoggerProvider.NLogLoggers[nameof(NLogLoggerTest)].TryGetTarget(out lst));

            // check the filter
            Assert.AreEqual(nameof(NLogLoggerTest), category);
            Assert.AreEqual(LogLevel.Debug, logLevel);

        }

        [TestMethod]
        public void TestOfCategoryNameMatcher()
        {
            NLogLoggerProvider prv = new NLogLoggerProvider();

            Assert.AreEqual<(MatchPatternResult, string)>((MatchPatternResult.StartWith, "f*"), prv.MatchToPatternList("FaKeNamE", new List<string>() { "f*" }));
            Assert.AreEqual<(MatchPatternResult, string)>((MatchPatternResult.EndWith, "*name"), prv.MatchToPatternList("FaKeNamE", new List<string>() { "*name" }));
            Assert.AreEqual<(MatchPatternResult, string)>((MatchPatternResult.Contains, "*ENA*"), prv.MatchToPatternList("FaKeNamE", new List<string>() { "*ENA*" }));
            Assert.AreEqual<(MatchPatternResult, string)>((MatchPatternResult.Exact, "FAKENAME"), prv.MatchToPatternList("FaKeNamE", new List<string>() { "FAKENAME" }));
            Assert.AreEqual<(MatchPatternResult, string)>((MatchPatternResult.WildMatch, "*"), prv.MatchToPatternList("werftg", new List<string>() { "*" }));
            Assert.AreEqual<(MatchPatternResult, string)>((MatchPatternResult.None, string.Empty), prv.MatchToPatternList("", new List<string>() { "FAKENAME" }));
            Assert.AreEqual<(MatchPatternResult, string)>((MatchPatternResult.None, string.Empty), prv.MatchToPatternList("", new List<string>() { "FAKENAME" }));
            Assert.AreEqual<(MatchPatternResult, string)>((MatchPatternResult.None, string.Empty), prv.MatchToPatternList("FaKeNamE", new List<string>() { }));


            Assert.AreEqual<(MatchPatternResult, string, string)>((MatchPatternResult.StartWith, "f*", "Map"), prv.MatchToMappingPattern("FaKeNamE", new Dictionary<string, string>() { ["f*"] = "Map" }));
            Assert.AreEqual<(MatchPatternResult, string, string)>((MatchPatternResult.EndWith, "*name", "Map"), prv.MatchToMappingPattern("FaKeNamE", new Dictionary<string, string>() { ["*name"] = "Map" }));
            Assert.AreEqual<(MatchPatternResult, string, string)>((MatchPatternResult.Contains, "*ENA*", "Map"), prv.MatchToMappingPattern("FaKeNamE", new Dictionary<string, string>() { ["*ENA*"] = "Map" }));
            Assert.AreEqual<(MatchPatternResult, string, string)>((MatchPatternResult.Exact, "FAKENAME", "Map"), prv.MatchToMappingPattern("FaKeNamE", new Dictionary<string, string>() { ["FAKENAME"] = "Map" }));
            Assert.AreEqual<(MatchPatternResult, string, string)>((MatchPatternResult.WildMatch, "*", "Map"), prv.MatchToMappingPattern("werftg", new Dictionary<string, string>() { ["*"] = "Map" }));
            Assert.AreEqual<(MatchPatternResult, string, string)>((MatchPatternResult.None, string.Empty, string.Empty), prv.MatchToMappingPattern("", new Dictionary<string, string>() { ["FAKENAME"] = "Map" }));
            Assert.AreEqual<(MatchPatternResult, string, string)>((MatchPatternResult.None, string.Empty, string.Empty), prv.MatchToMappingPattern("", new Dictionary<string, string>() { ["FAKENAME"] = "Map" }));
            Assert.AreEqual<(MatchPatternResult, string, string)>((MatchPatternResult.None, string.Empty, string.Empty), prv.MatchToMappingPattern("FaKeNamE", new Dictionary<string, string>()));

        }

        [TestMethod]
        public void CreateAnyLoggerStartsAndEndsWith()
        {
            LoggerFactory factory = new LoggerFactory();
            NLogLoggerSettings nLogLoggerSettings = new NLogLoggerSettings
            {
            };
            nLogLoggerSettings.AcceptedCategoryNames = new List<string> { "f*", "*name" };

            NLogLoggerProvider nLogLoggerProvider = new NLogLoggerProvider(nLogLoggerSettings);

            var logger = nLogLoggerProvider.CreateLogger("fAbracadabra") as NLogLogger;
            Assert.IsInstanceOfType(logger, typeof(NLogLogger));
            Assert.AreEqual("fAbracadabra", logger.CategoryName);

            logger = nLogLoggerProvider.CreateLogger("RoundNAmE") as NLogLogger;
            Assert.IsInstanceOfType(logger, typeof(NLogLogger));
            Assert.AreEqual("RoundNAmE", logger.CategoryName);
        }

        [TestMethod]
        public void CreateAnyLoggerWithAlias()
        {
            LoggerFactory factory = new LoggerFactory();
            NLogLoggerSettings nLogLoggerSettings = new NLogLoggerSettings
            {
            };
            nLogLoggerSettings.AcceptedAliasesCategoryNames = new Dictionary<string, string> { { "f*", "Remap1" }, { "*name", "Remap2" } };

            NLogLoggerProvider nLogLoggerProvider = new NLogLoggerProvider(nLogLoggerSettings);

            var logger = nLogLoggerProvider.CreateLogger("fAbracadabra") as NLogLogger;
            Assert.IsInstanceOfType(logger, typeof(NLogLogger));
            Assert.AreEqual("Remap1", logger.CategoryName);

            logger = nLogLoggerProvider.CreateLogger("RoundNAmE") as NLogLogger;
            Assert.IsInstanceOfType(logger, typeof(NLogLogger));
            Assert.AreEqual("Remap2", logger.CategoryName);
        }

        [TestMethod]
        public void CreateAnyLoggerWithAlias2()
        {
            LoggerFactory factory = new LoggerFactory();
            NLogLoggerSettings nLogLoggerSettings = new NLogLoggerSettings
            {
            };
            nLogLoggerSettings.AcceptedCategoryNames = new List<string> { "f*", "*name" };
            nLogLoggerSettings.AcceptedAliasesCategoryNames = new Dictionary<string, string> { { "f*", "Remap1" }, { "*name", "Remap2" } };

            NLogLoggerProvider nLogLoggerProvider = new NLogLoggerProvider(nLogLoggerSettings);

            var logger = nLogLoggerProvider.CreateLogger("fAbracadabra") as NLogLogger;
            Assert.IsInstanceOfType(logger, typeof(NLogLogger));
            Assert.AreEqual("Remap1", logger.CategoryName);

            logger = nLogLoggerProvider.CreateLogger("RoundNAmE") as NLogLogger;
            Assert.IsInstanceOfType(logger, typeof(NLogLogger));
            Assert.AreEqual("Remap2", logger.CategoryName);
        }

        [TestMethod]
        public void CreateAnyLoggerWithAlias2Negative()
        {
            LoggerFactory factory = new LoggerFactory();
            NLogLoggerSettings nLogLoggerSettings = new NLogLoggerSettings
            {
            };
            nLogLoggerSettings.AcceptedCategoryNames = new List<string> { "f*", "*name" };
            nLogLoggerSettings.AcceptedAliasesCategoryNames = new Dictionary<string, string> { { "f*", "Remap1" }, { "*name", "Remap2" } };

            NLogLoggerProvider nLogLoggerProvider = new NLogLoggerProvider(nLogLoggerSettings);

            var logger = nLogLoggerProvider.CreateLogger("Abracadabra");
            Assert.IsInstanceOfType(logger, typeof(Microsoft.Extensions.Logging.Abstractions.NullLogger));

            logger = nLogLoggerProvider.CreateLogger("RoundNAmEs");
            Assert.IsInstanceOfType(logger, typeof(Microsoft.Extensions.Logging.Abstractions.NullLogger));

        }
    }
}
