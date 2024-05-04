using MemoDown.Components;
using MemoDown.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Radzen;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(cfg =>
    {
        cfg.LoginPath = "/login";
        cfg.ExpireTimeSpan = TimeSpan.FromDays(3);
        cfg.Events = new CookieAuthenticationEvents
        {
            OnValidatePrincipal = (context) =>
            {
                return Task.CompletedTask;
            }
        };
    });
//builder.Services.AddSingleton(sp =>
//{
//    // Get the address that the app is currently running at
//    var server = sp.GetRequiredService<IServer>();
//    var addressFeature = server.Features.Get<IServerAddressesFeature>();
//    string baseAddress = addressFeature != null ? addressFeature.Addresses.First() : string.Empty;
//    return new HttpClient { BaseAddress = new Uri(baseAddress) };
//});
builder.Services.AddRadzenComponents();

builder.Services.AddOptions<Account>().BindConfiguration(nameof(Account));

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

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapPost("logout", async (
                ClaimsPrincipal user,
                HttpContext context,
                [FromForm] string returnUrl
                ) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.LocalRedirect("/");
}).DisableAntiforgery();

app.Run();
