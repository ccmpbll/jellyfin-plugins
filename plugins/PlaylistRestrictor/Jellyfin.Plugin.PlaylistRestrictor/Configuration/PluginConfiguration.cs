using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.PlaylistRestrictor.Configuration;

public class PluginConfiguration : BasePluginConfiguration
{
    public bool RestrictPlaylistCreation { get; set; } = true;
}
