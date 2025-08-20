using Serilog;
using Serilog.Events;
using System.IO;

namespace MiniPrismKit.Services.Logging
{
    public static class Logging
    {
        private static bool _initialized;

        /// <summary>
        /// 初始化 Serilog。默认每天一个文件，路径为 {AppData}\MiniPrismKit\logs\yyyy-MM-dd.log
        /// </summary>
        public static void Init(string appName = null, string logDirectory = null, LogEventLevel level = LogEventLevel.Information)
        {
            if (_initialized) return;
            try
            {
                appName = string.IsNullOrWhiteSpace(appName) ? AppDomain.CurrentDomain.FriendlyName : appName;
                logDirectory = string.IsNullOrWhiteSpace(logDirectory)
                    ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), appName, "logs")
                    : logDirectory;

                Directory.CreateDirectory(logDirectory);

                Log.Logger = new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .Enrich.WithProperty("ProcessId", Environment.ProcessId)
                    .Enrich.WithProperty("ThreadId", Environment.CurrentManagedThreadId)
                    .WriteTo.File("log-.txt", rollingInterval: RollingInterval.Day)
                    .CreateLogger();

                _initialized = true;
                Log.Information("Serilog initialized. Logs directory: {LogDirectory}", logDirectory);
            }
            catch (Exception ex)
            {
                // Fallback: write to temp
                var fallback = Path.Combine(Path.GetTempPath(), "MiniPrismKit.log");
                File.AppendAllText(fallback, DateTime.Now + " Init Serilog failed: " + ex + Environment.NewLine);
            }
        }

        public static void Shutdown()
        {
            try { Log.CloseAndFlush(); } catch { /* ignore */ }
            _initialized = false;
        }
    }
}
