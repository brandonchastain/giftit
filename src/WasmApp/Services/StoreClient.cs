using System;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Authorization;
using RssApp.Config;
using GiftServer.Contracts;

namespace WasmApp.Services
{
    public class StoreClient
    {
        private readonly HttpClient _httpClient;
        private readonly RssWasmConfig _config;
        private readonly ILogger<StoreClient> _logger;
        private bool _disposed;

        public StoreClient(RssWasmConfig config, ILogger<StoreClient> logger, IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _config = config;
            _logger = logger;
        }

        public async Task<Store[]> GetStoresForPersonAsync(int personId)
        {
            return await _httpClient.GetFromJsonAsync<Store[]>($"{_config.ApiBaseUrl}api/store/person/{personId}");
        }

        public async Task AddStoreAsync(Store store)
        {
            await _httpClient.PostAsJsonAsync($"{_config.ApiBaseUrl}api/store", store);
        }

        public async Task DeleteStoreAsync(int id)
        {
            await _httpClient.DeleteAsync($"{_config.ApiBaseUrl}api/store/{id}");
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _httpClient.Dispose();
                _disposed = true;
            }
        }
    }
}
