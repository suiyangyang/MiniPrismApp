using MinitPrismKit.Services.Configuration;
using Serilog;

namespace MiniPrismKit.MVVM
{
    public class ViewModelBase
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
