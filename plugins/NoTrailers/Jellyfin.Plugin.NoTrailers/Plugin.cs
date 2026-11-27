using System;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.NoTrailers;

/// <summary>Removes remote (YouTube) trailer links from library items.</summary>
public class Plugin : BasePlugin<BasePluginConfiguration>
{
    /// <summary>Initializes a new instance of the <see cref="Plugin"/> class.</summary>
    public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
        : base(applicationPaths, xmlSerializer)
    {
    }

    /// <inheritdoc />
    public override string Name => "No Trailers";

    /// <inheritdoc />
    public override Guid Id => Guid.Parse("c00b58f5-93d6-48db-afa1-a152ca82704d");

    /// <inheritdoc />
    public override string Description => "Strips remote trailer links from movies, series and episodes so the Trailer button never shows.";
}
