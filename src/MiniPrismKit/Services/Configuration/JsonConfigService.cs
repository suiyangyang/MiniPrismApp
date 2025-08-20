using MinitPrismKit.Services.Configuration;
using Newtonsoft.Json.Linq;
using System.IO;

namespace MiniPrismKit.Services.Configuration
{
    public class JsonConfigService : IConfigService
    {
        private readonly string _filePath;
        private JObject _config;

        public JsonConfigService(string fileName = "appsettings.json")
        {
            _filePath = Path.Combine(AppContext.BaseDirectory, fileName);
            if (File.Exists(_filePath))
            {
                try
                {
                    var json = File.ReadAllText(_filePath);
                    _config = JObject.Parse(json);
                }
                catch (Exception ex)
                {
                    Serilog.Log.Warning(ex, "配置文件读取失败: {File}", _filePath);
                    _config = new JObject();
                }
            }
            else
            {
                _config = new JObject();
            }
        }

        public T? GetValue<T>(string key, T? defaultValue = default)
        {
            try
            {
                var token = _config.SelectToken(key);
                if (token == null) return defaultValue;
                return token.ToObject<T>();
            }
            catch
            {
                return defaultValue;
            }
        }

        public void SetValue<T>(string key, T value)
        {
            if (string.IsNullOrWhiteSpace(key)) return;

            try
            {
                // 支持嵌套路径 "Logging.Level"
                var segments = key.Split('.');
                JObject current = _config;

                for (int i = 0; i < segments.Length - 1; i++)
                {
                    var seg = segments[i];
                    if (current[seg] == null)
                        current[seg] = new JObject();
                    current = (JObject)current[seg]!;
                }

                current[segments[^1]] = JToken.FromObject(value);
            }
            catch (Exception ex)
            {
                Serilog.Log.Warning(ex, "SetValue 出错，Key={Key}", key);
            }
        }

        public void Save()
        {
            try
            {
                File.WriteAllText(_filePath, _config.ToString());
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "保存配置文件失败: {File}", _filePath);
            }
        }
    }
}
