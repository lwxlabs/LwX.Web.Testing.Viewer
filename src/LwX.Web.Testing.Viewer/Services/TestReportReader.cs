using System.Text.Json;
using LwX.Web.Testing.Reports;

namespace LwX.Web.Testing.Viewer.Services;

public sealed class TestReportReader
{
    private readonly ViewerOptions _options;
    private readonly ArtifactIndexBuilder _artifacts;
    private ViewerDashboardModel? _cache;
    private readonly JsonSerializerOptions _json = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public TestReportReader(ViewerOptions options, ArtifactIndexBuilder artifacts)
    {
        _options = options;
        _artifacts = artifacts;
    }

    public async Task<ViewerDashboardModel> LoadAsync(bool forceRefresh = false, CancellationToken cancellationToken = default)
    {
        if (!forceRefresh && _cache is not null) return _cache;

        var model = new ViewerDashboardModel { ResultsDirectory = _options.ResultsDirectory };
        Directory.CreateDirectory(_options.ResultsDirectory);

        var reports = Directory.EnumerateFiles(_options.ResultsDirectory, "run-summary.json", SearchOption.AllDirectories)
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .ToList();

        foreach (var reportFile in reports)
        {
            try
            {
                var json = await File.ReadAllTextAsync(reportFile, cancellationToken);
                var report = JsonSerializer.Deserialize<LwXTestRunReport>(json, _json);
                if (report is null) continue;
                var run = CreateRunModel(reportFile, report);
                model.Runs.Add(run);
            }
            catch
            {
                // Corrupt reports are ignored so the viewer can still open other results.
            }
        }

        model.LooseArtifacts = _artifacts.ScanLooseArtifacts(_options.ResultsDirectory);
        _cache = model;
        return model;
    }

    private ViewerRunModel CreateRunModel(string reportFile, LwXTestRunReport report)
    {
        var run = new ViewerRunModel
        {
            Id = string.IsNullOrWhiteSpace(report.RunId) ? Path.GetFileName(Path.GetDirectoryName(reportFile)) ?? Guid.NewGuid().ToString("N") : report.RunId,
            ReportPath = reportFile,
            RelativeReportPath = Path.GetRelativePath(_options.ResultsDirectory, reportFile).Replace('\\', '/'),
            Report = report,
            Artifacts = _artifacts.FromReportArtifacts(_options.ResultsDirectory, report.Artifacts)
        };

        foreach (var test in report.Tests)
        {
            run.Tests.Add(new ViewerTestModel
            {
                RunId = run.Id,
                Id = string.IsNullOrWhiteSpace(test.Id) ? Guid.NewGuid().ToString("N") : test.Id,
                Name = test.Name,
                Status = test.Status,
                DurationMs = test.DurationMs,
                ErrorMessage = test.ErrorMessage,
                StackTrace = test.StackTrace,
                Steps = test.Steps,
                Artifacts = _artifacts.FromReportArtifacts(_options.ResultsDirectory, test.Artifacts),
                Console = test.Console,
                Network = test.Network,
                FrameworkErrors = test.FrameworkErrors
            });
        }

        return run;
    }
}
