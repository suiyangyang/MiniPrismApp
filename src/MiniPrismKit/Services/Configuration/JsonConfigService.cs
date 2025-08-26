using Newtonsoft.Json;
using Serilog;
using System.IO;

namespace MiniPrismKit.Services.Configuration
{
    public class JsonConfigService : IConfigService, IDisposable
    {
        private readonly string _configFilePath;
        private Dictionary<string, object?> _config = new();
        private FileSystemWatcher? _watcher;
        private bool _disposed;

        public event EventHandler? ConfigChanged;

        public JsonConfigService(string configFilePath = "appsettings.json")
        {
            _configFilePath = configFilePath;
            Load();

            StartWatcher();
        }

        private void Load()
        {
            try
            {
                if (File.Exists(_configFilePath))
                {
                    var json = File.ReadAllText(_configFilePath);
                    _config = JsonConvert.DeserializeObject<Dictionary<string, object?>>(json)
                              ?? new Dictionary<string, object?>();
                }
                else
                {
                    _config = new Dictionary<string, object?>();
                    Save();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "加载配置文件失败：{File}", _configFilePath);
                _config = new Dictionary<string, object?>();
            }
        }

        public T GetValue<T>(string key, T defaultValue = default!)
        {
            try
            {
                if (_config.TryGetValue(key, out var value) && value != null)
                {
                    return (value is T typed) ? typed : JsonConvert.DeserializeObject<T>(value.ToString()!);
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "读取配置键 {Key} 失败，返回默认值", key);
            }
            return defaultValue;
        }

        public void SetValue<T>(string key, T value)
        {
            _config[key] = value;
        }

        public void Save()
        {
            try
            {
                var json = JsonConvert.SerializeObject(_config, Formatting.Indented);
                File.WriteAllText(_configFilePath, json);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "保存配置文件失败：{File}", _configFilePath);
            }
        }

        public void Reload()
        {
            Load();
            ConfigChanged?.Invoke(this, EventArgs.Empty);
        }

        private void StartWatcher()
        {
            try
            {
                _watcher = new FileSystemWatcher(Path.GetDirectoryName(_configFilePath) ?? ".", Path.GetFileName(_configFilePath))
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.CreationTime
                };

                _watcher.Changed += (s, e) =>
                {
                    // 避免文件写入时触发多次，需要加延迟
                    Thread.Sleep(200);
                    Reload();
                };

                _watcher.EnableRaisingEvents = true;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "配置文件监视器启动失败，将不会自动重载：{File}", _configFilePath);
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _watcher?.Dispose();
                _disposed = true;
            }
        }
    }
}
