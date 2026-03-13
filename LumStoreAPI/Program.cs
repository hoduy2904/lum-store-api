using LumStoreAPI.Application;
using LumStoreAPI.Application.Middlewares;
using LumStoreAPI.DataEngine;
using LumStoreAPI.Infrastructure;
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

builder.Services.AddCustomSettings(builder.Configuration);
builder.Services
    .AddHttpContextAccessor()
    .AddLumStoreRepoConfigurations()
    .AddDataEngine()
    .AddLumStoreApplicationConfigurations()
    .AddExceptionHandler<GlobalExceptionHandler>()
    .AddProblemDetails()
    .RegisterTasks()
    .AddCMSCache();

builder.Services.AddJwtAuthentication();

builder.AddLumStoreStaticConfiguration();

var app = builder.Build();

app.UseExceptionHandler();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
