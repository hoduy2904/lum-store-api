using LumStoreAPI.Application.DTOs.Responses;
using LumStoreAPI.Application.Exceptions;
using LumStoreAPI.Core.Interfaces.Sytems;
using LumStoreAPI.Core.Models.Constants.Systems;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
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

            string message = exception.Message;
            if (exception is SystemException)
            {
                message = "Internal server error";
            }
            else if (exception is SqlException sqlException)
            {
                message = "Database error";

                if (sqlException.Number == 2627)
                {
                    message = "Data already exists";
                }
                else if (sqlException.Number == 515)
                {
                    message = "Missing required data";
                }
            }

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Server Error",
                Detail = message,
                Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}"
            };

            httpContext.Response.StatusCode = problemDetails.Status.Value;

            await httpContext.Response
            .WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}
