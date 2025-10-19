# ci/run-editmode.ps1
$ErrorActionPreference = "Stop"

### 1) Locate Unity Editor (use env var if set; otherwise fallback) ############################
if ([string]::IsNullOrWhiteSpace($env:UNITY_EDITOR)) {
  # <-- If your local editor lives elsewhere, change this path.
  $env:UNITY_EDITOR = "D:/Program Files/Unity/Hub/Editor/2022.3.62f2/Editor/Unity.exe"
}
if (-not (Test-Path $env:UNITY_EDITOR)) {
  throw "UNITY_EDITOR not found at '$env:UNITY_EDITOR'. Set UNITY_EDITOR or update the fallback path."
}

### 2) Workspace + output paths ################################################################
$ws         = (Get-Location).Path
$resultsDir = Join-Path $ws "TestResults"
$xmlPath    = Join-Path $resultsDir "EditModeResults.xml"
$logPath    = Join-Path $resultsDir "EditModeLog.txt"

# Clean & recreate results dir
Remove-Item -Recurse -Force $resultsDir -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path $resultsDir | Out-Null

# Tell the in-Editor runner where to write the NUnit XML
$env:CB_TEST_RESULTS_PATH = $xmlPath

Write-Host "[CI] Workspace : $ws"
Write-Host "[CI] XML Path  : $xmlPath"
Write-Host "[CI] Log Path  : $logPath"

### 3) Run Unity headless, via our Editor entrypoint ###########################################
& "$env:UNITY_EDITOR" -batchmode -nographics `
  -projectPath $ws `
  -executeMethod CI.EditModeCiRunner.Run `
  -logFile $logPath
  
# Give Unity a moment to flush files
$deadline = (Get-Date).AddSeconds(30)
while ((Get-Date) -lt $deadline -and !(Test-Path $xmlPath)) { Start-Sleep -Milliseconds 250 }

### 4) Report & exit code #######################################################################
if (Test-Path $xmlPath) {
  [xml]$doc = Get-Content $xmlPath
  $total  = [int]$doc.'test-run'.total
  $passed = [int]$doc.'test-run'.passed
  $failed = [int]$doc.'test-run'.failed

  Write-Host "`nNUnit summary: $passed / $total passed"
  $doc.'test-run'.'test-suite'.SelectNodes("//test-case") |
    ForEach-Object { "{0} - {1}" -f $_.name, $_.result }

  if ($failed -gt 0) {
    Write-Error "`nSome tests failed ✖"
    exit 1
  } else {
    Write-Host "`nAll tests passed ✔"
    exit 0

  }
}
else {
  Write-Error "`nNo XML at $xmlPath. Log tail:"
  if (Test-Path $logPath) {
    Get-Content $logPath -Tail 400
  } else {
    Write-Host "No log file at $logPath"
  }
  exit 1
}
