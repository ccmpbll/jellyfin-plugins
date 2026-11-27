using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Data.Enums;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.NoTrailers;

/// <summary>Sweeps the whole library and clears remote trailers. Runs at startup and daily.</summary>
public class StripTrailersTask : IScheduledTask
{
    private readonly ILibraryManager _libraryManager;
    private readonly ILogger<StripTrailersTask> _logger;

    /// <summary>Initializes a new instance of the <see cref="StripTrailersTask"/> class.</summary>
    public StripTrailersTask(ILibraryManager libraryManager, ILogger<StripTrailersTask> logger)
    {
        _libraryManager = libraryManager;
        _logger = logger;
    }

    /// <inheritdoc />
    public string Name => "Remove remote trailers";

    /// <inheritdoc />
    public string Key => "NoTrailersStripRemoteTrailers";

    /// <inheritdoc />
    public string Description => "Clears remote trailer links from all movies, series and episodes.";

    /// <inheritdoc />
    public string Category => "No Trailers";

    /// <inheritdoc />
    public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        var items = _libraryManager.GetItemList(new InternalItemsQuery
        {
            IncludeItemTypes = [BaseItemKind.Movie, BaseItemKind.Series, BaseItemKind.Season, BaseItemKind.Episode],
            Recursive = true,
            IsVirtualItem = false,
        });

        var cleared = 0;
        for (var i = 0; i < items.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (await TrailerStripper.StripAsync(items[i], cancellationToken).ConfigureAwait(false))
            {
                cleared++;
            }

            progress.Report(100.0 * (i + 1) / items.Count);
        }

        _logger.LogInformation("Removed remote trailers from {Cleared} of {Total} items", cleared, items.Count);
    }

    /// <inheritdoc />
    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
    {
        return
        [
            new TaskTriggerInfo { Type = TaskTriggerInfoType.StartupTrigger },
            new TaskTriggerInfo { Type = TaskTriggerInfoType.DailyTrigger, TimeOfDayTicks = TimeSpan.FromHours(4).Ticks },
        ];
    }
}
