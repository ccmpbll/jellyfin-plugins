using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.PlaylistRestrictor;

public class PlaylistRestrictionFilter : IActionFilter
{
    private readonly ILogger<PlaylistRestrictionFilter> _logger;

    public PlaylistRestrictionFilter(ILogger<PlaylistRestrictionFilter> logger)
    {
        _logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!IsRestrictionEnabled())
        {
            return;
        }

        if (!IsPlaylistCreationRequest(context))
        {
            return;
        }

        var user = context.HttpContext.User;
        var isAdmin = user.IsInRole("Administrator") || HasJellyfinAdminClaim(user);
        if (isAdmin)
        {
            return;
        }

        var userId = user.FindFirst("Jellyfin-UserId")?.Value
                     ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!IsBlocked(userId))
        {
            return;
        }

        _logger.LogInformation("Blocked playlist change for user {UserId}", userId ?? "unknown");

        context.Result = new JsonResult(new { message = "You are not allowed to create or change playlists." })
        {
            StatusCode = 403
        };
    }

    private static bool IsBlocked(string? userId)
    {
        var config = Plugin.Instance?.Configuration;
        if (config is null || !config.BlockSelectedUsersOnly)
        {
            return true;
        }

        if (!Guid.TryParse(userId, out var id))
        {
            return false;
        }

        foreach (var blocked in config.BlockedUserIds)
        {
            if (Guid.TryParse(blocked, out var b) && b == id)
            {
                return true;
            }
        }

        return false;
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }

    private static bool IsPlaylistCreationRequest(ActionExecutingContext context)
    {
        var request = context.HttpContext.Request;
        return request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase)
               && request.Path.StartsWithSegments("/Playlists", StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasJellyfinAdminClaim(ClaimsPrincipal user)
    {
        return user.HasClaim("Jellyfin-IsApiAdminClaim", "true")
               || user.HasClaim(ClaimTypes.Role, "Administrator");
    }

    private static bool IsRestrictionEnabled()
    {
        return Plugin.Instance?.Configuration.RestrictPlaylistCreation ?? true;
    }
}
