using LumStoreAPI.Application.DTOs.Responses;
using LumStoreAPI.Application.Exceptions;
using LumStoreAPI.Core.Interfaces.Sytems;
using LumStoreAPI.Core.Models.Constants.Systems;
using LumStoreAPI.Core.Models.Enums;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Net;

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
            if (exception is ForbidException || exception is AuthInvalidException)
            {
                httpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                if (exception is AuthInvalidException authInvalidException)
                {
                    string status = ErrorStatusNameConstants.ACCOUNT_LOCKED;
                    if (authInvalidException.Status == Core.Models.Enums.AccountStatus.INACTIVE)
                    {
                        status = ErrorStatusNameConstants.ACCOUNT_INACTIVE;
                    }

                    await httpContext.Response.WriteAsJsonAsync(APIResponseBase.Failure(status, [authInvalidException.Message]));
                }
                return true;
            }
            var stackTrace = new StackTrace(exception, true);
            var frame = stackTrace.GetFrame(0);
            var method = frame?.GetMethod();

            string code = "Error";
            string source = "Exception";
            if (method != null)
            {
                code = method.Name;
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
