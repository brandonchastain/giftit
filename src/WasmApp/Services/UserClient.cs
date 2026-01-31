using System;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Authorization;
using RssApp.Config;
using GiftServer.Contracts;

namespace WasmApp.Services
{
    public class UserClient
    {
        private readonly HttpClient _httpClient;
        private readonly GiftedAppConfig _config;
        private readonly ILogger<UserClient> _logger;
        private bool _disposed;

        public UserClient(GiftedAppConfig config, ILogger<UserClient> logger, IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _config = config;
            _logger = logger;
        }

        public async Task<User> GetUserAsync(string email)
        {
            return await _httpClient.GetFromJsonAsync<User>($"{_config.ApiBaseUrl}api/user?email={Uri.EscapeDataString(email)}");
        }

        public async Task AddNewUserAsync(User user)
        {
            await _httpClient.PostAsJsonAsync($"{_config.ApiBaseUrl}api/user", user);
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
