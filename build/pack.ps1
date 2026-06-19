$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root "src/LwX.Web.Testing.Viewer/LwX.Web.Testing.Viewer.csproj"
$out = Join-Path $root "artifacts/packages"
New-Item -ItemType Directory -Force -Path $out | Out-Null
dotnet pack $project -c Release -o $out
