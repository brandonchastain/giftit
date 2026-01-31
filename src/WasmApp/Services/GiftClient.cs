using System;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Authorization;
using RssApp.Config;
using GiftServer.Contracts;

namespace WasmApp.Services
{
    public class GiftClient
    {
        private readonly HttpClient _httpClient;
        private readonly RssWasmConfig _config;
        private readonly ILogger<GiftClient> _logger;
        private bool _disposed;

        public GiftClient(RssWasmConfig config, ILogger<GiftClient> logger, IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _config = config;
            _logger = logger;
        }

        public async Task<Gift[]> GetGiftIdeasForPersonAsync(int personId)
        {
            return await _httpClient.GetFromJsonAsync<Gift[]>($"{_config.ApiBaseUrl}api/gift/person/{personId}");
        }

        public async Task<Gift> GetGiftAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<Gift>($"{_config.ApiBaseUrl}api/gift/{id}");
        }

        public async Task AddNewGiftAsync(Gift gift)
        {
            await _httpClient.PostAsJsonAsync($"{_config.ApiBaseUrl}api/gift", gift);
        }

        public async Task MarkAsPurchasedAsync(int id)
        {
            await _httpClient.PutAsync($"{_config.ApiBaseUrl}api/gift/{id}/purchased", null);
        }

        public async Task DeleteGiftAsync(int id)
        {
            await _httpClient.DeleteAsync($"{_config.ApiBaseUrl}api/gift/{id}");
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
