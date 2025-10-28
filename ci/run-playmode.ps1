# ci/run-playmode.ps1
# Launches Unity and executes CI.PlayModeCiRunner.Run (writes NUnit XML and enforces 0-tests fail).

$ErrorActionPreference = 'Stop'

# --- Paths
$projectRoot = (Get-Location).Path
$resultsDir  = Join-Path $projectRoot 'TestResults'
$xmlPath     = Join-Path $resultsDir 'PlayModeResults.xml'
$logPath     = Join-Path $resultsDir 'PlayModeLog.txt'

# --- Unity Editor
if (-not $env:UNITY_EDITOR -or -not (Test-Path $env:UNITY_EDITOR)) {
    throw 'UNITY_EDITOR env var not set or path not found. Example: D:/Program Files/Unity/Hub/Editor/2022.3.62f2/Editor/Unity.exe'
}

# --- Clean results
if (Test-Path $resultsDir) { Remove-Item -Recurse -Force $resultsDir }
New-Item -ItemType Directory -Force -Path $resultsDir | Out-Null

# --- Expose results path for the Editor runner
$env:CB_TEST_RESULTS_PATH = $xmlPath

Write-Host 'Running Unity PlayMode tests via CI.PlayModeCiRunner.Run...'
Write-Host ('Editor: {0}' -f $env:UNITY_EDITOR)
Write-Host ('Project: {0}' -f $projectRoot)
Write-Host ('XML: {0}' -f $xmlPath)
Write-Host ('Log: {0}' -f $logPath)

# --- Execute the custom Editor method (no -runTests)
& "$env:UNITY_EDITOR" `
  -projectPath "$projectRoot" `
  -batchmode -nographics `
  -executeMethod CI.PlayModeCiRunner.Run `
  -logFile "$logPath" `
  -quit

$unityExit = $LASTEXITCODE

# --- Validate results XML exists
if (-not (Test-Path $xmlPath)) {
    Write-Host ("[CI] PlayMode results XML not found at {0}" -f $xmlPath)
    Write-Host ("[CI] Unity Exit Code: {0} (failing due to missing XML)" -f $unityExit)
    exit 1
}

# --- Parse XML, enforce '0 tests = fail' (belt-and-suspenders; Editor script already enforces)
[xml]$xml = Get-Content $xmlPath
[int]$total = 0
$testRunNode = $xml.SelectSingleNode('//test-run')
if ($testRunNode -and $testRunNode.Attributes['total']) {
    $total = [int]$testRunNode.Attributes['total'].Value
} else {
    $cases = $xml.SelectNodes('//test-case'); if ($cases) { $total = $cases.Count }
}
if ($total -eq 0) {
    Write-Host '[CI] No PlayMode tests found (XML total==0).'
    exit 2
}

# --- Mirror Unity/editor exit (0 ok; 1 failure/crash; 2 zero-tests handled above)
exit $unityExit

