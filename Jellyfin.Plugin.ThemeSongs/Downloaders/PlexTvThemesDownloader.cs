using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.ThemeSongs.Downloaders;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.ThemeSongs.Downloaders;

public sealed class PlexTvThemesDownloader : IAudioDownloader
{
    private readonly HttpClient _http;
    private readonly ILogger<PlexTvThemesDownloader> _logger;

    public PlexTvThemesDownloader(HttpClient http, ILogger<PlexTvThemesDownloader> logger)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // query = TVDB-ID der Serie, preferredFormat wird ignoriert (Server liefert MP3)
    public async Task<Stream?> DownloadByQueryAsync(string query, string preferredFormat, CancellationToken ct)
    {
        var tvdbId = query?.Trim();
        if (string.IsNullOrWhiteSpace(tvdbId))
        {
            _logger.LogDebug("PlexTvThemes: keine TVDB-ID übergeben.");
            return null;
        }

        foreach (var scheme in new[] { "https", "http" })
        {
            var url = $"{scheme}://tvthemes.plexapp.com/{Uri.EscapeDataString(tvdbId)}.mp3";
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("audio/mpeg"));
            req.Headers.UserAgent.ParseAdd("ThemeSongsPlugin/1.0");

            try
            {
                var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode)
                {
                    _logger.LogDebug("PlexTvThemes: HTTP {Status} für TVDB {TvdbId} ({Url})", (int)resp.StatusCode, tvdbId, url);
                    resp.Dispose();
                    continue;
                }

                // Erfolgreich: Stream an Aufrufer zurückgeben (Aufrufer disposed den Stream)
                return await resp.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "PlexTvThemes: Fehler beim Abruf für TVDB {TvdbId} ({Url})", tvdbId, url);
            }
        }

        return null;
    }
}