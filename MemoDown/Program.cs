using MemoDown.Components;
using MemoDown.Options;
using MemoDown.Services;
using MemoDown.Store;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Radzen;
using System.Security.Claims;
using NotificationService = MemoDown.Services.NotificationService;
using ContextMenuService = MemoDown.Services.ContextMenuService;
using DialogService = MemoDown.Services.DialogService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddHubOptions(opt =>
    {
        // https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/signalr?view=aspnetcore-8.0#maximum-receive-message-size
        opt.MaximumReceiveMessageSize = 1024 * 1024 * 20; // 20m, default is 32k
    });

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(cfg =>
    {
        cfg.Cookie.Name = "memodown.cookies";
        cfg.LoginPath = "/login";
        cfg.ExpireTimeSpan = TimeSpan.FromDays(3);
        cfg.SlidingExpiration = true;
        //cfg.Events = new CookieAuthenticationEvents
        //{
        //    OnValidatePrincipal = (context) =>
        //    {
        //        return Task.CompletedTask;
        //    }
        //};
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

builder.Services.AddSingleton<MemoStore>();
builder.Services.AddHostedService<MemoStoreInitializeService>();
builder.Services.AddSingleton<MemoService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<ContextMenuService>();
builder.Services.AddScoped<DialogService>();

builder.Services.AddOptions<AccountOptions>().BindConfiguration("Account");
builder.Services.AddOptions<MemoDownOptions>().BindConfiguration("MemoDown");

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
