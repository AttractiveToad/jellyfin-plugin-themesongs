using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Data.Enums;
using Jellyfin.Plugin.ThemeSongs.Services;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.ThemeSongs.Tasks
{
    /// <summary>
    /// Scheduled task for downloading missing theme songs for movies and shows.
    /// </summary>
    public class DownloadThemeSongsTask : IScheduledTask
    {
        private readonly ILibraryManager _libraryManager;
        private readonly ThemeDownloader _themeDownloader;
        private readonly ILogger<DownloadThemeSongsTask> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DownloadThemeSongsTask"/> class.
        /// </summary>
        /// <param name="libraryManager">The library manager instance.</param>
        /// <param name="themeDownloader">The theme downloader service.</param>
        /// <param name="logger">The logger instance.</param>
        public DownloadThemeSongsTask(
            ILibraryManager libraryManager,
            ThemeDownloader themeDownloader,
            ILogger<DownloadThemeSongsTask> logger)
        {
            _libraryManager = libraryManager;
            _themeDownloader = themeDownloader;
            _logger = logger;
        }

        /// <summary>
        /// Gets the task name.
        /// </summary>
        public string Name => "Download Missing Theme Songs";

        /// <summary>
        /// Gets the task key identifier.
        /// </summary>
        public string Key => "DownloadMissingThemeSongs";

        /// <summary>
        /// Gets the task description.
        /// </summary>
        public string Description => "Downloads missing theme songs for movies and shows.";

        /// <summary>
        /// Gets the task category.
        /// </summary>
        public string Category => "Theme Songs";

        /// <summary>
        /// Executes the scheduled task to download missing theme songs.
        /// </summary>
        /// <param name="progress">Progress reporter for the task.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Theme Songs task started.");
            var items = _libraryManager.GetItemList(new InternalItemsQuery
            {
                IncludeItemTypes = new[] { BaseItemKind.Movie, BaseItemKind.Series }
            });

            var total = items.Count;
            var current = 0;

            foreach (var item in items)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                await _themeDownloader.DownloadThemeIfMissing(item).ConfigureAwait(false);
                current++;
                progress.Report((double)current / total * 100);
            }

            _logger.LogInformation("Theme Songs task finished.");
        }

        /// <summary>
        /// Gets the default triggers for this scheduled task.
        /// </summary>
        /// <returns>Collection of default task triggers.</returns>
        public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
        {
            yield return new TaskTriggerInfo
            {
                Type = TaskTriggerInfo.TriggerInterval,
                IntervalTicks = TimeSpan.FromHours(12).Ticks
            };
        }
    }
}