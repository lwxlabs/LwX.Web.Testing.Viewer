using LwX.Web.Testing.Viewer.Services;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

var resultsDirectory = ViewerArguments.GetResultsDirectory(args)
    ?? builder.Configuration["LwXViewer:ResultsDirectory"]
    ?? Environment.GetEnvironmentVariable("LWX_TEST_RESULTS")
    ?? Path.Combine(builder.Environment.ContentRootPath, "test-results");

resultsDirectory = Path.GetFullPath(resultsDirectory);
Directory.CreateDirectory(resultsDirectory);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddSingleton(new ViewerOptions(resultsDirectory));
builder.Services.AddSingleton<TestReportReader>();
builder.Services.AddSingleton<ArtifactIndexBuilder>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(resultsDirectory),
    RequestPath = "/artifacts",
    ServeUnknownFileTypes = true
});
app.UseAntiforgery();

app.MapGet("/api/results", async (TestReportReader reader) => await reader.LoadAsync());
app.MapGet("/api/results/refresh", async (TestReportReader reader) => await reader.LoadAsync(forceRefresh: true));
app.MapGet("/api/open-results-folder", (ViewerOptions options) => Results.Ok(new { options.ResultsDirectory }));

app.MapRazorComponents<LwX.Web.Testing.Viewer.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();

internal static class ViewerArguments
{
    public static string? GetResultsDirectory(string[] args)
    {
        for (var i = 0; i < args.Length; i++)
        {
            if ((args[i] == "--results" || args[i] == "-r") && i + 1 < args.Length) return args[i + 1];
            if (args[i].StartsWith("--results=", StringComparison.OrdinalIgnoreCase)) return args[i]["--results=".Length..];
        }
        return null;
    }
}
