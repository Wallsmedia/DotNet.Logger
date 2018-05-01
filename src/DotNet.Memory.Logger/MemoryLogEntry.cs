// \\     |/\  /||
//  \\ \\ |/ \/ ||
//   \//\\/|  \ || 
// Copyright © Artem Paskhin 2018. All rights reserved.
// Wallsmedia LTD 2018:{Artem Paskhin}
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Dot Net Memory logger for Microsoft.Extensions.Logging.

using System;
using Microsoft.Extensions.Logging;

namespace DotNet.Memory.Logger
{
    /// <summary>
    /// The memory logger stored entry.
    /// </summary>
    public class MemoryLogEntry
    {

        /// <summary>
        /// Gets or sets the log entry time stamp.
        /// </summary>
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the log level.
        /// </summary>
        public LogLevel LogLevel { get; set; }

        /// <summary>
        /// Gets or sets the logger name.
        /// </summary>
        public string LogName { get; set; }

        /// <summary>
        /// Gets or sets the event id.
        /// </summary>
        public EventId EventId { get; set; }

        /// <summary>
        /// Gets or sets the log message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the scope string.
        /// </summary>
        public string Scope { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Exception"/> in case of the exception.
        /// </summary>
        public Exception Exception { get; set; }
    }
}