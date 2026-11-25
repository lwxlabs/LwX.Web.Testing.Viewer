using LwX.Web.Testing.Reports;

namespace LwX.Web.Testing.Viewer.Services;

public sealed class ArtifactIndexBuilder
{
    private static readonly Dictionary<string, string> ContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".png"] = "image/png",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".webp"] = "image/webp",
        [".gif"] = "image/gif",
        [".webm"] = "video/webm",
        [".mp4"] = "video/mp4",
        [".zip"] = "application/zip",
        [".har"] = "application/json",
        [".json"] = "application/json",
        [".log"] = "text/plain",
        [".txt"] = "text/plain"
    };

    public List<ViewerArtifactModel> FromReportArtifacts(string resultsDirectory, IEnumerable<LwXArtifactReference> artifacts)
    {
        return artifacts.Where(x => TryResolveArtifact(resultsDirectory, x.RelativePath, out _, out _)).Select(x => new ViewerArtifactModel
        {
            Kind = x.Kind,
            Name = string.IsNullOrWhiteSpace(x.Name) ? Path.GetFileName(x.RelativePath) : x.Name,
            RelativePath = Normalize(x.RelativePath),
            Url = ArtifactUrl(x.RelativePath),
            ContentType = x.ContentType ?? GuessContentType(x.RelativePath),
            SizeBytes = x.SizeBytes
        }).ToList();
    }

    public List<ViewerArtifactModel> ScanLooseArtifacts(string resultsDirectory)
    {
        if (!Directory.Exists(resultsDirectory)) return [];
        var files = Directory.EnumerateFiles(resultsDirectory, "*.*", new EnumerationOptions { RecurseSubdirectories = true, AttributesToSkip = FileAttributes.ReparsePoint, IgnoreInaccessible = true })
            .Where(x => !Path.GetFileName(x).Equals("run-summary.json", StringComparison.OrdinalIgnoreCase))
            .Where(x => TryResolveArtifact(resultsDirectory, Path.GetRelativePath(resultsDirectory, x), out _, out _))
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .Take(500)
            .ToList();

        return files.Select(file =>
        {
            var relative = Normalize(Path.GetRelativePath(resultsDirectory, file));
            var info = new FileInfo(file);
            return new ViewerArtifactModel
            {
                Kind = GuessKind(file),
                Name = Path.GetFileName(file),
                RelativePath = relative,
                Url = ArtifactUrl(relative),
                ContentType = GuessContentType(file),
                SizeBytes = info.Length
            };
        }).ToList();
    }

    public static bool TryResolveArtifact(string root, string relativePath, out string file, out string contentType)
    {
        file = string.Empty;
        contentType = string.Empty;
        if (string.IsNullOrWhiteSpace(relativePath)) return false;
        var segments = relativePath.Replace('\\', '/').Split('/');
        if (segments.Any(s => string.IsNullOrWhiteSpace(s) || s is "." or ".." || s.Contains(':'))) return false;
        if (!ContentTypes.TryGetValue(Path.GetExtension(segments[^1]), out var type)) return false;

        try
        {
            var current = Path.GetFullPath(root);
            foreach (var segment in segments)
            {
                current = Path.Combine(current, segment);
                if ((File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0) return false;
            }

            if (!File.Exists(current)) return false;
            file = current;
            contentType = type;
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            return false;
        }
    }

    private static string ArtifactUrl(string path) =>
        "/artifacts/" + string.Join("/", Normalize(path).Split('/').Select(Uri.EscapeDataString));

    private static string GuessKind(string file)
    {
        var ext = Path.GetExtension(file).ToLowerInvariant();
        return ext switch
        {
            ".png" or ".jpg" or ".jpeg" or ".webp" or ".gif" => "screenshot",
            ".webm" or ".mp4" => "video",
            ".zip" => "trace",
            ".har" => "har",
            ".log" or ".txt" => "log",
            ".json" => "json",
            _ => "file"
        };
    }

    private static string GuessContentType(string path)
    {
        var ext = Path.GetExtension(path);
        return ContentTypes.TryGetValue(ext, out var type) ? type : "application/octet-stream";
    }

    private static string Normalize(string path) => path.Replace('\\', '/').TrimStart('/');
}
