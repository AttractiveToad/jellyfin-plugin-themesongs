using System;
using System.Threading.Tasks;
using Jellyfin.Data.Enums;
using Jellyfin.Plugin.ThemeSongs.Services;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.ThemeSongs.Controller
{
    /// <summary>
    /// Theme Songs API controller.
    /// </summary>
    [ApiController]
    [Route("ThemeSongs")]
    public class ThemeSongsController : ControllerBase
    {
        private readonly ILibraryManager _libraryManager;
        private readonly ThemeDownloader _themeDownloader;
        private readonly ILogger<ThemeSongsController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThemeSongsController"/> class.
        /// </summary>
        /// <param name="libraryManager">The library manager instance.</param>
        /// <param name="themeDownloader">The theme downloader service.</param>
        /// <param name="logger">The logger instance.</param>
        public ThemeSongsController(
            ILibraryManager libraryManager,
            ThemeDownloader themeDownloader,
            ILogger<ThemeSongsController> logger)
        {
            _libraryManager = libraryManager;
            _themeDownloader = themeDownloader;
            _logger = logger;
        }

        /// <summary>
        /// Starts the theme songs download task for all movies and series in the library.
        /// </summary>
        /// <returns>A <see cref="ActionResult"/> indicating the operation was accepted.</returns>
        [HttpPost("DownloadThemeSongs")]
        public ActionResult DownloadThemeSongs()
        {
            _logger.LogInformation("Starting Theme Songs download");
            _ = Task.Run(async () =>
            {
                try
                {
                    var items = _libraryManager.GetItemList(new InternalItemsQuery
                    {
                        IncludeItemTypes = new[] { BaseItemKind.Movie, BaseItemKind.Series }
                    });
                    foreach (var item in items)
                    {
                        await _themeDownloader.DownloadThemeIfMissing(item).ConfigureAwait(false);
                    }

                    _logger.LogInformation("Theme Songs download completed");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during theme songs download");
                }
            });
            return NoContent();
        }
    }
}