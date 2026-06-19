# LwX.Web.Testing Viewer

`LwX.Web.Testing.Viewer` is a **separate optional package/project**. It is a local visual dashboard for Playwright runs created by `LwX.Web.Testing`. The core test package does not need to reference the viewer.

It reads structured `run-summary.json` reports and also scans loose artifacts when older tests did not generate a report.


## Package separation

Use `LwX.Web.Testing` in test projects. Use `LwX.Web.Testing.Viewer` only when you want to open a visual dashboard.

```txt
LwX.Web.Testing          = core test engine / report writer
LwX.Web.Testing.Viewer   = optional report/artifact viewer
```

Install/run as tool after publishing:

```powershell
dotnet tool install --global LwX.Web.Testing.Viewer --prerelease
lwx-test-viewer --results ./test-results
```

## Start the viewer

```powershell
dotnet run --project ./src/LwX.Web.Testing.Viewer -- --results ./test-results
```

or set an environment variable:

```powershell
$env:LWX_TEST_RESULTS = "C:/projects/my-app/test-results"
dotnet run --project ./src/LwX.Web.Testing.Viewer
```

## Enable report generation

```csharp
protected override void Configure(LwXWebTestingOptions options)
{
    options.BaseUrl = "http://localhost:5173";
    options.UseReact();
    options.UseProfile(LwXTestProfile.CI);

    options.Report.Enabled = true;
    options.Report.OutputDirectory = "test-results";
    options.Report.FileName = "run-summary.json";

    options.Artifacts.ScreenshotOnFailure = true;
    options.Artifacts.TraceOnFailure = true;
    options.Artifacts.VideoOnFailure = true;
    options.Artifacts.HarOnFailure = true;
    options.Logging.WriteToFile = true;
}
```

## Named visual steps

```csharp
[Fact]
public async Task Editor_Should_Save()
{
    await RunSafelyAsync(async () =>
    {
        await StepAsync("Open editor", () => GoToAsync("/editor"));
        await StepAsync("Fill title", () => Page.FillByTestIdAsync("title", "Demo"));
        await StepAsync("Save", () => Page.SaveShortcutAsync());
    }, "Editor_Should_Save");
}
```

## What it shows

- run summary
- passed / failed / skipped tests
- screenshots
- step screenshots
- videos
- traces
- HAR files
- logs
- browser console messages
- JavaScript page errors
- failed network requests
- framework errors for Blazor, Angular, React, Vue, Svelte, Solid, Astro, plain HTML, and others
- loose artifacts from old runs without reports

## Theme

The viewer has dark, light and auto modes. Dark is the default visual style.
