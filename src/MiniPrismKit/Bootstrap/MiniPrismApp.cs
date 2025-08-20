using MiniPrismKit.Services.Configuration;
using MiniPrismKit.Services.Exceptions;
using MiniPrismKit.Services.Logging;
using MinitPrismKit.Services.Configuration;
using Serilog;
using System.Windows;

namespace MiniPrismKit.Bootstrap
{
    /// <summary>
    /// 可在应用中继承此基类，快速获得：Prism(DryIoc) + Serilog(日志按天) + 全局异常处理 + 轻量对话框。
    /// </summary>
    public abstract class MiniPrismApp : PrismApplication
    {
        private readonly string _appName;
        private readonly string _logDirectory;

        protected MiniPrismApp(string appName = null, string logDirectory = null)
        {
            _appName = appName;
            _logDirectory = logDirectory;
        }

        protected override IContainerExtension CreateContainerExtension() => new DryIocContainerExtension(new Container(rules => rules.WithTrackingDisposableTransients()));

        protected override void OnStartup(StartupEventArgs e)
        {
            // 初始化日志
            Logging.Init(_appName, _logDirectory);

            // 注册全局异常处理
            ExceptionHandlers.Register(new DefaultExceptionHandler());

            Log.Information("Application starting with {App}", _appName ?? AppDomain.CurrentDomain.FriendlyName);
            base.OnStartup(e);
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // Prism 对话框：注册默认窗口
            containerRegistry.RegisterDialogWindow<Dialogs.DialogWindow>();
            containerRegistry.RegisterSingleton<IConfigService, JsonConfigService>();

            // 用户可在此注册自定义服务
            ConfigureServices(containerRegistry);
        }

        /// <summary>给派生类添加自定义服务的扩展点。</summary>
        protected virtual void ConfigureServices(IContainerRegistry containerRegistry) { }

        protected override Window CreateShell()
        {
            // 交由派生类创建主窗口
            return CreateShellInternal();
        }

        protected override void ConfigureViewModelLocator()
        {
            base.ConfigureViewModelLocator();

            // 默认规则：View -> ViewModel
            ViewModelLocationProvider.SetDefaultViewTypeToViewModelTypeResolver(viewType =>
            {
                var viewName = viewType.FullName!;
                var viewAssemblyName = viewType.Assembly.FullName!;

                // 把命名空间里的 "Views" 替换成 "ViewModels"
                var viewModelName = viewName.Replace(".Views.", ".ViewModels.") + "ViewModel";

                return Type.GetType($"{viewModelName}, {viewAssemblyName}");
            });
        }

        /// <summary>派生类必须返回主窗口实例。</summary>
        protected abstract Window CreateShellInternal();

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information("Application exiting with code {Code}", e.ApplicationExitCode);
            Logging.Shutdown();
            base.OnExit(e);
        }
    }
}
