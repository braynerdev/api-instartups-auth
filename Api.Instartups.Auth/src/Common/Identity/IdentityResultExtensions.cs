using Api.Instartups.Auth.DTOs;
using Api.Instartups.Auth.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace Api.Instartups.Auth.Common.Identity;

public static class IdentityResultExtensions
{
    public static void EnsureSucceeded(this IdentityResult result)
    {
        if (result.Succeeded)
            return;

        throw new IdentityValidationException(
            result.Errors
                .GroupBy(r => new
                {
                    Field = GetField(r.Code),
                    Code = r.Code
                })
                .Select(group =>
                    new ValidateErrorDTO
                    (
                        GetField(group.First().Code),
                        group.First().Code,
                        group.Select(e => e.Description).ToList()
                    )
                )
        );
    }

    private static string GetField(string code)
    {
        return code switch
        {
            "DuplicateUserName" or "InvalidUserName" => "username",
            "DuplicateEmail" or "InvalidEmail" => "email",

            "PasswordTooShort" or
                "PasswordRequiresDigit" or
                "PasswordRequiresUpper" or
                "PasswordRequiresLower" or
                "PasswordRequiresNonAlphanumeric" or
                "PasswordRequiresUniqueChars" => "password",

            _ => string.Empty
        };
    }
}
