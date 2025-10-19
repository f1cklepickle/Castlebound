$ErrorActionPreference = "Stop"

function Step($m) { Write-Host "[CI] $m" }

# --- Unity location (change if needed)
$UnityExe = "D:/Program Files/Unity/Hub/Editor/2022.3.62f2/Editor/Unity.exe"
if (!(Test-Path $UnityExe)) { throw "Unity.exe not found at: $UnityExe" }
$env:UNITY_EDITOR = $UnityExe

# --- Workspace must be set BEFORE Join-Path
$ws = (Get-Location).Path
if (-not $ws) { throw "Workspace path could not be resolved." }
Step "Workspace : $ws"

# --- Prepare output folder
$resultsDir = Join-Path -Path $ws -ChildPath "TestResults"
Remove-Item -Path $resultsDir -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $resultsDir -Force | Out-Null

$xmlPath = Join-Path -Path $resultsDir -ChildPath "EditModeResults.xml"
$logPath = Join-Path -Path $resultsDir -ChildPath "EditModeLog.txt"
$env:CB_TEST_RESULTS_PATH = $xmlPath

Step "XML Path  : $xmlPath"
Step "Log Path  : $logPath"

# --- Run Unity headless
& $UnityExe -batchmode -nographics `
  -projectPath $ws `
  -executeMethod CI.EditModeCiRunner.Run `
  -logFile $logPath
$procExit = $LASTEXITCODE

# --- Wait for results (up to 90s) and ensure XML is non-empty
$deadline = (Get-Date).AddSeconds(90)
while ((Get-Date) -lt $deadline) {
  if (Test-Path $xmlPath) {
    try { if ((Get-Item $xmlPath).Length -gt 0) { break } } catch {}
  }
  Start-Sleep -Milliseconds 500
}

# --- Report and decide via NUnit XML (robust)
if (Test-Path $xmlPath) {
  [xml]$doc = Get-Content $xmlPath
  $total        = [int]$doc.'test-run'.total
  $passed       = [int]$doc.'test-run'.passed
  $failed       = [int]$doc.'test-run'.failed
  $inconclusive = [int]$doc.'test-run'.inconclusive

  Write-Host "`nNUnit summary: $passed / $total passed"
  $doc.'test-run'.'test-suite'.SelectNodes("//test-case") |
    ForEach-Object { "{0} - {1}" -f $_.name, $_.result }

  if ($failed -eq 0 -and $inconclusive -eq 0) {
    Write-Host "`nAll tests passed ✔"
    exit 0
  } else {
    Write-Error "`nSome tests failed or were inconclusive ✖"
    exit 1
  }
}
else {
  Write-Host "`nNo XML at $xmlPath. Log tail:"
  if (Test-Path $logPath) { Get-Content $logPath -Tail 200 } else { Write-Host "No log file at $logPath" }
  exit 1
}
