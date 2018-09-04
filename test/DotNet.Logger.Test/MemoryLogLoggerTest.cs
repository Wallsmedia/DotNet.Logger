// \\     |/\  /||
//  \\ \\ |/ \/ ||
//   \//\\/|  \ || 
// Copyright © Artem & Alexander Paskhin 2013-2018. All rights reserved.
// Wallsmedia LTD 2018:{Artem & Alexander Paskhin}
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Dot NET Memory Logger Test

using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DotNet.Memory.Logger.Test
{
    [TestClass]
    public class MemoryLogLoggerTest
    {

        [TestMethod]
        public void CreateNullLogger()
        {
            LoggerFactory factory = new LoggerFactory();
            MemoryLoggerSettings memoryLoggerSettings = new MemoryLoggerSettings
            {
                MemoryCacheSize = 10,
                AcceptedCategoryNames = new System.Collections.Generic.List<string> { nameof(MemoryLogLoggerTest) }
            };
            MemoryLoggerProvider memoryLoggerProvider = new MemoryLoggerProvider(memoryLoggerSettings);
            factory.AddProvider(memoryLoggerProvider);

            // Create fake logger
            ILogger logger = factory.CreateLogger("Fake");
            Assert.AreEqual(0, memoryLoggerProvider.MemoryLoggers.Count);
        }

        [TestMethod]
        public void CreateAnyLogger()
        {
            LoggerFactory factory = new LoggerFactory();
            MemoryLoggerSettings memoryLoggerSettings = new MemoryLoggerSettings
            {
                MemoryCacheSize = 10,
            };
            MemoryLoggerProvider memoryLoggerProvider = new MemoryLoggerProvider(memoryLoggerSettings);
            factory.AddProvider(memoryLoggerProvider);

            // Create fake logger
            ILogger logger = factory.CreateLogger("Fake");
            Assert.AreEqual(1, memoryLoggerProvider.MemoryLoggers.Count);
        }

        [TestMethod]
        public void CreateMemoryLogger()
        {
            LoggerFactory factory = new LoggerFactory();
            string category = String.Empty;
            LogLevel logLevel = LogLevel.None;
            MemoryLoggerSettings memoryLoggerSettings = new MemoryLoggerSettings
            {
                MemoryCacheSize = 10,
                AcceptedCategoryNames = new System.Collections.Generic.List<string> { nameof(MemoryLogLoggerTest) },
                Filter = ( s, l) => { category = s; logLevel = l; return true; }
            };
            MemoryLoggerProvider memoryLoggerProvider = new MemoryLoggerProvider(memoryLoggerSettings);
            factory.AddProvider(memoryLoggerProvider);

            // Create real logger
            ILogger logger = factory.CreateLogger(nameof(MemoryLogLoggerTest));
            Assert.AreEqual(1, memoryLoggerProvider.MemoryLoggers.Count);

            // Send the log message
            EventId eventId = new EventId(1, nameof(EventId));
            Exception exception = new Exception();
            string message = "Message-{param}";
            object[] args = new object[] { "param" };

            logger.LogDebug(eventId, exception, message, args);

            MemoryLogger lst;
            Assert.IsTrue(memoryLoggerProvider.MemoryLoggers[nameof(MemoryLogLoggerTest)].TryGetTarget(out lst));

            // check the filter
            Assert.AreEqual(nameof(MemoryLogLoggerTest), category);
            Assert.AreEqual(LogLevel.Debug, logLevel);

            // check message 
            MemoryLogEntry msg;
            Assert.IsTrue(lst.LogMessages.TryPeek(out msg));

            // check correct message
            Assert.AreEqual(eventId, msg.EventId);
            Assert.AreEqual(exception, msg.Exception);
            Assert.AreEqual("Message-param", msg.Message);
            Assert.AreEqual(LogLevel.Debug,msg.LogLevel);
            Assert.AreEqual(category, msg.LogName);

        }

        [TestMethod]
        public void CheckLogCapacity()
        {
            LoggerFactory factory = new LoggerFactory();
            string category = String.Empty;
            LogLevel logLevel = LogLevel.None;
            MemoryLoggerSettings memoryLoggerSettings = new MemoryLoggerSettings
            {
                MemoryCacheSize = 10,
                AcceptedCategoryNames = new System.Collections.Generic.List<string> { nameof(MemoryLogLoggerTest) },
            };
            MemoryLoggerProvider memoryLoggerProvider = new MemoryLoggerProvider(memoryLoggerSettings);
            factory.AddProvider(memoryLoggerProvider);

            // Create real logger
            ILogger logger = factory.CreateLogger(nameof(MemoryLogLoggerTest));

            // Send the log message
            EventId eventId = new EventId(1, nameof(EventId));
            Exception exception = new Exception();
            string message = "Message-{param}";
            object[] args = new object[] { "param" };

            for (int i = 0; i < memoryLoggerSettings.MemoryCacheSize + memoryLoggerSettings.MemoryCacheSize; i++)
            {
                logger.LogDebug(eventId, exception, message, args);
            }

            MemoryLogger lst;
            Assert.IsTrue(memoryLoggerProvider.MemoryLoggers[nameof(MemoryLogLoggerTest)].TryGetTarget(out lst));
            Assert.AreEqual(memoryLoggerSettings.MemoryCacheSize, lst.LogMessages.Count);
        }

    }
}
