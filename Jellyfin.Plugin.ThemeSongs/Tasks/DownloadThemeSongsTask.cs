using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.ThemeSongs.Downloaders;
using Jellyfin.Plugin.ThemeSongs.Services;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.ThemeSongs.Tasks
{
    public sealed class DownloadThemeSongsTask : IScheduledTask
    {
        public string Name => "Download Theme Songs";
        public string Key => "DownloadThemeSongs";
        public string Description => "Scans the library for series/movies and downloads missing theme songs next to the media files.";
        public string Category => "Theme Songs";

        private readonly ILibraryManager _libraryManager;
        private readonly ILogger<DownloadThemeSongsTask> _logger;

        public DownloadThemeSongsTask(
            ILibraryManager libraryManager,
            ILogger<DownloadThemeSongsTask> logger)
        {
            _libraryManager = libraryManager;
            _logger = logger;
        }

        public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Theme Songs task started.");
            
            var downloader = new YouTubeAudioDownloader(_logger);
            var worker = new ThemeSongsWorker(_libraryManager, downloader, _logger);
            
            await worker.RunAsync(cancellationToken).ConfigureAwait(false);
            
            _logger.LogInformation("Theme Songs task finished.");
        }

        public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
        {
            yield return new TaskTriggerInfo
            {
                Type = TaskTriggerInfo.TriggerInterval,
                IntervalTicks = TimeSpan.FromHours(24).Ticks
            };
        }
    }
}