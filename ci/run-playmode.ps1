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

# --- Expose results path for CI (parity with EditMode)
$env:CB_TEST_RESULTS_PATH = $xmlPath

Write-Host 'Running Unity PlayMode tests...'
Write-Host ('Editor: {0}' -f $env:UNITY_EDITOR)
Write-Host ('Project: {0}' -f $projectRoot)
Write-Host ('XML: {0}' -f $xmlPath)
Write-Host ('Log: {0}' -f $logPath)

# --- Run Unity once
& "$env:UNITY_EDITOR" `
  -runTests `
  -projectPath "$projectRoot" `
  -testPlatform playmode `
  -testResults "$xmlPath" `
  -logFile "$logPath" `
  -batchmode -nographics -quit

$unityExit = $LASTEXITCODE

# --- If results XML is missing, check the log for "no tests" fingerprints.
$noTestsPatterns = @(
  'No tests found',
  'No tests were found',
  'Skipping execution\.\? No tests',
  'Test run finished.*Total:\s*0',
  'there were no tests to run'
)

function LogIndicatesNoTests {
    param([string]$path)
    if (-not (Test-Path $path)) { return $false }
    $text = Get-Content $path -Raw
    foreach ($p in $noTestsPatterns) {
        if ($text -match $p) { return $true }
    }
    return $false
}

if (-not (Test-Path $xmlPath)) {
    if (LogIndicatesNoTests -path $logPath) {
        Write-Host 'No PlayMode tests found (detected via log; Unity emitted no XML).'
        exit 2
    } else {
        Write-Host ("PlayMode results XML not found at {0}" -f $xmlPath)
        Write-Host ("Unity Exit Code: {0} (failing due to missing XML)" -f $unityExit)
        exit 1
    }
}

# --- Parse XML, enforce '0 tests = fail'
[xml]$xml = Get-Content $xmlPath

[int]$total = 0
$testRunNode = $xml.SelectSingleNode('//test-run')
if ($testRunNode -and $testRunNode.Attributes['total']) {
    $total = [int]$testRunNode.Attributes['total'].Value
} else {
    $cases = $xml.SelectNodes('//test-case')
    if ($cases) { $total = $cases.Count }
}

if ($total -eq 0) {
    Write-Host 'No PlayMode tests found! Ensure you have at least one test in Assets/_Project/_Tests/PlayMode/.'
    exit 2
}

# --- Mirror Unity result (non-zero means failing tests or crash)
exit $unityExit
