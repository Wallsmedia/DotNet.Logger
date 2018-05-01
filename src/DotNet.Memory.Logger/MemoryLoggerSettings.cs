// \\     |/\  /||
//  \\ \\ |/ \/ ||
//   \//\\/|  \ || 
// Copyright © Artem Paskhin 2018. All rights reserved.
// Wallsmedia LTD 2018:{Artem Paskhin}
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Dot Net Memory logger for Microsoft.Extensions.Logging.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.Extensions.Logging;

namespace DotNet.Memory.Logger
{
    /// <summary>
    /// Settings for <see cref="MemoryLogger"/>.
    /// </summary>
    [DataContract]
    public class MemoryLoggerSettings
    {
        /// <summary>
        /// The list of accepted category names. If it is empty all categories will be accepted.
        /// The name can have the wild char '*' at the end, which means "start with"; or at the beginning , which means "end with".
        /// </summary>
        [DataMember]
        public List<string> AcceptedCategoryNames { get; set; } = new List<string>();

        /// <summary>
        /// Sets or gets the memory log capacity size.
        /// </summary>
        [DataMember]
        public int MemoryCacheSize { get; set; } = 1024;

        /// <summary>
        /// Gets or sets the logging severity level.
        /// </summary>
        [DataMember]
        public Microsoft.Extensions.Logging.LogLevel? MinLevel { get; set; }

        /// <summary>
        /// The function used to filter events based on the log level and category name.
        /// </summary>
        [IgnoreDataMember]
        public Func<string, LogLevel, bool> Filter { get; set; }

    }
}
