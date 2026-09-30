using LogMyOJT.Components;
using LogMyOJT.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

// no database yet, so whatever the user types just lives in memory for their session
builder.Services.AddScoped<UserState>();

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
