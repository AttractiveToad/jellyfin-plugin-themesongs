using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using YouTubeSearch;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace Jellyfin.Plugin.ThemeSongs.Downloaders;

public sealed class YouTubeAudioDownloader : IAudioDownloader
{
    private readonly ILogger _logger;
    private readonly VideoSearch _videoSearch;
    private readonly YoutubeClient _youtube;

    public YouTubeAudioDownloader(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _videoSearch = new VideoSearch();
        _youtube = new YoutubeClient();
    }


    public async Task<Stream?> DownloadByQueryAsync(string query, string preferredFormat, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Searching for: {Query}", query);
            
            // Suche Video
            var searchResults = await _videoSearch.GetVideos(query, 1);
            if (!searchResults.Any())
            {
                _logger.LogWarning("No videos found for query: {Query}", query);
                return null;
            }

            var videoUrl = searchResults[0].getUrl();
            var videoId = ParseVideoId(videoUrl);
            
            _logger.LogInformation("Found video: {Url}", videoUrl);

            // Hole Streams
            var streamManifest = await _youtube.Videos.Streams.GetManifestAsync(videoId, ct);
            
            // Wähle den besten Audio-Stream (sortiert nach Bitrate)
            var audioStreamInfo = streamManifest.GetAudioStreams()
                .OrderByDescending(s => s.Bitrate)
                .FirstOrDefault();
            
            if (audioStreamInfo == null)
            {
                _logger.LogWarning("No audio stream found for video: {Url}", videoUrl);
                return null;
            }

            // Erstelle temporären Stream
            var memoryStream = new MemoryStream();
            await _youtube.Videos.Streams.CopyToAsync(audioStreamInfo, memoryStream, cancellationToken: ct);
            memoryStream.Position = 0;
            
            return memoryStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading audio for query: {Query}", query);
            return null;
        }
    }

    private static string ParseVideoId(string url)
    {
        var parts = url.Split('=');
        return parts.Length > 1 ? parts[1] : url;
    }
}