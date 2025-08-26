namespace MiniPrismKit.MVVM
{
    public interface IViewLoadedAndUnloadedAware
    {
        bool IsLoaded { get; set; }

        void OnLoaded();

        void OnUnloaded();

        void OnInitialize();
    }

    public interface IViewLoadedAndUnloadedAware<in TView>
    {
        void OnLoaded(TView view);

        void OnUnloaded(TView view);

        void OnInitialize(TView view);
    }

    /// <summary>
    /// 解决控件作为TabItem时切换Tab选中状态时触发Loaded和Unloaded事件，
    /// 导致无法准确注册和解注册事件的问题
    /// 该Aware用于TabItem所属的ViewModel
    /// </summary>
    public interface ITabItemLoadedAndUnloadedAware
    {
        /// <summary>
        /// 是否第一次加载
        /// </summary>
        bool IsFirstLoaded { get; set; }

        /// <summary>
        /// 所属TabControl Loaded事件触发时调用
        /// </summary>
        void OnTabControlLoaded();

        /// <summary>
        /// 所属TabControl Unloaded事件触发时调用
        /// </summary>
        void OnTabControlUnloaded();

        /// <summary>
        /// TabItem初始化完成时触发
        /// </summary>
        void OnInitialize();
    }
}
