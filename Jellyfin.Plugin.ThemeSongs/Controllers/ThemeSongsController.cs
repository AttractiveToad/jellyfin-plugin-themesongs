using System;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.ThemeSongs.Downloaders;
using Jellyfin.Plugin.ThemeSongs.Services;
using MediaBrowser.Controller.Library;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.ThemeSongs.Controllers;

[ApiController]
[Route("plugins/theme-songs")]
public sealed class ThemeSongsController : ControllerBase
{
    private readonly ILibraryManager _libraryManager;
    private readonly ILogger<ThemeSongsController> _logger;

    public ThemeSongsController(ILibraryManager libraryManager, ILogger<ThemeSongsController> logger)
    {
        _libraryManager = libraryManager;
        _logger = logger;
    }

    [HttpPost("scan-download")]
    public async Task<IActionResult> ScanAndDownload(CancellationToken ct)
    {
        var downloader = new YouTubeAudioDownloader(_logger);
        var worker = new ThemeSongsWorker(_libraryManager, downloader, _logger);
        
        try 
        {
            await worker.RunAsync(ct);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Download failed");
            return StatusCode(500);
        }
    }
}