using System;
using System.Text;
using System.Threading.Tasks;
using Core.Networking;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Core.SDK
{
    public interface ISupabaseClient
    {
        UniTask<string> GetAsync(string endpoint, string query = null);
        UniTask<string> PostAsync(string endpoint, string jsonBody);
        UniTask<string> PatchAsync(string endpoint, string query, string jsonBody);
        UniTask<string> DeleteAsync(string endpoint, string query);
        UniTask<string> RpcAsync(string functionName, string jsonBody);
    }

    /// <summary>
    /// Supabase REST API client
    /// Handles all HTTP requests to Supabase
    /// </summary>
    public class SupabaseClient : ISupabaseClient
    {
        private readonly SupabaseConfig _config;

        public SupabaseClient(SupabaseConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));

            if (!_config.IsValid())
            {
                throw new InvalidOperationException("Invalid Supabase configuration");
            }
        }

        /// <summary>
        /// GET request
        /// </summary>
        public async UniTask<string> GetAsync(string endpoint, string query = null)
        {
            var url = $"{_config.RestApiUrl}/{endpoint}";
            if (!string.IsNullOrEmpty(query))
            {
                url += $"?{query}";
            }

            using (var request = UnityWebRequest.Get(url))
            {
                SetHeaders(request);

                await request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    throw new Exception($"GET request failed: {request.error}\nResponse: {request.downloadHandler.text}");
                }

                return request.downloadHandler.text;
            }
        }

        /// <summary>
        /// POST request
        /// </summary>
        public async UniTask<string> PostAsync(string endpoint, string jsonBody)
        {
            var url = $"{_config.RestApiUrl}/{endpoint}";
            var bodyBytes = Encoding.UTF8.GetBytes(jsonBody);

            using (var request = new UnityWebRequest(url, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyBytes);
                request.downloadHandler = new DownloadHandlerBuffer();

                SetHeaders(request);
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Prefer", "return=representation");

                await request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    throw new Exception($"POST request failed: {request.error}\nResponse: {request.downloadHandler.text}");
                }

                return request.downloadHandler.text;
            }
        }

        /// <summary>
        /// PATCH request
        /// </summary>
        public async UniTask<string> PatchAsync(string endpoint, string query, string jsonBody)
        {
            var url = $"{_config.RestApiUrl}/{endpoint}?{query}";
            var bodyBytes = Encoding.UTF8.GetBytes(jsonBody);

            using (var request = new UnityWebRequest(url, "PATCH"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyBytes);
                request.downloadHandler = new DownloadHandlerBuffer();

                SetHeaders(request);
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Prefer", "return=representation");

                await request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    throw new Exception($"PATCH request failed: {request.error}\nResponse: {request.downloadHandler.text}");
                }

                return request.downloadHandler.text;
            }
        }

        /// <summary>
        /// DELETE request
        /// </summary>
        public async UniTask<string> DeleteAsync(string endpoint, string query)
        {
            var url = $"{_config.RestApiUrl}/{endpoint}?{query}";

            using (var request = UnityWebRequest.Delete(url))
            {
                request.downloadHandler = new DownloadHandlerBuffer();

                SetHeaders(request);

                await request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    throw new Exception($"DELETE request failed: {request.error}\nResponse: {request.downloadHandler.text}");
                }

                return request.downloadHandler.text;
            }
        }

        /// <summary>
        /// Call RPC function
        /// </summary>
        public async UniTask<string> RpcAsync(string functionName, string jsonBody)
        {
            var url = $"{_config.RestApiUrl}/rpc/{functionName}";
            var bodyBytes = Encoding.UTF8.GetBytes(jsonBody);

            using (var request = new UnityWebRequest(url, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyBytes);
                request.downloadHandler = new DownloadHandlerBuffer();

                SetHeaders(request);
                request.SetRequestHeader("Content-Type", "application/json");

                await request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    throw new Exception($"RPC request failed: {request.error}\nResponse: {request.downloadHandler.text}");
                }

                return request.downloadHandler.text;
            }
        }

        /// <summary>
        /// Set common headers for all requests
        /// </summary>
        private void SetHeaders(UnityWebRequest request)
        {
            request.SetRequestHeader("apikey", _config.anonKey);
            request.SetRequestHeader("Authorization", $"Bearer {_config.anonKey}");
            request.timeout = _config.timeoutSeconds;
        }
    }
}
