using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RssApp.Config;
using WasmApp;
using WasmApp.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Add configuration from appsettings.json
builder.Configuration.AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile("appsettings.Development.json", optional: true);

// Configure GiftedAppConfig as a singleton with values from configuration
var config = GiftedAppConfig.LoadFromAppSettings(builder.Configuration);
builder.Services
    .AddSingleton<GiftedAppConfig>(_ => config);

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, MyAuthenticationStateProvider>();

// Add the authentication header handler
builder.Services.AddTransient<AuthenticationHeaderHandler>()
.AddSingleton<PersonClient>()
.AddSingleton<UserClient>()
.AddSingleton<GiftClient>()
.AddSingleton<StoreClient>();

// Configure HttpClient with authentication handler for API calls
builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri(config.ApiBaseUrl);
})
.AddHttpMessageHandler<AuthenticationHeaderHandler>();

var app = builder.Build();
await app.RunAsync();
