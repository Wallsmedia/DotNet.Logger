// \\     |/\  /||
//  \\ \\ |/ \/ ||
//   \//\\/|  \ || 
// Copyright © Artem Paskhin 2018. All rights reserved.
// Wallsmedia LTD 2018:{Artem Paskhin}
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Dot Net Sample logger for Microsoft.Extensions.Logging.

using Microsoft.Extensions.Logging;

namespace RestWebApplication
{
    public static class LogSelector
    {
        public const string CommonInfoLogCategoryName = "CommonInfo";
        public const string ConsoleInfoLogCategoryName = "ConsoleInfo";
        public const string ConsoleErrorLogCategoryName = "ConsoleError";
        public const string FatalErrorLogCategoryName = "FatalError";
        public const string BusinessErrorLogCategoryName = "BusinessError";
        public const string EventLoggerLogCategoryName = "EventLogger";

        public static ILogger ConsoleInfoLogger(this ILoggerFactory factory)
        {
            return factory.CreateLogger(ConsoleInfoLogCategoryName);
        }

        public static ILogger ConsoleErrorLogger(this ILoggerFactory factory)
        {
            return factory.CreateLogger(ConsoleErrorLogCategoryName);
        }

        public static ILogger CommonInfoLogger(this ILoggerFactory factory)
        {
            return factory.CreateLogger(CommonInfoLogCategoryName);
        }

        public static ILogger FatalErrorLogger(this ILoggerFactory factory)
        {
            return factory.CreateLogger(FatalErrorLogCategoryName);
        }

        public static ILogger BusinessErrorLogger(this ILoggerFactory factory)
        {
            return factory.CreateLogger(BusinessErrorLogCategoryName);
        }

        public static ILogger EventLogger(this ILoggerFactory factory)
        {
            return factory.CreateLogger(EventLoggerLogCategoryName);
        }

    }
}