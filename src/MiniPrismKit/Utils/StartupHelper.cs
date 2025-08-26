using Microsoft.Win32;
using System.Reflection;

namespace MiniPrismKit.Utils
{
    public static class StartupHelper
    {
        public static void SetStartup(bool enable)
        {
            Serilog.Log.Information($"设置开机启动:{enable}");
            try
            {
                // 用程序集名称作为应用名（更通用）
                string appName = Assembly.GetEntryAssembly()?.GetName().Name ?? "UnknownApp";

                // 或者用文件名（更贴近实际 exe 名字）
                // string appName = Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly()?.Location);

                string exePath = Assembly.GetEntryAssembly()?.Location ?? "";

                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    if (enable)
                    {
                        key.SetValue(appName, exePath);
                    }
                    else
                    {
                        key.DeleteValue(appName, false);
                    }
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Warning(ex, "设置开机启动失败");
            }
        }
    }
}
