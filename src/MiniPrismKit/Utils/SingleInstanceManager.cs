using System.IO;
using System.IO.Pipes;
using System.Reflection;
using System.Windows;

namespace MiniPrismKit.Utils
{
    public static class SingleInstanceManager
    {
        private static readonly string AppName = GetAppName();
        private static readonly string MutexName = $@"Global\{AppName}";
        private static readonly string PipeName = AppName;

        private static Mutex _mutex;

        /// <summary>
        /// 启动时调用，返回 false 表示已运行
        /// </summary>
        public static bool EnsureSingleInstance()
        {
            bool createdNew;
            _mutex = new Mutex(true, MutexName, out createdNew);

            if (createdNew)
            {
                // 第一次启动，启动监听
                StartPipeServer();
                return true;
            }
            else
            {
                // 已运行，通知主进程激活窗口
                SendActivateRequest();
                return false;
            }
        }

        private static string GetAppName()
        {
            // 方式1：用程序集名称（推荐）
            return Assembly.GetEntryAssembly()?.GetName().Name ?? "UnknownApp";

            // 方式2：用可执行文件名
            // return Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName);
        }


        /// <summary>
        /// 第二次启动时：发送激活请求给主实例
        /// </summary>
        private static void SendActivateRequest()
        {
            try
            {
                using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
                client.Connect(1000); // 1秒超时
                using var writer = new StreamWriter(client);
                writer.WriteLine("activate");
                writer.Flush();
            }
            catch
            {
                // 主进程未响应，忽略
            }
        }

        /// <summary>
        /// 第一次启动时：开启管道监听，接受激活请求
        /// </summary>
        private static void StartPipeServer()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        using var server = new NamedPipeServerStream(PipeName, PipeDirection.In);
                        server.WaitForConnection();
                        using var reader = new StreamReader(server);
                        string command = reader.ReadLine();

                        if (command == "activate")
                        {
                            Application.Current?.Dispatcher.Invoke(() =>
                            {
                                var win = Application.Current.MainWindow;
                                if (win != null)
                                {
                                    if (!win.IsVisible)
                                        win.Show(); // 恢复隐藏窗口
                                    if (win.WindowState == WindowState.Minimized)
                                        win.WindowState = WindowState.Normal;
                                    win.Activate();
                                    win.Topmost = true;  // 临时置顶确保激活
                                    win.Topmost = false;
                                    win.Focus();
                                }
                            });
                        }
                    }
                    catch
                    {
                        // 忽略单次错误
                    }
                }
            });
        }
    }
}
