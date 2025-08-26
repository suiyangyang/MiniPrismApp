namespace MiniPrismKit.Services.Configuration
{
    public interface IConfigService
    {/// <summary>
     /// 当配置文件被外部修改时触发
     /// </summary>
        event EventHandler? ConfigChanged;

        /// <summary>
        /// 从配置文件获取值
        /// </summary>
        T? GetValue<T>(string key, T? defaultValue = default);

        /// <summary>
        /// 设置配置值（修改或新增）
        /// </summary>
        void SetValue<T>(string key, T value);

        /// <summary>
        /// 保存当前配置到文件
        /// </summary>
        void Save();
    }
}
