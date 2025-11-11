param()

$ErrorActionPreference = 'Stop'

$projectPath    = (Get-Location).Path
$testResultsDir = Join-Path $projectPath 'TestResults'
$xmlPath        = Join-Path $testResultsDir 'PlayModeResults.xml'
$logPath        = Join-Path $testResultsDir 'PlayModeLog.txt'

if (-not (Test-Path $testResultsDir)) {
    New-Item -ItemType Directory -Path $testResultsDir -Force | Out-Null
}

$unity = $env:UNITY_EDITOR
if (-not $unity -or -not (Test-Path $unity)) {
    $unity = 'D:\\Program Files\\Unity\\Hub\\Editor\\2022.3.62f2\\Editor\\Unity.exe'
}

Write-Host "Using Unity Editor: $unity"
Write-Host "Project: $projectPath"
Write-Host "XML (target): $xmlPath"
Write-Host "Log: $logPath"

if (Test-Path $xmlPath) { Remove-Item $xmlPath -Force }
if (Test-Path $logPath) { Remove-Item $logPath -Force }

$arguments = @(
    '-batchmode',
    '-nographics',
    '-projectPath', $projectPath,
    '-runTests',
    '-testPlatform', 'PlayMode',
    '-testResults', $xmlPath,
    '-logFile', $logPath
)

# --- Fully synchronous blocking call ---
Write-Host "[CI] Launching Unity in batchmode for PlayMode tests..."
$process = Start-Process -FilePath $unity -ArgumentList $arguments -Wait -PassThru
$unityExit = $process.ExitCode
Write-Host "[CI] Unity exited with code $unityExit"

# --- Give Unity time to finish file writes ---
for ($i = 0; $i -lt 20; $i++) {
    if (Test-Path $xmlPath) { break }
    Start-Sleep -Milliseconds 500
}

if (-not (Test-Path $xmlPath)) {
    Write-Host "[CI] ERROR: Unity did not produce a test results XML at $xmlPath"
    if (Test-Path $logPath) { Write-Host "[CI] See Unity log at $logPath for details." }
    exit 2
}

[xml]$xml = Get-Content $xmlPath

$total = 0
$node = $xml.SelectSingleNode('//test-run')
if ($node -and $node.Attributes['total']) {
    $total = [int]$node.Attributes['total'].Value
} else {
    $cases = $xml.SelectNodes('//test-case')
    if ($cases) { $total = $cases.Count }
}

Write-Host "[CI] NUnit XML total tests: $total"

if ($total -eq 0) {
    Write-Host "[CI] No PlayMode tests found (XML total==0). Failing lane."
    exit 2
}

exit $unityExit
