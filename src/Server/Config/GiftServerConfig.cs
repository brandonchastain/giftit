using System;
using Microsoft.Extensions.Configuration;

namespace GiftServer.Config
{
    public class GiftServerConfig
    {
        public string ServerHostName { get; set; } = "https://localhost:7034/";
        public string DbPath { get; set; }
        public bool IsTestUserEnabled { get; set; }

        public static GiftServerConfig LoadFromAppSettings(IConfiguration configuration)
        {
            var config = new GiftServerConfig();
            configuration.GetSection(nameof(GiftServerConfig)).Bind(config);
            return config;
        }
    }
}