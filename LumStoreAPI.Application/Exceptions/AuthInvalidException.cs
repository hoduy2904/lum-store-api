using System;
using LumStoreAPI.Core.Models.Enums;

namespace LumStoreAPI.Application.Exceptions;

public class AuthInvalidException : Exception
{
    public AccountStatus Status { get; set; }
    public AuthInvalidException(AccountStatus status, string message) : base(message)
    {
        this.Status = status;
    }
}
