using Microsoft.AspNetCore.Cors.Infrastructure;
using Server.Authentication;
using GiftServer.Data;
using GiftServer.Services;
using GiftServer.Config;

var builder = WebApplication.CreateBuilder(args);

// Add configuration from appsettings.json
builder.Configuration.AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile("appsettings.Development.json", optional: true);
var config = GiftServerConfig.LoadFromAppSettings(builder.Configuration);

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxConcurrentConnections = 50;
    serverOptions.Limits.MaxConcurrentUpgradedConnections = 50;
    serverOptions.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10MB
});

var configureCors = (CorsOptions options) => 
{
    options.AddPolicy(
        name: "AllowSpecificOrigins",
        policy =>
        {
            policy
            .WithOrigins("*")
            .AllowAnyHeader()
            .AllowAnyMethod();
        });
};

builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.ClearProviders();
    loggingBuilder.AddConsole();
    loggingBuilder.AddDebug();
});

builder.Services.AddCors(configureCors);
builder.Services.AddControllers();

// Add authentication
builder.Services.AddAuthentication(StaticWebAppsAuthenticationHandler.AuthenticationScheme)
    .AddScheme<StaticWebAppsAuthenticationOptions, StaticWebAppsAuthenticationHandler>(
        StaticWebAppsAuthenticationHandler.AuthenticationScheme,
        options => { options.IsTestUserEnabled = config.IsTestUserEnabled; });

builder.Services.AddAuthorization();

// Gifted services
builder.Services
.AddSingleton<GiftServerConfig>(_ => config)
.AddSingleton<IUserRepository>(sb =>
{
    return new SQLiteUserRepository(
        $"Data Source={config.DbPath};Mode=ReadWriteCreate;Cache=Shared;Pooling=True",
        sb.GetRequiredService<ILogger<SQLiteUserRepository>>());
})
.AddSingleton<IStoreRepository>(sb =>
{
    return new SQLiteStoreRepository(
        $"Data Source={config.DbPath};Mode=ReadWriteCreate;Cache=Shared;Pooling=True",
        sb.GetRequiredService<ILogger<SQLiteStoreRepository>>());
})
.AddSingleton<IPersonRepository>(sb =>
{
    return new SQLitePersonRepository(
        $"Data Source={config.DbPath};Mode=ReadWriteCreate;Cache=Shared;Pooling=True",
        sb.GetRequiredService<ILogger<SQLitePersonRepository>>());
})
.AddSingleton<IGiftRepository>(sb =>
{
    return new SQLiteGiftRepository(
        $"Data Source={config.DbPath};Mode=ReadWriteCreate;Cache=Shared;Pooling=True",
        sb.GetRequiredService<ILogger<SQLiteGiftRepository>>());
})
.AddSingleton<DatabaseBackupService>()
.AddHostedService<DatabaseBackupService>(p => p.GetRequiredService<DatabaseBackupService>());

if (!builder.Environment.IsDevelopment())
{
    builder.WebHost.UseKestrel();
}

var app = builder.Build();

// Restore database from backup BEFORE building the app
var backup = app.Services.GetRequiredService<DatabaseBackupService>();
await backup.RestoreFromBackupAsync(CancellationToken.None);

// Instantiate repositories to ensure db tables are created in order.
var a = app.Services.GetRequiredService<IUserRepository>();
var b = app.Services.GetRequiredService<IStoreRepository>();
var c = app.Services.GetRequiredService<IPersonRepository>();
var d = app.Services.GetRequiredService<IGiftRepository>();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowSpecificOrigins");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();