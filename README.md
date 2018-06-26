# DotNet.NLogger.NetCore

**DotNet.NLogger.NetCore** is an adapter between [NLog](https://github.com/NLog/NLog) and [Microsoft.Extensions.Logging](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-2.1&tabs=aspnetcore2x).

It allows to simplify using NLog by utilizing [ILoggerFactory](https://github.com/aspnet/Logging) and [ILogger](https://github.com/aspnet/Logging) interfaces in an application.

[NLog](https://github.com/NLog/NLog) is a flexible and free logging platform for various .NET platforms, including .NET standard. NLog makes it easy to write to several targets. (database, file, console) and change the logging configuration on-the-fly.

## Nuget.org

- Nuget package [DotNet.NLogger.NetCore](https://www.nuget.org/packages/DotNet.NLog.NetCore/)
- Nuget package [DotNet.Memory.Logger](https://www.nuget.org/packages/DotNet.Memory.Logger/)


## Adding DotNet.NLogger.NetCore


You have to define two configurations:

 - Create the  [NLog configuration](https://github.com/NLog/NLog/wiki/Configuration-file) xml for Example:
 ``` xml
 <?xml version="1.0" encoding="utf-8" ?>
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
      xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
      autoReload="true"
      internalLogLevel="Warn"
      internalLogFile="c:\temp\internal-nlog.txt">

  <!-- https://github.com/NLog/NLog/wiki/Configuration-file  -->
  <targets>
    
    <!-- Null loggers -->
    
    <target xsi:type="Null" name="NullLog" />
    <target xsi:type="Null" name="SystemLog" />
    
    <!-- Console loggers -->
    <target xsi:type="Console" name="ConsoleInfoLog" />
    <target xsi:type="Console" name="ConsoleErrorLog" error="true" />


    <!-- File loggers -->

    <target xsi:type="File" name="CommonInfoLogFile"
            fileName="\Logs\RestWebApplication\Info\RestWebApp_CommonInfo-P_${processid}-${shortdate:universalTime=true}.log"
            layout="${longdate:universalTime=true}|${event-properties:item=EventId.Id}|${logger}|${uppercase:${level}}|${message}| ${exception}" />
    
    <target xsi:type="File" name="BusinessErrorLogFile"
            fileName="\Logs\RestWebApplication\BusinessError\RestWebApp_BusinessError-P_${processid}-${shortdate:universalTime=true}.log"
            layout="${longdate:universalTime=true}|${event-properties:item=EventId.Id}|${logger}|${uppercase:${level}}|${message}| ${exception}" />

    <target xsi:type="File" name="FatalErrorLogFile"
            fileName="\Logs\RestWebApplication\Error\RestWebApp_FatalError-P_${processid}-${shortdate:universalTime=true}.log"
            layout="${longdate:universalTime=true}|${event-properties:item=EventId.Id}|${logger}|${uppercase:${level}}|${message}| ${exception:innerFormat=Message,Method,StackTrace:maxInnerExceptionLevel=1:format=Message,Method,StackTrace}" />
  
  </targets>

  <rules>
    <!-- Loggers application section -->

    <logger name="FatalError" writeTo="FatalErrorLogFile,CommonInfoLogFile" minlevel="Error"  final="true" enabled="true" />
    <logger name="BusinessError" writeTo="BusinessErrorLogFile,CommonInfoLogFile" minlevel="Error"  final="true" enabled="true" />
 
    <logger name="CommonInfo" writeTo="CommonInfoLogFile"               minlevel="Info"   final="true" enabled="true" />
    
    <logger name="ConsoleError" writeTo="ConsoleErrorLog,FatalErrorLogFile,CommonInfoLogFile" minlevel="Error"  final="true" enabled="true" />
    <logger name="ConsoleInfo" writeTo="ConsoleInfoLog,CommonInfoLogFile"  minlevel="Info"   final="true" enabled="true" />

    <!-- Other log info -->
    <logger name="Microsoft*"      minlevel="Trace" writeTo="SystemLog"      final="true" enabled="false" />
    <!-- discard all not consumed -->
    <logger name="*" minlevel="Trace" writeTo="NullLog" />
  </rules>

</nlog>
 ```
   
- Create **DotNet.NLogger.NetCore** configuration section in "appsettings.json". The **NLogLoggerSettings** section defines the Category Name "filter" and Category Name "mapper". 

 The **Category Name "filter"** is used to filter in accepted category names. It is expected that the category name is exact match to **<logger name="...."**  in the NLog configuration.

 The **Category Name "mapper"** is used to filter in category names and map them onto new name that expected to be match to **<logger name="..."** in the NLog configuration.

```
{
"NLogLoggerSettings": {
    "IncludeScopes": true,
    "AcceptedCategoryNames": [ /* Filter of category name */
      "ConsoleInfo",   /* category name accepted as a "NLog logger name" */
      "CommonInfo",    /* category name accepted as a "NLog logger name" */
      "ConsoleError",  /* category name accepted as a "NLog logger name" */
      "FatalError",    /* category name accepted as a "NLog logger name" */
      "BusinessError", /* category name accepted as a "NLog logger name" */
      "*Error*",       /* category name contains "Error" accepted as a "NLog logger name" */
      "*Info",         /* category name end with "Info" accepted as a "NLog logger name" */
      "Com*",          /* category name start with "Com" accepted as a "NLog logger name" */
      "*"              /* any category name  will be accepted accepted as a "NLog logger name" */
    ],

    /* Map category name "ABC" to "NLog logger name" = "ConsoleError" */
    "AcceptedAliasesCategoryNames:ABD": "ConsoleError"  
    
    /* Map category name end with "*Hosted" to "NLog logger name" = "ConsoleError" */
    "AcceptedAliasesCategoryNames:*Hosted": "ConsoleError"  

    /* Map category name start with "Microsoft.AspNetCore*" to "NLog logger name" = "ConsoleError" */
    "AcceptedAliasesCategoryNames:Microsoft.AspNetCore*": "ConsoleError" 

    /* Map category name contains "*AspNetCore*" to "NLog logger name" = "ConsoleError"*/
    "AcceptedAliasesCategoryNames:*AspNetCore*": "ConsoleError"

    /* Map any category  to "NLog logger name" = "ConsoleError" */
    "AcceptedAliasesCategoryNames:*": "ConsoleError"

  }
}
```

After defining the configurations, add in the Web Host Builder configuring of Microsoft.Extensions.Logging.LoggerFactory the following initialization code:

``` C#

     .ConfigureLogging((hostingContext, logging) =>
       {

        // ** Add DotNet.NLogger.NetCore

        string logPath = Path.Combine(hostingContext.HostingEnvironment.ContentRootPath, $"nlog.{hostingContext.HostingEnvironment.EnvironmentName}.config");
        if (!File.Exists(logPath))
        {
            throw new MissingMemberException($"Missing NLog configuration file '{logPath}'");
        }
        var nLoggingConfiguration = new XmlLoggingConfiguration(logPath);

        var logJsonCgf = hostingContext.Configuration.GetSection(nameof(NLogLoggerSettings));
        if (!logJsonCgf.Exists())
        {
            throw new MissingMemberException($"Missing configuration section '{nameof(NLogLoggerSettings)}'");
        }

        logging.AddNLogLogger(logJsonCgf, nLoggingConfiguration);
      }
```

## Example project

See sample project [Using Nlog in .Net Core  Rest Web Application](https://github.com/Wallsmedia/DotNet.Logger/tree/master/samples/RestWebApplication)

## Microsoft.Extensions.Logging - Configuration
If you decided to use additional filtering from **Microsoft.Extensions.Logging** over the filtering that provided  with **DotNet.NLogger.NetCore** by adding configuration:

``` C#
 logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
```

or it will be added  by default in .Net Core > 2.0. see [WebHost.cs](https://github.com/aspnet/MetaPackages/blob/dev/src/Microsoft.AspNetCore/WebHost.cs))

So, It is recommend to read "how to configure the Logging'" section from ASP.NET CORE web guide [Log filtering](https://docs.microsoft.com/en-gb/aspnet/core/fundamentals/logging/?view=aspnetcore-2.1&tabs=aspnetcore2x#log-filtering). 


# DotNet.Memory.Logger
.NET Memory Logger is a simple extension to log into memory by using [ConcurrentQueue\<T\>](https://docs.microsoft.com/en-gb/dotnet/api/system.collections.concurrent.concurrentqueue-1) collections


## Adding DotNet.Memory.Logger
T.B.D - ASAP



