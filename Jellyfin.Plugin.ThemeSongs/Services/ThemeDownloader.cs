using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using Microsoft.Extensions.Logging;
using YoutubeExplode;
using YouTubeSearch;

namespace Jellyfin.Plugin.ThemeSongs.Services
{
    /// <summary>
    /// Service for downloading theme songs from YouTube.
    /// </summary>
    public class ThemeDownloader
    {
        private readonly ILogger<ThemeDownloader> _logger;
        private readonly YoutubeClient _youtube;
        private readonly VideoSearch _videoSearch;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThemeDownloader"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        public ThemeDownloader(ILogger<ThemeDownloader> logger)
        {
            _logger = logger;
            _youtube = new YoutubeClient();
            _videoSearch = new VideoSearch();
        }

        /// <summary>
        /// Downloads a theme song for the specified media item if it doesn't already exist.
        /// </summary>
        /// <param name="item">The media item to download a theme song for.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task DownloadThemeIfMissing(BaseItem item)
        {
            var themePath = Path.Combine(item.ContainingFolderPath, "theme.mp3");
            if (File.Exists(themePath))
            {
                return;
            }

            string? searchQuery = item switch
            {
                Movie movie => $"{movie.Name} {movie.ProductionYear} theme song",
                Series series => $"{series.Name} opening theme song",
                _ => null
            };

            if (searchQuery == null)
            {
                _logger.LogWarning("Unable to build search query for item type: {ItemName} ({ItemType})", item.Name, item.GetType().Name);
                return;
            }

            try
            {
                var searchResults = await _videoSearch.GetVideos(searchQuery, 1).ConfigureAwait(false);

                if (searchResults.Count == 0)
                {
                    _logger.LogWarning("No YouTube results found for: {ItemName}", item.Name);
                    return;
                }

                var videoUrl = searchResults[0].getUrl();
                await DownloadYoutubeAudio(videoUrl, themePath).ConfigureAwait(false);
                _logger.LogInformation("Successfully downloaded theme song for: {ItemName}", item.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to download theme song for: {ItemName}", item.Name);
            }
        }

        /// <summary>
        /// Downloads audio from a YouTube video and saves it to the specified path.
        /// </summary>
        /// <param name="videoUrl">The YouTube video URL.</param>
        /// <param name="outputPath">The output file path for the audio.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task DownloadYoutubeAudio(string videoUrl, string outputPath)
        {
            try
            {
                var videoId = ExtractVideoId(videoUrl);
                var streamManifest = await _youtube.Videos.Streams.GetManifestAsync(videoId).ConfigureAwait(false);

                var audioStreamInfo = streamManifest.GetAudioStreams()
                    .OrderByDescending(x => x.Bitrate)
                    .FirstOrDefault();

                if (audioStreamInfo == null)
                {
                    _logger.LogError("No audio stream found for video: {VideoId}", videoId);
                    throw new InvalidOperationException("No audio stream found");
                }

                await _youtube.Videos.Streams.DownloadAsync(audioStreamInfo, outputPath).ConfigureAwait(false);
            }
            catch (Exception ex) when (!(ex is InvalidOperationException))
            {
                _logger.LogError(ex, "Failed to download YouTube audio from {VideoUrl} to {OutputPath}", videoUrl, outputPath);
                throw new InvalidOperationException($"Failed to download audio from YouTube: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Extracts the video ID from a YouTube URL.
        /// </summary>
        /// <param name="url">The YouTube URL.</param>
        /// <returns>The extracted video ID.</returns>
        private string ExtractVideoId(string url)
        {
            if (url.Contains("youtube.com", StringComparison.OrdinalIgnoreCase) || url.Contains("youtu.be", StringComparison.OrdinalIgnoreCase))
            {
                var uri = new Uri(url);
                if (url.Contains("youtube.com", StringComparison.OrdinalIgnoreCase))
                {
                    var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                    return query["v"] ?? url;
                }
                else if (url.Contains("youtu.be", StringComparison.OrdinalIgnoreCase))
                {
                    return uri.Segments.Last();
                }
            }

            return url;
        }
    }
}