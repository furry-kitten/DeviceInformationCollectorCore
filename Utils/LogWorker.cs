using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace UsbDeviceInformationCollectorCore.Utils
{
    internal class LogWorker
    {
        public static string AppDataFolder =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "NSysGroup");

        public static string LogDir = Path.Combine(AppDataFolder, "logs");

        public static void ConfigNLog()
        {
            var config = new LoggingConfiguration();
            var layout =
                @"${longdate:universalTime=true} [${level}] ${logger}: ${message} ${exception:format=tostring,data:innerFormat=tostring:maxInnerExceptionLevel=2:exceptionDataSeparator=\r\n}";

            var usbDeviceCollectorLog = new FileTarget("UsbDeviceInformationCollectorCore.log")
            {
                FileName = $"{LogDir}/UsbDeviceInformationCollectorCore.log",
                Layout = layout,
                EnableArchiveFileCompression = true,
                ArchiveFileName = LogDir + "/{#}.zip",
                ArchiveNumbering = ArchiveNumberingMode.DateAndSequence,
                ArchiveDateFormat = "yyyyMMdd",
                ArchiveOldFileOnStartup = true,
                //EnableFileDelete = true,
                ArchiveOldFileOnStartupAboveSize = 20 * 1024 * 1024
            };

            config.AddRule(LogLevel.Trace, LogLevel.Off, usbDeviceCollectorLog);
            //var androidLog = new FileTarget("androidLog")
            //{
            //    FileName = LogDir + "/android.log",
            //    Layout = @"${longdate:universalTime=true}: ${message}",
            //};
            //config.AddRule(LogLevel.Warn, LogLevel.Warn, androidLog, "*", true);
#if false
        var logfile = new FileTarget("logfile") { FileName =
 "${basedir:fixTempDir=true}/logs/DesktopDiagnostic.${longdate:cached=true}.log", Layout = layout };
        var debug = new OutputDebugStringTarget() { Layout = layout };
        var efFile = new FileTarget("eflog") { FileName = "${basedir:fixTempDir=true}/logs/EntityFramework.log", Layout
 = layout };

        //config.AddRule(LogLevel.Debug, LogLevel.Fatal, efFile, "Microsoft.EntityFrameworkCore*", true);
        config.AddRule(LogLevel.Debug, LogLevel.Fatal, debug);
        config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);

        var ianFile = new FileTarget("Ian logs")
            { FileName = "${basedir:fixTempDir=true}/logs/Ian.log", Layout =
 @"${longdate:universalTime=true}: ${message}" };
        config.AddRule(LogLevel.Info, LogLevel.Info, ianFile);
#else
            //var ianFile = new FileTarget("Ian logs")
            //{
            //    FileName = LogDir + "/Ian.log",
            //    Layout = @"${longdate:universalTime=true}: ${message}",
            //    EnableArchiveFileCompression = true,
            //    //ArchiveFileName = LogDir + "/Ian{###}.zip",
            //    //ArchiveNumbering = ArchiveNumberingMode.Sequence,
            //    //ArchiveOldFileOnStartup = true,
            //    //ArchiveEvery = FileArchivePeriod.Day,
            //};
            //config.AddRule(LogLevel.Info, LogLevel.Info, ianFile, "*", true);

            //var fileTarget = new FileTarget("logfile")
            //{
            //    FileName = LogDir + "/DesktopDiagnostic.log",
            //    Layout = layout,
            //    EnableArchiveFileCompression = true,
            //    ArchiveFileName = LogDir + "/{#}.zip",
            //    ArchiveNumbering = ArchiveNumberingMode.DateAndSequence,
            //    //ArchiveDateFormat = "yyyyMMdd",
            //    ArchiveOldFileOnStartup = true,
            //    ArchiveOldFileOnStartupAboveSize = 20 * 1024 * 1024,
            //    //EnableFileDelete = true
            //};
            //config.AddRule(LogLevel.Debug, LogLevel.Fatal, fileTarget);

            //var mailTarget = new MailTarget("mail")
            //{
            //    SmtpServer = "smtp.gmail.com",
            //    SmtpPort = 587,
            //    EnableSsl = true,
            //    SmtpAuthentication = SmtpAuthenticationMode.Basic,
            //    SmtpUserName = "techbox@nsysgroup.com",
            //    SmtpPassword = "af09daRw9",
            //    DeliveryMethod = SmtpDeliveryMethod.Network,
            //    From = "techbox@nsysgroup.com",
            //    To = "lada@nsysgroup.com",
            //    Subject = "Logs",
            //    Body = "${longdate:universalTime=true}: ${message}${newline}${newline}",
            //    Timeout = 10000 * 10,
            //};
            //var bufferingWrapper = new BufferingTargetWrapper(mailTarget);
            //var asyncTarget = new AsyncTargetWrapper(bufferingWrapper);
            //config.AddRule(LogLevel.Info, LogLevel.Fatal, asyncTarget); //todo ?
#endif
            LogManager.Configuration = config;
        }

        public static void ZipLog()
        {
            var zipLogs = Directory.GetFiles(LogDir, "*.log");
            if (!zipLogs.Any())
            {
                return;
            }

            var zipName = $"{AppDataFolder}/{DateTime.UtcNow:s}.zip";
            ZipFile.CreateFromDirectory(LogDir, zipName);
        }
    }
}