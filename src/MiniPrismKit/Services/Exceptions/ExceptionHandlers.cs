using Serilog;
using System.Windows;

namespace MiniPrismKit.Services.Exceptions
{
    public static class ExceptionHandlers
    {
        private static bool _initialized;
        private static IExceptionHandler _handler = new DefaultExceptionHandler();

        public static void Register(IExceptionHandler? handler = null)
        {
            if (_initialized) return;
            _initialized = true;

            if (handler != null)
                _handler = handler;

            Application.Current.DispatcherUnhandledException += (s, e) =>
            {
                _handler.Handle(e.Exception);
                e.Handled = true;
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                _handler.Handle(e.Exception);
                e.SetObserved();
            };

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                if (e.ExceptionObject is Exception ex)
                    _handler.Handle(ex);
                else
                    Log.Error("未处理的非UI线程异常: {ExceptionObject}", e.ExceptionObject);
            };
        }
    }
}
