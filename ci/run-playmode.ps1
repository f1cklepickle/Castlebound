# ci/run-playmode.ps1
# Executes CI.PlayModeCiRunner.Run and ALWAYS targets the project's Unity version.

$ErrorActionPreference = 'Stop'

function Get-ProjectUnityVersion {
    $pv = Join-Path (Join-Path (Get-Location).Path 'ProjectSettings') 'ProjectVersion.txt'
    if (-not (Test-Path $pv)) { return $null }
    $txt = Get-Content $pv -Raw
    # Handles "m_EditorVersion: 2022.3.62f2" or "m_EditorVersionWithRevision: 2022.3.62f2 (xxxx)"
    if ($txt -match 'm_EditorVersion:\s*([0-9]+\.[0-9]+\.[0-9]+[a-z0-9]+)') { return $Matches[1] }
    if ($txt -match 'm_EditorVersionWithRevision:\s*([0-9]+\.[0-9]+\.[0-9]+[a-z0-9]+)') { return $Matches[1] }
    return $null
}

function Resolve-UnityEditor {
    $projVer = Get-ProjectUnityVersion

    # 0) Respect UNITY_EDITOR if explicitly set and valid
    if ($env:UNITY_EDITOR -and (Test-Path $env:UNITY_EDITOR)) { return $env:UNITY_EDITOR }

    # 1) Prefer exact ProjectVersion under Unity Hub (C: or D:)
    $hubRoots = @(
        'C:\Program Files\Unity\Hub\Editor',
        'D:\Program Files\Unity\Hub\Editor'
    )
    if ($projVer) {
        foreach ($root in $hubRoots) {
            $cand = Join-Path (Join-Path $root $projVer) 'Editor/Unity.exe'
            if (Test-Path $cand) { return $cand }
        }
    }

    # 2) Fallback: latest Hub editor if exact version not found
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

    # 3) Last resort: Unity.exe on PATH
    try {
        & 'Unity.exe' -batchmode -nographics -quit -version -logFile - | Out-Null
        if ($LASTEXITCODE -eq 0) { return 'Unity.exe' }
    } catch { }

    throw 'Unity editor not found. Install the project version via Unity Hub or set UNITY_EDITOR to the exact exe.'
}

# --- Paths
$projectRoot = (Get-Location).Path
$resultsDir  = Join-Path $projectRoot 'TestResults'
$xmlPath     = Join-Path $resultsDir 'PlayModeResults.xml'
$logPath     = Join-Path $resultsDir 'PlayModeLog.txt'

# --- Resolve Unity (project version)
$unity = Resolve-UnityEditor
Write-Host ("Using Unity Editor: {0}" -f $unity)

# --- Clean results
if (Test-Path $resultsDir) { Remove-Item -Recurse -Force $resultsDir }
New-Item -ItemType Directory -Force -Path $resultsDir | Out-Null

# --- Expose results path for the Editor runner (your C# runner reads this)
$env:CB_TEST_RESULTS_PATH = $xmlPath

Write-Host 'Running Unity PlayMode tests via CI.PlayModeCiRunner.Run...'
Write-Host ("Project: {0}" -f $projectRoot)
Write-Host ("XML: {0}" -f $xmlPath)
Write-Host ("Log: {0}" -f $logPath)

# --- Execute the custom Editor method (guarantees XML; PlayMode runner fails on 0 tests)
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

# --- Parse XML; enforce 0-tests = fail (belt & suspenders)
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

exit $unityExit
