using LumStoreAPI.Application;
using LumStoreAPI.Application.Middlewares;
using LumStoreAPI.DataEngine;
using LumStoreAPI.Infrastructure;
using LumStoreAPI.Infrastructure.SeedData;
using LumStoreAPI.SDK;
using LumStoreAPI.Tasks;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) =>
{
    lc
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: LogEventLevel.Error);
});
// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Configuration.SDKConfigure();
builder.Services.AddCustomSettings(builder.Configuration);
builder.Services
    .AddHttpContextAccessor()
    .AddLumStoreRepoConfigurations()
    .AddDataEngine()
    .AddLumStoreApplicationConfigurations()
    .AddExceptionHandler<GlobalExceptionHandler>()
    .AddProblemDetails()
    .RegisterTasks()
    .AddCMSCache()
    .AddLumStoreSDK();

builder.Services.AddJwtAuthentication();

// ── HttpClient for external integrations ──────────────────────────────────
builder.Services.AddHttpClient("Shiprelay");
builder.Services.AddHttpClient("WMS");
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(typeof(LumStoreApplicationConfiguration).Assembly, typeof(Program).Assembly);
});

builder.AddLumStoreStaticConfiguration();

var app = builder.Build();

await LumStoreSeedData.SeedAsync(app.Services);

app.UseExceptionHandler();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
else
{
    app.UseDefaultFiles();
    app.UseStaticFiles();
    app.MapFallbackToFile("index.html");
}

//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
