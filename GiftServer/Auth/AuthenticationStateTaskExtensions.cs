using Microsoft.AspNetCore.Components.Authorization;

namespace GiftServer.Auth;

public static class AuthenticationStateTaskExtensions
{
    public static async Task<string?> GetUsername(this Task<AuthenticationState>? getState)
    {
        string? email = "brandonchastain@gmail.com";

        if (getState is not null)
        {
            var state = await getState;
            email = state?.User?.Identity?.Name ?? email;
        }

        return email;
    }
}