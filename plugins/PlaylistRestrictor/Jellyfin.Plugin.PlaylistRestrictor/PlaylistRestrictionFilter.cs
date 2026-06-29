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

        var isAdmin = context.HttpContext.User.IsInRole("Administrator")
                      || HasJellyfinAdminClaim(context.HttpContext.User);

        if (!isAdmin)
        {
            var userId = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? context.HttpContext.User.FindFirst("Jellyfin-UserId")?.Value
                         ?? "unknown";

            _logger.LogInformation("Blocked playlist creation for non-admin user {UserId}", userId);

            context.Result = new JsonResult(new { message = "Playlist creation is restricted to administrators." })
            {
                StatusCode = 403
            };
        }
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
