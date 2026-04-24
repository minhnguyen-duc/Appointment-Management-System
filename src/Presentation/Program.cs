using Infrastructure.ExternalServices.Calendar;
using Infrastructure.ExternalServices.SendGrid;
using Infrastructure.ExternalServices.Twilio;
using Infrastructure.ExternalServices.VnPay;
using Infrastructure.AI.SemanticKernel;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Repository ──
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();

// ── External Services ──
builder.Services.AddScoped<ISmsService, TwilioSmsService>();
builder.Services.AddScoped<IEmailService, SendGridEmailService>();
builder.Services.AddScoped<IPaymentService, VnPayPaymentService>();
builder.Services.AddScoped<ICalendarSyncService, UnifiedCalendarSyncService>();

// ── AI / RAG ──
builder.Services.AddScoped<IRagService, RagService>();

// ── Blazor ──
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode().AddInteractiveWebAssemblyRenderMode();

app.Run();
