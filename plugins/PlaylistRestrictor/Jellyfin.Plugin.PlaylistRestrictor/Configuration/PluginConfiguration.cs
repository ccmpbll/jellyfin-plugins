using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.PlaylistRestrictor.Configuration;

public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>Master switch.</summary>
    public bool RestrictPlaylistCreation { get; set; } = true;

    /// <summary>False: block every non-admin user. True: block only the users in <see cref="BlockedUserIds"/>.</summary>
    public bool BlockSelectedUsersOnly { get; set; }

    /// <summary>User IDs blocked when <see cref="BlockSelectedUsersOnly"/> is on.</summary>
    public string[] BlockedUserIds { get; set; } = [];
}
