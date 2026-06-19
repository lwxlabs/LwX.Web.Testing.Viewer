param(
    [string]$Results = ".\test-results-example"
)
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
dotnet run --project (Join-Path $root "src/LwX.Web.Testing.Viewer/LwX.Web.Testing.Viewer.csproj") -- --results $Results
