# ci/run-playmode.ps1
# Launches Unity and executes CI.PlayModeCiRunner.Run (writes NUnit XML and enforces 0-tests fail).
# Robustly resolves Unity editor path so $env:UNITY_EDITOR is optional.

$ErrorActionPreference = 'Stop'

function Resolve-UnityEditor {
    # 1) Respect UNITY_EDITOR if valid
    if ($env:UNITY_EDITOR -and (Test-Path $env:UNITY_EDITOR)) { return $env:UNITY_EDITOR }

    # 2) Probe typical Unity Hub locations (latest first)
    $hubRoots = @(
        'C:\Program Files\Unity\Hub\Editor',
        'D:\Program Files\Unity\Hub\Editor'
    )
    foreach ($root in $hubRoots) {
        if (Test-Path $root) {
            $cand = Get-ChildItem -Path $root -Directory -ErrorAction SilentlyContinue |
                    Sort-Object Name -Descending |
                    ForEach-Object { Join-Path $_.FullName 'Editor/Unity.exe' } |
                    Where-Object { Test-Path $_ } |
                    Select-Object -First 1
            if ($cand) { return $cand }
        }
    }

    # 3) Fall back to Unity.exe on PATH
    try {
        & 'Unity.exe' -batchmode -nographics -quit -version -logFile - | Out-Null
        if ($LASTEXITCODE -eq 0) { return 'Unity.exe' }
    } catch { }

    throw 'Unity editor not found. Set UNITY_EDITOR or install a Hub editor on this runner.'
}

# --- Paths
$projectRoot = (Get-Location).Path
$resultsDir  = Join-Path $projectRoot 'TestResults'
$xmlPath     = Join-Path $resultsDir 'PlayModeResults.xml'
$logPath     = Join-Path $resultsDir 'PlayModeLog.txt'

# --- Resolve Unity
$unity = Resolve-UnityEditor

# --- Clean results
if (Test-Path $resultsDir) { Remove-Item -Recurse -Force $resultsDir }
New-Item -ItemType Directory -Force -Path $resultsDir | Out-Null

# --- Expose results path for the Editor runner (your runners read this)
$env:CB_TEST_RESULTS_PATH = $xmlPath

Write-Host 'Running Unity PlayMode tests via CI.PlayModeCiRunner.Run...'
Write-Host ("Editor: {0}" -f $unity)
Write-Host ("Project: {0}" -f $projectRoot)
Write-Host ("XML: {0}" -f $xmlPath)
Write-Host ("Log: {0}" -f $logPath)

# --- Execute the custom Editor method (not -runTests)
& "$unity" `
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

# --- Parse XML, enforce '0 tests = fail' (belt & suspenders; PlayModeCiRunner already does this)
[xml]$xml = Get-Content $xmlPath
[int]$total = 0
$node = $xml.SelectSingleNode('//test-run')
if ($node -and $node.Attributes['total']) { $total = [int]$node.Attributes['total'].Value }
else {
    $cases = $xml.SelectNodes('//test-case'); if ($cases) { $total = $cases.Count }
}
if ($total -eq 0) {
    Write-Host '[CI] No PlayMode tests found (XML total==0).'
    exit 2
}

# --- Mirror Unity/editor exit (0 ok; 1 failure/crash; 2 zero-tests handled above)
exit $unityExit

