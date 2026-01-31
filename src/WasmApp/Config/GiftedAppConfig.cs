
namespace RssApp.Config
{
    public class GiftedAppConfig
    {
        public GiftedAppConfig()
        {
        }

        public string ApiBaseUrl { get; set; } = "https://localhost:7034/";
        public string AuthApiBaseUrl { get; set; } = "https://localhost:7085/";
        public bool EnableTestAuth { get; set; } = false;
        public string TestAuthUsername { get; set; } = "testuser";

        public static GiftedAppConfig LoadFromAppSettings(IConfiguration configuration)
        {
            var config = new GiftedAppConfig();
            configuration.GetSection(nameof(GiftedAppConfig)).Bind(config);
            return config;
        }
    }
}