using Coravel;
using MemoDown.Components;
using MemoDown.Constants;
using MemoDown.Options;
using MemoDown.Services;
using MemoDown.Services.Background;
using MemoDown.Store;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Radzen;
using System.Security.Claims;
using ContextMenuService = MemoDown.Services.ContextMenuService;
using DialogService = MemoDown.Services.DialogService;
using NotificationService = MemoDown.Services.NotificationService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddHubOptions(opt =>
    {
        // https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/signalr?view=aspnetcore-8.0#maximum-receive-message-size
        opt.MaximumReceiveMessageSize = MemoConstants.MaxUploadSize; // 25m, default is 32k
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

builder.Services.AddHttpClient();

builder.Services.AddRadzenComponents();

builder.Services.AddSingleton<MemoStore>();
//builder.Services.AddHostedService<MemoStoreInitializeService>();
//builder.Services.AddHostedService<UploadsCleanupService>();
builder.Services.AddSingleton<MemoService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<ContextMenuService>();
builder.Services.AddScoped<DialogService>();
builder.Services.AddSingleton<CloudflareTurnstileService>();
builder.Services.AddSingleton<GithubSyncService>();
builder.Services.AddSingleton<GithubAutoSyncService>();
builder.Services.AddSingleton<UploadsCleanupService>();

builder.Services.AddOptions<MemoDownOptions>()
    .BindConfiguration(MemoDownOptions.Key)
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddSingleton<IValidateOptions<MemoDownOptions>, MemoDownOptionsValidation>();

builder.Services.AddScheduler();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    //app.UseHsts();
}

//app.UseHttpsRedirection();

app.UseStaticFiles();

// used for uploading, define a new request path
var memoDownOptions = app.Services.GetRequiredService<IOptions<MemoDownOptions>>().Value;
if (!string.IsNullOrWhiteSpace(memoDownOptions.UploadsRelativePath))
{
    var uploadsDir = Path.Combine(memoDownOptions.MemoDir, memoDownOptions.UploadsRelativePath);
    if (!Directory.Exists(uploadsDir))
    {
        Directory.CreateDirectory(uploadsDir);
    }

    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(uploadsDir),
        RequestPath = $"/{memoDownOptions.UploadsVirtualPath}"
    });
}

app.Services.UseScheduler(scheduler =>
{
    scheduler
    .Schedule<UploadsCleanupService>()
    .Cron(memoDownOptions.CleanUploadsAt)
    .Zoned(TimeZoneInfo.Local)
    .PreventOverlapping(nameof(UploadsCleanupService));

    if (memoDownOptions.Github.Enable && memoDownOptions.Github.EnableAutoSync)
    {
        scheduler
        .Schedule<GithubAutoSyncService>()
        .Cron(memoDownOptions.Github.AutoSyncAt)
        .Zoned(TimeZoneInfo.Local)
        .PreventOverlapping(nameof(GithubAutoSyncService));
    }
});

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapPost("logout", async (
                ClaimsPrincipal user,
                HttpContext context,
                [FromForm] string? returnUrl
                ) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.LocalRedirect("/");
}).DisableAntiforgery();

app.Run();
