# LwX.Web.Testing.Viewer

Local Blazor dashboard for [LwX.Web.Testing](https://github.com/lwxlabs/LwX.Web.Testing) reports and Playwright artifacts.

- Test results, steps, console and network diagnostics
- Screenshots, videos, traces, HAR and logs
- Structured JSON reports and loose artifacts

## Quick start

Requires the .NET 10 SDK (the viewer targets .NET 8).

```bash
dotnet run --project src/LwX.Web.Testing.Viewer --no-launch-profile -- --results ./test-results-example
```

Open the localhost URL printed by the app. Replace the results path with your own test output.

## Local tool

```powershell
pwsh build/pack.ps1
dotnet tool install --global LwX.Web.Testing.Viewer --version 1.0.0-preview.1 --add-source ./artifacts/packages
lwx-test-viewer --results ./test-results-example
```

The viewer is standalone and reads report schema 1.0. It is intended for local use only; reports and artifacts may contain sensitive test data.

MIT license.
