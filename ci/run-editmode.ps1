$ErrorActionPreference = "Stop"

function Step($msg) { Write-Host "[CI] $msg" }

# 1) Locate Unity Editor (use env var if set; otherwise fallback)
if ([string]::IsNullOrWhiteSpace($env:UNITY_EDITOR)) {
  $env:UNITY_EDITOR = "D:\Program Files\Unity\Hub\Editor\2022.3.62f2\Editor\Unity.exe"
}
if (-not (Test-Path $env:UNITY_EDITOR)) {
  throw "UNITY_EDITOR not found at '$env:UNITY_EDITOR'. Set UNITY_EDITOR or update the fallback path."
}

# 2) Workspace + output paths
$ws       = (Get-Location).Path
$results  = Join-Path $ws "TestResults"
$xmlPath  = Join-Path $results "EditModeResults.xml"
$logPath  = Join-Path $results "EditModeLog.txt"

# Clean a fresh results dir
Remove-Item -Recurse -Force $results -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path $results | Out-Null

# Tell the in-Editor runner where to write the NUnit XML
$env:CB_TEST_RESULTS_PATH = $xmlPath

Step "Workspace : $ws"
Step "XML Path  : $xmlPath"
Step "Log Path  : $logPath"

# 3) Run Unity headless via our Editor entrypoint
& "$env:UNITY_EDITOR" -batchmode -nographics `
  -projectPath $ws `
  -executeMethod CI.EditModeCiRunner.Run `
  -logFile $logPath
$unityCode = $LASTEXITCODE

# 4) Wait for NUnit XML (up to 10 minutes) or until Unity exits & flushes files
$deadline = (Get-Date).AddMinutes(10)
function IsUnityBusy {
  $procs = Get-Process -ErrorAction SilentlyContinue | Where-Object { $_.ProcessName -match '^Unity$' }
  return ($procs -ne $null -and $procs.Count -gt 0)
}
do {
  if (Test-Path $xmlPath) {
    $len = (Get-Item $xmlPath).Length
    if ($len -gt 0) { break }
  }
  Start-Sleep -Milliseconds 500
} while ((Get-Date) -lt $deadline -and (IsUnityBusy))

# 5) Report & exit code
if (Test-Path $xmlPath) {
  [xml]$doc = Get-Content $xmlPath
  $total  = [int]$doc.'test-run'.total
  $passed = [int]$doc.'test-run'.passed
  Write-Host ""
  Write-Host "NUnit summary: $passed / $total passed"
  $doc.'test-run'.'test-suite'.SelectNodes("//test-case") | ForEach-Object {
    "{0} - {1}" -f $_.name, $_.result
  }
  if ($passed -eq $total) {
    Write-Host "`nAll tests passed ✔"
    exit 0
  } else {
    Write-Error "`nSome tests failed ✖"
    exit 1
  }
} else {
  Write-Error "`nNo XML at $xmlPath. Log tail:"
  if (Test-Path $logPath) { Get-Content $logPath -Tail 500 } else { Write-Host "No log file at $logPath" }
  exit 1
}

