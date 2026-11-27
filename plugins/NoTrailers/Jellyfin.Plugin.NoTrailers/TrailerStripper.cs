using System;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.NoTrailers;

/// <summary>Clears RemoteTrailers whenever an item is added or updated (metadata refreshes re-add them from TMDb).</summary>
public sealed class TrailerStripper : IHostedService
{
    private readonly ILibraryManager _libraryManager;
    private readonly ILogger<TrailerStripper> _logger;

    /// <summary>Initializes a new instance of the <see cref="TrailerStripper"/> class.</summary>
    public TrailerStripper(ILibraryManager libraryManager, ILogger<TrailerStripper> logger)
    {
        _libraryManager = libraryManager;
        _logger = logger;
    }

    /// <summary>Clears an item's remote trailers. Returns false when there was nothing to clear.</summary>
    public static async Task<bool> StripAsync(BaseItem item, CancellationToken cancellationToken)
    {
        // Guard: only write when there is something to remove, so our own update doesn't loop.
        if (item.RemoteTrailers is not { Count: > 0 })
        {
            return false;
        }

        item.RemoteTrailers = Array.Empty<MediaUrl>();
        await item.UpdateToRepositoryAsync(ItemUpdateType.MetadataEdit, cancellationToken).ConfigureAwait(false);
        return true;
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _libraryManager.ItemAdded += OnItemChanged;
        _libraryManager.ItemUpdated += OnItemChanged;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _libraryManager.ItemAdded -= OnItemChanged;
        _libraryManager.ItemUpdated -= OnItemChanged;
        return Task.CompletedTask;
    }

    private void OnItemChanged(object? sender, ItemChangeEventArgs e)
    {
        var item = e.Item;
        if (item.RemoteTrailers is not { Count: > 0 })
        {
            return;
        }

        // Off the event thread: the library manager raises these synchronously.
        _ = Task.Run(async () =>
        {
            try
            {
                if (await StripAsync(item, CancellationToken.None).ConfigureAwait(false))
                {
                    _logger.LogInformation("Removed remote trailers from {Name} ({Id})", item.Name, item.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove remote trailers from {Name} ({Id})", item.Name, item.Id);
            }
        });
    }
}
