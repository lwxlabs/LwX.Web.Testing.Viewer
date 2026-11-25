using LwX.Web.Testing.Viewer.Services;
using System.Net;
using Microsoft.AspNetCore.StaticFiles;

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

app.Use(async (context, next) =>
{
    var remote = context.Connection.RemoteIpAddress;
    if (remote is null || !IPAddress.IsLoopback(remote.IsIPv4MappedToIPv6 ? remote.MapToIPv4() : remote) ||
        !(context.Request.Host.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
          (IPAddress.TryParse(context.Request.Host.Host.Trim('[', ']'), out var host) && IPAddress.IsLoopback(host))))
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return;
    }

    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    await next();
});

app.UseStaticFiles();
app.MapGet("/artifacts/{**path}", (string path, ViewerOptions options) =>
{
    if (!ArtifactIndexBuilder.TryResolveArtifact(options.ResultsDirectory, path, out var file, out var type))
        return Results.NotFound();

    return Results.File(file, type, enableRangeProcessing: true);
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
