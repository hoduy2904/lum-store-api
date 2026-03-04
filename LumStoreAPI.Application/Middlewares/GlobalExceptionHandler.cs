using LumStoreAPI.Core.Interfaces.Sytems;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace LumStoreAPI.Application.Middlewares
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly IServiceProvider _serviceProvider;

        public GlobalExceptionHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var stackTrace = new StackTrace(exception, true);
            var frame = stackTrace.GetFrame(0);
            var method = frame?.GetMethod();

            string code = "Error";
            string source = "Exception";
            if (method != null)
            {
                code = method.Name;
                string? className = method.DeclaringType?.FullName;
                source = method.DeclaringType?.Namespace ?? "Global Error";
            }
            using (var scope = _serviceProvider.CreateScope())
            {
                var eventLogService = scope.ServiceProvider.GetRequiredService<IEventLogService>();
                await eventLogService.LogException(source, code, "", exception);
            }

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Server Error",
                Detail = exception.Message,
                Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}"
            };

            httpContext.Response.StatusCode = problemDetails.Status.Value;

            await httpContext.Response
            .WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}
