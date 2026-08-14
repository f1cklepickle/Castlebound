$ErrorActionPreference = "Stop"

function Step($message) { Write-Host "[CI] $message" }

if ([string]::IsNullOrWhiteSpace($env:UNITY_EDITOR)) {
  $env:UNITY_EDITOR = "D:\Program Files\Unity\Hub\Editor\2022.3.62f2\Editor\Unity.exe"
}
if (-not (Test-Path -LiteralPath $env:UNITY_EDITOR)) {
  throw "UNITY_EDITOR not found at '$env:UNITY_EDITOR'. Set UNITY_EDITOR or update the fallback path."
}

$workspace = [IO.Path]::GetFullPath((Get-Location).Path)
$buildRoot = [IO.Path]::GetFullPath((Join-Path $workspace "build\Windows"))
$outputDirectory = Join-Path $buildRoot "Castlebound"
$executablePath = Join-Path $outputDirectory "Castlebound.exe"
$dataDirectory = Join-Path $outputDirectory "Castlebound_Data"
$logPath = Join-Path $workspace "build\WindowsBuildLog.txt"
$workspacePrefix = $workspace.TrimEnd('\') + '\'

if (-not $buildRoot.StartsWith($workspacePrefix, [StringComparison]::OrdinalIgnoreCase)) {
  throw "Refusing to clean Windows build path outside workspace: '$buildRoot'"
}

if (Test-Path -LiteralPath $buildRoot) {
  Remove-Item -LiteralPath $buildRoot -Recurse -Force
}
New-Item -ItemType Directory -Force -Path $outputDirectory | Out-Null
if (Test-Path -LiteralPath $logPath) {
  Remove-Item -LiteralPath $logPath -Force
}

$env:CB_WINDOWS_BUILD_DIR = $outputDirectory

Step "Workspace : $workspace"
Step "Output    : $outputDirectory"
Step "Log Path  : $logPath"
Step "Unity     : $env:UNITY_EDITOR"

$arguments = @(
  '-batchmode', '-nographics',
  '-buildTarget', 'StandaloneWindows64',
  '-projectPath', $workspace,
  '-executeMethod', 'CI.WindowsCiBuildRunner.Run',
  '-logFile', $logPath
)

$scriptAssemblies = Join-Path $workspace "Library\ScriptAssemblies"
if (Test-Path -LiteralPath $scriptAssemblies) {
  Remove-Item -LiteralPath $scriptAssemblies -Recurse -Force
  Step "Cleared Library/ScriptAssemblies to force script recompile from source"
}

$artifactDatabase = Join-Path $workspace "Library\ArtifactDB"
if (Test-Path -LiteralPath $artifactDatabase) {
  Remove-Item -LiteralPath $artifactDatabase -Force
  Step "Deleted Library/ArtifactDB to force Library rebuild"
}

Step "Launching Unity..."
$process = Start-Process -FilePath $env:UNITY_EDITOR -ArgumentList $arguments -Wait -PassThru -NoNewWindow
$unityCode = $process.ExitCode
Step "Unity exited with code: $unityCode"

if ($unityCode -ne 0) {
  Write-Error "Unity Windows build failed with exit code $unityCode"
  if (Test-Path -LiteralPath $logPath) { Get-Content -LiteralPath $logPath -Tail 300 }
  exit $unityCode
}

if (-not (Test-Path -LiteralPath $executablePath -PathType Leaf)) {
  Write-Error "Expected Windows executable not found at '$executablePath'"
  if (Test-Path -LiteralPath $logPath) { Get-Content -LiteralPath $logPath -Tail 300 }
  exit 2
}

if (-not (Test-Path -LiteralPath $dataDirectory -PathType Container)) {
  Write-Error "Expected Windows data directory not found at '$dataDirectory'"
  if (Test-Path -LiteralPath $logPath) { Get-Content -LiteralPath $logPath -Tail 300 }
  exit 3
}

Step "Windows player build completed successfully."
exit 0
