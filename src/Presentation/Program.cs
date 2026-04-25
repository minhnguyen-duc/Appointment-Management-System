using Presentation;
using Application.Auth.Commands;
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
using Microsoft.EntityFrameworkCore;
using Presentation.Services;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddScoped<RequestOtpCommandHandler>();
builder.Services.AddScoped<VerifyOtpCommandHandler>();

// ── External Services ──
builder.Services.AddScoped<TwilioSmsService>();
builder.Services.AddScoped<ZaloOaSmsService>();
builder.Services.AddScoped<ISmsService, CompositeSmsService>(); // KAN-11: Twilio + Zalo fallback
builder.Services.AddScoped<IEmailService,        SendGridEmailService>();
builder.Services.AddScoped<IPaymentService,      VnPayPaymentService>();
builder.Services.AddScoped<ICalendarSyncService, UnifiedCalendarSyncService>();
builder.Services.AddScoped<IRagService,          RagService>();
builder.Services.AddSingleton<IOtpService,       InMemoryOtpService>();

// ── Presentation Services (Client) ──
builder.Services.AddScoped<AppointmentClientService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<PatientClientService>();
builder.Services.AddScoped<DoctorClientService>();
builder.Services.AddScoped<PaymentClientService>();
builder.Services.AddScoped<RagClientService>();

// ── Blazor ──
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

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
app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode()
   .AddInteractiveWebAssemblyRenderMode();

app.Run();
