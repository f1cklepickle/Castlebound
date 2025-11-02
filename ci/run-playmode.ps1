# ci/run-playmode.ps1 — seed XML, run Unity, then fail only when total==0
$ErrorActionPreference = 'Stop'

function Get-ProjectUnityVersion {
  $pv = Join-Path (Join-Path (Get-Location).Path 'ProjectSettings') 'ProjectVersion.txt'
  if (-not (Test-Path $pv)) { return $null }
  $txt = Get-Content $pv -Raw
  if ($txt -match 'm_EditorVersion:\s*([0-9]+\.[0-9]+\.[0-9]+[a-z0-9]+)') { return $Matches[1] }
  if ($txt -match 'm_EditorVersionWithRevision:\s*([0-9]+\.[0-9]+\.[0-9]+[a-z0-9]+)') { return $Matches[1] }
  return $null
}

function Resolve-UnityEditor {
  $projVer = Get-ProjectUnityVersion
  if ($env:UNITY_EDITOR -and (Test-Path $env:UNITY_EDITOR)) { return $env:UNITY_EDITOR }

  $hubRoots = @('C:\Program Files\Unity\Hub\Editor','D:\Program Files\Unity\Hub\Editor')
  if ($projVer) {
    foreach ($root in $hubRoots) {
      $cand = Join-Path (Join-Path $root $projVer) 'Editor/Unity.exe'
      if (Test-Path $cand) { return $cand }
    }
  }

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

  try { & 'Unity.exe' -batchmode -nographics -quit -version -logFile - | Out-Null; if ($LASTEXITCODE -eq 0) { return 'Unity.exe' } } catch { }
  throw 'Unity editor not found. Install the project version via Unity Hub or set UNITY_EDITOR to the exact exe.'
}

$projectRoot = (Get-Location).Path
$resultsDir  = Join-Path $projectRoot 'TestResults'
$xmlPath     = Join-Path $resultsDir 'PlayModeResults.xml'
$logPath     = Join-Path $resultsDir 'PlayModeLog.txt'

$unity = Resolve-UnityEditor
Write-Host "Using Unity Editor: $unity"
Write-Host "Project: $projectRoot"

if (-not (Test-Path $resultsDir)) { New-Item -ItemType Directory -Force -Path $resultsDir | Out-Null }

$now = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ss")
$seed = @"
<?xml version="1.0" encoding="utf-8"?>
<test-run id="2" testcasecount="0" result="Inconclusive" total="0" passed="0" failed="0" inconclusive="0" skipped="0" asserts="0"
          start-time="$now" end-time="$now" duration="0">
  <command-line></command-line>
  <test-suite type="Assembly" name="PlayMode" executed="True" result="Inconclusive" time="0" asserts="0">
  </test-suite>
</test-run>
"@
$seed | Set-Content -Path $xmlPath -Encoding UTF8

Write-Host "XML (target): $xmlPath"
Write-Host "Log: $logPath"

$env:CB_TEST_RESULTS_PATH = $xmlPath

& "$unity" `
  -projectPath "$projectRoot" `
  -batchmode -nographics `
  -executeMethod CI.PlayModeCiRunner.Run `
  -logFile "$logPath" `
  -quit
$unityExit = $LASTEXITCODE

if (-not (Test-Path $xmlPath)) {
  Write-Host "[CI] Unexpected: results missing even after seed; restoring seed."
  $seed | Set-Content -Path $xmlPath -Encoding UTF8
}

[xml]$xml = Get-Content $xmlPath
[int]$total = 0
$node = $xml.SelectSingleNode('//test-run')
if ($node -and $node.Attributes['total']) { $total = [int]$node.Attributes['total'].Value }
else { $cases = $xml.SelectNodes('//test-case'); if ($cases) { $total = $cases.Count } }

if ($total -eq 0) {
  Write-Host "[CI] No PlayMode tests found (XML total==0). Failing lane."
  exit 2
}

exit $unityExit
