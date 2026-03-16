using System;
using System.Security.Claims;
using LumStoreAPI.Application.DTOs.Responses;
using LumStoreAPI.Core.Models.Constants.Systems;
using LumStoreAPI.Core.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;

namespace LumStoreAPI.Application.Middlewares;

public class AuthorizeHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();
    public async Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Forbidden && (context.User.FindFirstValue("Status") ?? "").Equals(nameof(AccountStatus.INACTIVE)))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(APIResponseBase.Failure(ErrorStatusNameConstants.ACCOUNT_INACTIVE, ["Your account doesn't active"]));
            return;
        }
        await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
    }
}
