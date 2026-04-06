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

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(typeof(LumStoreApplicationConfiguration).Assembly, typeof(Program).Assembly);
});

builder.AddLumStoreStaticConfiguration();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.WithOrigins("http://localhost:3000")
            .AllowCredentials()// Allow requests from all origins
                   .AllowAnyMethod() // Allow all HTTP methods (GET, POST, PUT, DELETE, etc.)
                   .AllowAnyHeader(); // Allow all request headers
        });
});
var app = builder.Build();

app.UseExceptionHandler();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseCors("AllowAll");

app.MapControllers();

app.Run();
