using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.ThemeSongs.Services;

public sealed class FileSystemService
{
    private readonly ILogger<FileSystemService> _logger;

    public FileSystemService(ILogger<FileSystemService> logger) => _logger = logger;

    /// <summary>
    /// Schreibt eine Datei zuerst in tempPath und verschiebt sie anschließend atomar nach finalPath (gleicher Ordner empfohlen).
    /// </summary>
    public async Task<bool> WriteAtomicallyAsync(Stream source, string tempPath, string finalPath, CancellationToken ct)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        if (string.IsNullOrWhiteSpace(tempPath)) throw new ArgumentException("Temp path must not be empty.", nameof(tempPath));
        if (string.IsNullOrWhiteSpace(finalPath)) throw new ArgumentException("Final path must not be empty.", nameof(finalPath));

        try
        {
            var finalDir = Path.GetDirectoryName(finalPath);
            if (!string.IsNullOrEmpty(finalDir))
            {
                Directory.CreateDirectory(finalDir);
            }

            // Alte Temp-Datei bereinigen, falls von einem vorherigen Lauf übrig.
            TryDelete(tempPath);

            await using (var fs = new FileStream(
                tempPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 64 * 1024,
                useAsync: true))
            {
                await source.CopyToAsync(fs, ct).ConfigureAwait(false);
            }

            // Atomar ersetzen, wenn temp und final im selben Verzeichnis liegen.
            // File.Move(..., overwrite: true) vermeidet den vorherigen Delete-Branch.
            File.Move(tempPath, finalPath, overwrite: true);
            return true;
        }
        catch (OperationCanceledException)
        {
            // Bei Abbruch temporäre Datei aufräumen und Cancellation korrekt propagieren.
            TryDelete(tempPath);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Schreiben: {Path}", finalPath);
            TryDelete(tempPath);
            return false;
        }
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
            // Best effort cleanup: Fehler hier sind nicht kritisch.
        }
    }
}