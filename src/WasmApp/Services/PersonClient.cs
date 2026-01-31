using System;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Authorization;
using RssApp.Config;
using GiftServer.Contracts;

namespace WasmApp.Services
{
    public class PersonClient
    {
        private readonly HttpClient _httpClient;
        private readonly RssWasmConfig _config;
        private readonly ILogger<PersonClient> _logger;
        private bool _disposed;

        public PersonClient(RssWasmConfig config, ILogger<PersonClient> logger, IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _config = config;
            _logger = logger;
        }

        public async Task<Person[]> GetMyPeopleAsync(int userId)
        {
            return await _httpClient.GetFromJsonAsync<Person[]>($"{_config.ApiBaseUrl}api/person/my?id={userId}");
        }

        public async Task<Person> GetPersonAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<Person>($"{_config.ApiBaseUrl}api/person?id={id}");
        }

        public async Task AddNewPersonAsync(Person person)
        {
            await _httpClient.PostAsJsonAsync($"{_config.ApiBaseUrl}api/person", person);
        }

        public async Task DeletePersonAsync(int id)
        {
            await _httpClient.DeleteAsync($"{_config.ApiBaseUrl}api/person?id={id}");
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
