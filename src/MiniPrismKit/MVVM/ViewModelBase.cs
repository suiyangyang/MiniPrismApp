using MiniPrismKit.Services.Configuration;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Serilog;
using System.Windows.Threading;

namespace MiniPrismKit.MVVM
{
    public class ViewModelBase : BindableBase
    {
        protected readonly IEventAggregator EventAggregator;
        protected readonly IConfigService Config;
        protected readonly ILogger Logger;
        protected readonly IContainerProvider Container; // 注入容器

        public ViewModelBase(
            IEventAggregator eventAggregator,
            IConfigService config,
            IContainerProvider container) // 注入
        {
            EventAggregator = eventAggregator;
            Config = config;
            Container = container;
            Logger = Serilog.Log.Logger;
        }

        public Dispatcher Dispatcher { get; set; }

        // 示例：发布事件
        protected void PublishEvent<TEvent, TPayload>(TPayload payload)
            where TEvent : PubSubEvent<TPayload>, new()
        {
            EventAggregator?.GetEvent<TEvent>()?.Publish(payload);
        }

        // 示例：通过容器解析对象
        protected T Resolve<T>() => Container.Resolve<T>();
    }
}
