using GiftServer.Components;
using GiftServer;
using IniParser;
using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Hangfire;
using Hangfire.Storage.SQLite;
using GiftServer.Notification;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var parser = new FileIniDataParser();
var config = parser.ReadFile("appconfig.ini");
var dbUrl = config["Turso"]["dbUrl"];
var authToken = config["Turso"]["authToken"];

var sendgridKey = Environment.GetEnvironmentVariable("SENDGRIDAPIKEY") ?? string.Empty;
GlobalConfiguration.Configuration.UseSQLiteStorage();

builder.Services.AddSingleton<INotifier, EmailNotifier>((_) => new EmailNotifier(sendgridKey));
builder.Services.AddSingleton<PersonRepository>();
builder.Services.AddSingleton<GiftRepository>();
builder.Services.AddSingleton<UserRepository>();
builder.Services.AddSingleton((_) => new TursoClient(dbUrl, authToken));
builder.Services.AddHangfire(configuration => configuration
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSQLiteStorage());
builder.Services.AddHangfireServer();
builder.Services
    .AddAuth0WebAppAuthentication(options => {
      options.Domain = builder.Configuration["Auth0:Domain"];
      options.ClientId = builder.Configuration["Auth0:ClientId"];
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();
//app.UseHangfireDashboard();

app.MapGet("/Account/Login", async (HttpContext httpContext, string returnUrl = "/") =>
{
  var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
          .WithRedirectUri(returnUrl)
          .Build();

  await httpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
});

app.MapGet("/Account/Logout", async (HttpContext httpContext) =>
{
  var authenticationProperties = new LogoutAuthenticationPropertiesBuilder()
          .WithRedirectUri("/")
          .Build();

  await httpContext.SignOutAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
  await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();