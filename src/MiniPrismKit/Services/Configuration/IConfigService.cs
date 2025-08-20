namespace MinitPrismKit.Services.Configuration
{
    public interface IConfigService
    {
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
