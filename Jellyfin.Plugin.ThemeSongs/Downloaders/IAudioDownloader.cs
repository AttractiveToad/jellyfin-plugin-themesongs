using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Jellyfin.Plugin.ThemeSongs.Downloaders;

public interface IAudioDownloader
{
    Task<Stream?> DownloadByQueryAsync(string query, string preferredFormat, CancellationToken ct);
}