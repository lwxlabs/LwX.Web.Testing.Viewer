// Report schema 1.0 shared with LwX.Web.Testing (MIT).
using System.Text.Json.Serialization;

namespace LwX.Web.Testing.Reports;

public sealed class LwXTestRunReport
{
    public string SchemaVersion { get; set; } = "1.0";
    public string RunId { get; set; } = $"lwx-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmssfff}";
    public DateTimeOffset StartedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? FinishedAtUtc { get; set; }
    public string Framework { get; set; } = "Generic";
    public string Browser { get; set; } = "Chromium";
    public string Profile { get; set; } = "Local";
    public string BaseUrl { get; set; } = string.Empty;
    public string MachineName { get; set; } = Environment.MachineName;
    public string? OperatingSystem { get; set; } = Environment.OSVersion.ToString();
    public string? DotNetVersion { get; set; } = Environment.Version.ToString();
    public Dictionary<string, string> Metadata { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<LwXTestCaseReport> Tests { get; set; } = [];
    public List<LwXArtifactReference> Artifacts { get; set; } = [];
    public List<LwXConsoleReportEntry> Console { get; set; } = [];
    public List<LwXNetworkReportEntry> Network { get; set; } = [];
    public List<LwXFrameworkReportEntry> FrameworkErrors { get; set; } = [];

    [JsonIgnore]
    public TimeSpan? Duration => FinishedAtUtc is null ? null : FinishedAtUtc.Value - StartedAtUtc;
}

public sealed class LwXTestCaseReport
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "Unnamed test";
    public string ClassName { get; set; } = string.Empty;
    public string Status { get; set; } = "Running";
    public DateTimeOffset StartedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? FinishedAtUtc { get; set; }
    public long DurationMs { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorType { get; set; }
    public string? StackTrace { get; set; }
    public List<LwXTestStepReport> Steps { get; set; } = [];
    public List<LwXArtifactReference> Artifacts { get; set; } = [];
    public List<LwXConsoleReportEntry> Console { get; set; } = [];
    public List<LwXNetworkReportEntry> Network { get; set; } = [];
    public List<LwXFrameworkReportEntry> FrameworkErrors { get; set; } = [];
}

public sealed class LwXTestStepReport
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = "Running";
    public DateTimeOffset StartedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? FinishedAtUtc { get; set; }
    public long DurationMs { get; set; }
    public string? ErrorMessage { get; set; }
    public List<LwXArtifactReference> Artifacts { get; set; } = [];
}

public sealed class LwXArtifactReference
{
    public string Kind { get; set; } = "file";
    public string Name { get; set; } = string.Empty;
    public string RelativePath { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public long? SizeBytes { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class LwXConsoleReportEntry
{
    public DateTimeOffset TimestampUtc { get; set; } = DateTimeOffset.UtcNow;
    public string Type { get; set; } = "log";
    public string Text { get; set; } = string.Empty;
    public string? Location { get; set; }
}

public sealed class LwXNetworkReportEntry
{
    public DateTimeOffset TimestampUtc { get; set; } = DateTimeOffset.UtcNow;
    public string Method { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public int? StatusCode { get; set; }
    public string? Failure { get; set; }
    public bool Failed => StatusCode >= 400 || !string.IsNullOrWhiteSpace(Failure);
}

public sealed class LwXFrameworkReportEntry
{
    public DateTimeOffset TimestampUtc { get; set; } = DateTimeOffset.UtcNow;
    public string Framework { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = "error";
}
