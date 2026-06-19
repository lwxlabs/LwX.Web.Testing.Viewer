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
        return artifacts.Select(x => new ViewerArtifactModel
        {
            Kind = x.Kind,
            Name = string.IsNullOrWhiteSpace(x.Name) ? Path.GetFileName(x.RelativePath) : x.Name,
            RelativePath = Normalize(x.RelativePath),
            Url = "/artifacts/" + Normalize(x.RelativePath),
            ContentType = x.ContentType ?? GuessContentType(x.RelativePath),
            SizeBytes = x.SizeBytes
        }).ToList();
    }

    public List<ViewerArtifactModel> ScanLooseArtifacts(string resultsDirectory)
    {
        if (!Directory.Exists(resultsDirectory)) return [];
        var files = Directory.EnumerateFiles(resultsDirectory, "*.*", SearchOption.AllDirectories)
            .Where(x => !Path.GetFileName(x).Equals("run-summary.json", StringComparison.OrdinalIgnoreCase))
            .Where(x => IsKnownArtifact(x))
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
                Url = "/artifacts/" + relative,
                ContentType = GuessContentType(file),
                SizeBytes = info.Length
            };
        }).ToList();
    }

    private static bool IsKnownArtifact(string file)
    {
        var ext = Path.GetExtension(file);
        return ContentTypes.ContainsKey(ext);
    }

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
