using LwX.Web.Testing.Reports;

namespace LwX.Web.Testing.Viewer.Services;

public sealed class ViewerDashboardModel
{
    public string ResultsDirectory { get; set; } = string.Empty;
    public DateTimeOffset LoadedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public List<ViewerRunModel> Runs { get; set; } = [];
    public List<ViewerArtifactModel> LooseArtifacts { get; set; } = [];
    public int TotalTests => Runs.Sum(x => x.TotalTests);
    public int PassedTests => Runs.Sum(x => x.PassedTests);
    public int FailedTests => Runs.Sum(x => x.FailedTests);
    public int SkippedTests => Runs.Sum(x => x.SkippedTests);
    public int TotalArtifacts => Runs.Sum(x => x.Artifacts.Count + x.Tests.Sum(t => t.Artifacts.Count + t.Steps.Sum(s => s.Artifacts.Count))) + LooseArtifacts.Count;
}

public sealed class ViewerRunModel
{
    public string Id { get; set; } = string.Empty;
    public string ReportPath { get; set; } = string.Empty;
    public string RelativeReportPath { get; set; } = string.Empty;
    public LwXTestRunReport Report { get; set; } = new();
    public List<ViewerTestModel> Tests { get; set; } = [];
    public List<ViewerArtifactModel> Artifacts { get; set; } = [];
    public int TotalTests => Tests.Count;
    public int PassedTests => Tests.Count(x => x.Status.Equals("Passed", StringComparison.OrdinalIgnoreCase));
    public int FailedTests => Tests.Count(x => x.Status.Equals("Failed", StringComparison.OrdinalIgnoreCase));
    public int SkippedTests => Tests.Count(x => x.Status.Equals("Skipped", StringComparison.OrdinalIgnoreCase));
}

public sealed class ViewerTestModel
{
    public string RunId { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public long DurationMs { get; set; }
    public string? ErrorMessage { get; set; }
    public string? StackTrace { get; set; }
    public List<LwXTestStepReport> Steps { get; set; } = [];
    public List<ViewerArtifactModel> Artifacts { get; set; } = [];
    public List<LwXConsoleReportEntry> Console { get; set; } = [];
    public List<LwXNetworkReportEntry> Network { get; set; } = [];
    public List<LwXFrameworkReportEntry> FrameworkErrors { get; set; } = [];
}

public sealed class ViewerArtifactModel
{
    public string Kind { get; set; } = "file";
    public string Name { get; set; } = string.Empty;
    public string RelativePath { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public long? SizeBytes { get; set; }
    public bool IsImage => ContentType?.StartsWith("image/", StringComparison.OrdinalIgnoreCase) == true || RelativePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase) || RelativePath.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || RelativePath.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) || RelativePath.EndsWith(".webp", StringComparison.OrdinalIgnoreCase);
    public bool IsVideo => ContentType?.StartsWith("video/", StringComparison.OrdinalIgnoreCase) == true || RelativePath.EndsWith(".webm", StringComparison.OrdinalIgnoreCase) || RelativePath.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase);
    public bool IsTrace => RelativePath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase);
    public bool IsHar => RelativePath.EndsWith(".har", StringComparison.OrdinalIgnoreCase);
    public bool IsLog => RelativePath.EndsWith(".log", StringComparison.OrdinalIgnoreCase) || RelativePath.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) || RelativePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase);
}
