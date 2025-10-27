$ErrorActionPreference = "Stop"

# --- Paths
$projectRoot = (Get-Location).Path
$resultsDir  = Join-Path $projectRoot "TestResults"
$xmlPath     = Join-Path $resultsDir "PlayModeResults.xml"
$logPath     = Join-Path $resultsDir "PlayModeLog.txt"

# --- Unity Editor
if (-not $env:UNITY_EDITOR -or -not (Test-Path $env:UNITY_EDITOR)) {
    throw "UNITY_EDITOR env var not set or path not found. Example: `D:/Program Files/Unity/Hub/Editor/2022.3.62f2/Editor/Unity.exe`"
}

# --- Clean results
if (Test-Path $resultsDir) { Remove-Item -Recurse -Force $resultsDir }
New-Item -ItemType Directory -Force -Path $resultsDir | Out-Null

# --- Expose results path for CI (optional parity with EditMode script)
$env:CB_TEST_RESULTS_PATH = $xmlPath

Write-Host "Running Unity PlayMode tests..."
Write-Host "Editor: $env:UNITY_EDITOR"
Write-Host "Project: $projectRoot"
Write-Host "XML: $xmlPath"
Write-Host "Log: $logPath"

# --- Run Unity once
& "$env:UNITY_EDITOR" `
  -runTests `
  -projectPath "$projectRoot" `
  -testPlatform playmode `
  -testResults "$xmlPath" `
  -logFile "$logPath" `
  -batchmode -nographics -quit

$unityExit = $LASTEXITCODE

# --- Validate results XML exists
if (-not (Test-Path $xmlPath)) {
    Write-Host "PlayMode results XML not found at $xmlPath"
    Write-Host "Unity Exit Code: $unityExit (will be ignored here; failing due to missing XML)"
    exit 1
}

# --- Parse XML, enforce '0 tests = fail'
[xml]$xml = Get-Content $xmlPath

# Unity's NUnit XML can vary; try robust detection:
# 1) NUnit3: <test-run total="N" failed="F" ...>
# 2) Fallback: count test-case nodes
$total = $null
$testRunNode = $xml.SelectSingleNode("//test-run")
if ($testRunNode -and $testRunNode.Attributes["total"]) {
    $total = [int]$testRunNode.Attributes["total"].Value
} else {
    $cases = $xml.SelectNodes("//test-case")
    if ($cases) { $total = $cases.Count } else { $total = 0 }
}

if ($total -eq 0) {
    Write-Host "No PlayMode tests found! Ensure you have at least one test in Assets/_Project/_Tests/PlayMode/."
    exit 2
}

# --- Mirror Unity result (non-zero means failing tests or crash)
exit $unityExit

