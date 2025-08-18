using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.ThemeSongs.Downloaders;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;
using Jellyfin.Data.Enums;

namespace Jellyfin.Plugin.ThemeSongs.Services;

public sealed class ThemeSongsWorker
{
    private readonly ILibraryManager _library;
    private readonly IAudioDownloader _downloader;
    private readonly ILogger _logger;

    public ThemeSongsWorker(
        ILibraryManager library, 
        IAudioDownloader downloader,
        ILogger logger)
    {
        _library = library;
        _downloader = downloader;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        // Handle Movies
        foreach (var movie in _library.GetItemList(new InternalItemsQuery 
        { 
            IncludeItemTypes = new[] { BaseItemKind.Movie },
            IsVirtualItem = false,
            Recursive = true
        }).OfType<Movie>())
        {
            if (ct.IsCancellationRequested) break;
            await DownloadThemeAsync(movie, $"{movie.Name} theme song {movie.ProductionYear}", ct);
        }

        // Handle TV Series
        foreach (var series in _library.GetItemList(new InternalItemsQuery
        {
            IncludeItemTypes = new[] { BaseItemKind.Series },
            IsVirtualItem = false,
            Recursive = true,
            HasTvdbId = true
        }).OfType<Series>())
        {
            if (ct.IsCancellationRequested) break;
            var tvdbId = series.GetProviderId(MetadataProvider.Tvdb);
            if (!string.IsNullOrEmpty(tvdbId))
            {
                await DownloadThemeAsync(series, tvdbId, ct);
            }
        }
    }

    private async Task DownloadThemeAsync(BaseItem item, string query, CancellationToken ct)
    {
        var themePath = Path.Combine(item.Path, "theme.mp3");
        if (File.Exists(themePath)) return;

        try
        {
            using var stream = await _downloader.DownloadByQueryAsync(query, "mp3", ct);
            if (stream != null)
            {
                await using var fileStream = File.Create(themePath);
                await stream.CopyToAsync(fileStream, ct);
                _logger.LogInformation("Downloaded theme for: {Name}", item.Name);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading theme for: {Name}", item.Name);
        }
    }
}