using Presentation;
using Application.Auth.Commands;
using Presentation.Components.Shared;
using Application.Appointments.Commands;
using Application.Common.Interfaces;
using Infrastructure.AI.SemanticKernel;
using Infrastructure.ExternalServices.Calendar;
using Infrastructure.ExternalServices.SendGrid;
using Infrastructure.ExternalServices.Twilio;
using Infrastructure.ExternalServices.Zalo;
using Infrastructure.ExternalServices;
using Infrastructure.ExternalServices.VnPay;
using Infrastructure.Identity;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Presentation.Services;

var builder = WebApplication.CreateBuilder(args);

// Serve NuGet-sourced Blazor framework files (_framework/blazor.web.js) in all environments
// Must be called on WebHostBuilder, not on WebApplication
builder.WebHost.UseStaticWebAssets();

// ── HttpContext ──
builder.Services.AddHttpContextAccessor();

// ── Database ──
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Repositories ──
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IPatientRepository, PatientRepository>();

// ── Query / Command Services ──
builder.Services.AddScoped<IAppointmentQueryService, AppointmentQueryService>();
builder.Services.AddScoped<IPatientQueryService,     PatientQueryService>();
builder.Services.AddScoped<IPatientCommandService,   PatientCommandService>();
builder.Services.AddScoped<IDoctorQueryService,      DoctorQueryService>();

// ── Application Command Handlers ──
builder.Services.AddScoped<BookAppointmentCommandHandler>();
builder.Services.AddScoped<CheckPhoneCommandHandler>();
builder.Services.AddScoped<RequestOtpCommandHandler>();
builder.Services.AddScoped<VerifyOtpCommandHandler>();
builder.Services.AddScoped<LoginWithPasswordCommandHandler>();
builder.Services.AddScoped<SetPasswordCommandHandler>();

// ── External Services ──
builder.Services.AddScoped<TwilioSmsService>();
builder.Services.AddScoped<ZaloOaSmsService>();
builder.Services.AddScoped<ISmsService, CompositeSmsService>(); // KAN-11: Twilio + Zalo fallback
builder.Services.AddScoped<IEmailService,        SendGridEmailService>();
builder.Services.AddScoped<IPaymentService,      VnPayPaymentService>();
builder.Services.AddScoped<ICalendarSyncService, UnifiedCalendarSyncService>();
builder.Services.AddScoped<IRagService,          RagService>();
builder.Services.AddSingleton<IOtpService,       InMemoryOtpService>();

// ── Auth ticket store (fixes Blazor SignalR/SignInAsync conflict) ──
builder.Services.AddSingleton<Presentation.Services.PendingLoginStore>();

// ── Presentation Services (Client) ──
builder.Services.AddScoped<AppointmentClientService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<PatientClientService>();
builder.Services.AddScoped<DoctorClientService>();
builder.Services.AddScoped<PaymentClientService>();
builder.Services.AddScoped<RagClientService>();

// ── Blazor ──
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ── Authentication & Authorization ──
// KAN-11: Cookie-based session after OTP verification
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath         = "/auth/login";
        options.LogoutPath        = "/auth/logout";
        options.ExpireTimeSpan    = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

// ── CORS (nếu dùng WebAssembly client riêng) ──
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// ── Auto-migrate DB khi startup (Dev only) ──
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
        await Infrastructure.Persistence.SeedData.SeedAsync(db);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode() // WASM mode omitted: no .Client project (see arch spec §2.1)
   .WithStaticAssets();

// ── /auth/do-login: set cookie in real HTTP context (not SignalR) ──
app.MapGet("/auth/do-login", async (
    string ticket, string returnUrl, string? msg,
    Presentation.Services.PendingLoginStore store,
    HttpContext ctx) =>
{
    var claims = store.Consume(ticket);
    if (claims is null)
        return Results.Redirect("/auth/login?error=ticket_expired");

    var identity  = new System.Security.Claims.ClaimsIdentity(claims, "Cookies");
    var principal = new System.Security.Claims.ClaimsPrincipal(identity);
    await ctx.SignInAsync("Cookies", principal, new Microsoft.AspNetCore.Authentication.AuthenticationProperties
    {
        IsPersistent = true,
        ExpiresUtc   = DateTimeOffset.UtcNow.AddHours(8),
    });

    // Pass toast via ?t= query param; Homepage will replaceState to clean URL
    var dest = string.IsNullOrWhiteSpace(returnUrl) ? "/homepage" : returnUrl;
    if (!string.IsNullOrWhiteSpace(msg))
        dest += (dest.Contains('?') ? "&" : "?") + "t=" + Uri.EscapeDataString(msg);
    return Results.Redirect(dest);
});

// ── /auth/do-logout: sign out, show toast, stay on homepage ──
app.MapGet("/auth/do-logout", async (HttpContext ctx) =>
{
    // Sign out explicitly with scheme name — works on GET without antiforgery
    await ctx.SignOutAsync("Cookies", new Microsoft.AspNetCore.Authentication.AuthenticationProperties
    {
        RedirectUri = null
    });
    // Delete the auth cookie manually as a safety net
    ctx.Response.Cookies.Delete(".AspNetCore.Cookies");
    ctx.Response.Cookies.Delete(".AspNetCore.Cookies.C1");

    var toastMsg = Uri.EscapeDataString("Bạn đã đăng xuất thành công");
    return Results.Redirect($"/homepage?t={toastMsg}");
}).ExcludeFromDescription();

// ── /auth/logout: legacy — redirect to login ──
app.MapGet("/auth/logout", async (HttpContext ctx) =>
{
    await ctx.SignOutAsync("Cookies");
    return Results.Redirect("/auth/login");
}).ExcludeFromDescription();

app.Run();
