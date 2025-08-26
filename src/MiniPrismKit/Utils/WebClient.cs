using Newtonsoft.Json;
using Serilog;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;

namespace MiniPrismKit.Utils
{
    public static class WebClient
    {
        private static HttpClient _httpClient;
        private static string? _bearerToken;

        static WebClient()
        {
            var handler = new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(5),
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
                MaxConnectionsPerServer = 10
            };

            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        public static void ConfigureHandler(SocketsHttpHandler handler, TimeSpan? timeout = null)
        {
            if (_httpClient != null)
                throw new InvalidOperationException("WebClient 已初始化，无法再配置连接池。请在第一次使用前调用 ConfigureHandler。");

            _httpClient = new HttpClient(handler)
            {
                Timeout = timeout ?? TimeSpan.FromSeconds(30)
            };
        }

        public static void SetBearerToken(string token) => _bearerToken = token;
        public static void ClearToken() => _bearerToken = null;

        private static void ApplyHeaders(HttpRequestMessage request)
        {
            if (!string.IsNullOrWhiteSpace(_bearerToken))
            {
                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _bearerToken);
                Log.Debug("添加 Authorization Bearer Token");
            }
        }

        #region GET / POST JSON

        public static async Task<T?> GetAsync<T>(string url)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                ApplyHeaders(request);

                Log.Information("GET 请求开始: {Method} {Url} Headers={Headers}",
                    request.Method, request.RequestUri, request.Headers);

                using var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                Log.Information("GET 请求完成: {Url} StatusCode={StatusCode} 耗时={Elapsed}ms ContentLength={Length}",
                    url, response.StatusCode, sw.ElapsedMilliseconds, content.Length);

                response.EnsureSuccessStatusCode();
                return JsonConvert.DeserializeObject<T>(content);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "GET 请求失败: {Url} 耗时={Elapsed}ms", url, sw.ElapsedMilliseconds);
                throw;
            }
        }

        public static async Task<T?> PostJsonAsync<T>(string url, object data)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                var json = JsonConvert.SerializeObject(data);
                using var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
                ApplyHeaders(request);

                Log.Information("POST 请求开始: {Method} {Url} Headers={Headers} Payload={Payload}",
                    request.Method, request.RequestUri, request.Headers, json);

                using var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                Log.Information("POST 请求完成: {Url} StatusCode={StatusCode} 耗时={Elapsed}ms ResponseLength={Length}",
                    url, response.StatusCode, sw.ElapsedMilliseconds, content.Length);

                response.EnsureSuccessStatusCode();
                return JsonConvert.DeserializeObject<T>(content);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "POST 请求失败: {Url} Payload={Payload} 耗时={Elapsed}ms", url, JsonConvert.SerializeObject(data), sw.ElapsedMilliseconds);
                throw;
            }
        }

        #endregion

        #region DELETE JSON

        public static async Task<T?> DeleteAsync<T>(string url)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Delete, url);
                ApplyHeaders(request);

                Log.Information("DELETE 请求开始: {Url} Headers={Headers}",
                    request.RequestUri, request.Headers);

                using var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                Log.Information("DELETE 请求完成: {Url} StatusCode={StatusCode} 耗时={Elapsed}ms ContentLength={Length}",
                    url, response.StatusCode, sw.ElapsedMilliseconds, content.Length);

                response.EnsureSuccessStatusCode();
                return JsonConvert.DeserializeObject<T>(content);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "DELETE 请求失败: {Url} 耗时={Elapsed}ms", url, sw.ElapsedMilliseconds);
                throw;
            }
        }

        #endregion

        #region 文件上传

        /// <summary>
        /// 上传单个文件
        /// </summary>
        public static async Task<T?> UploadFileAsync<T>(string url, string filePath, IProgress<double>? progress = null)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                using var content = new MultipartFormDataContent();
                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                //using var streamContent = new StreamContent(fs);
                using var streamContent = new ProgressableStreamContent(fs, 4096, progress);
                //streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                content.Add(streamContent, "file", Path.GetFileName(filePath));

                using var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = content
                };
                ApplyHeaders(request);

                Log.Information("上传文件开始: {File} -> {Url}", filePath, url);

                using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                var respContent = await response.Content.ReadAsStringAsync();

                Log.Information("上传文件完成: {File} -> {Url} StatusCode={StatusCode} 耗时={Elapsed}ms ResponseLength={Length}",
                    filePath, url, response.StatusCode, sw.ElapsedMilliseconds, respContent.Length);

                response.EnsureSuccessStatusCode();
                return JsonConvert.DeserializeObject<T>(respContent);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "上传文件失败: {File} -> {Url} 耗时={Elapsed}ms", filePath, url, sw.ElapsedMilliseconds);
                throw;
            }
        }

        /// <summary>
        /// 上传多个文件
        /// </summary>
        public static async Task<List<T?>> UploadFilesAsync<T>(string url, IEnumerable<string> filePaths, IProgress<double>? progress = null)
        {
            var results = new List<T?>();
            int total = filePaths.Count();
            int index = 0;

            foreach (var filePath in filePaths)
            {
                index++;
                var subProgress = new Progress<double>(p =>
                {
                    progress?.Report(((index - 1) + p) / total * 100.0);
                });

                var result = await UploadFileAsync<T>(url, filePath, subProgress);
                results.Add(result);
            }

            progress?.Report(100.0);
            return results;
        }

        #endregion

        #region 辅助类：支持进度的 StreamContent

        private class ProgressableStreamContent : HttpContent
        {
            private const int defaultBufferSize = 4096;
            private readonly Stream _content;
            private readonly int _bufferSize;
            private readonly IProgress<double>? _progress;

            public ProgressableStreamContent(Stream content, int bufferSize, IProgress<double>? progress)
            {
                _content = content;
                _bufferSize = bufferSize;
                _progress = progress;
            }

            protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
            {
                var buffer = new byte[_bufferSize];
                long size = _content.Length;
                long uploaded = 0;

                int read;
                while ((read = await _content.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await stream.WriteAsync(buffer, 0, read);
                    uploaded += read;
                    _progress?.Report((double)uploaded / size * 100.0);
                }
            }

            protected override bool TryComputeLength(out long length)
            {
                length = _content.Length;
                return true;
            }
        }

        #endregion
    }
}
