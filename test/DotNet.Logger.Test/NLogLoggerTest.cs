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

            Assert.AreEqual<string>("f*", prv.MatchToPattern("FaKeNamE", new string[] { "f*" }));
            Assert.AreEqual<string>("*name", prv.MatchToPattern("FaKeNamE", new string[] { "*name" }));
            Assert.AreEqual<string>("FAKENAME", prv.MatchToPattern("FaKeNamE", new string[] { "FAKENAME" }));
            Assert.AreEqual<string>("", prv.MatchToPattern("", new string[] { "FAKENAME" }));
            Assert.AreEqual<string>("", prv.MatchToPattern("FaKeNamE", new string[] { }));

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
