using GiftServer.Components;
using GiftServer;
using IniParser;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var parser = new FileIniDataParser();
var config = parser.ReadFile("appconfig.ini");
var dbUrl = config["Turso"]["dbUrl"];
var authToken = config["Turso"]["authToken"];

builder.Services.AddSingleton<DataAccess>();
builder.Services.AddSingleton((_) => new TursoClient(dbUrl, authToken));

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

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
