$ErrorActionPreference = "Stop"

function Step($msg) { Write-Host "[CI] $msg" }

if ([string]::IsNullOrWhiteSpace($env:UNITY_EDITOR)) {
  $env:UNITY_EDITOR = "D:\Program Files\Unity\Hub\Editor\2022.3.62f2\Editor\Unity.exe"
}
if (-not (Test-Path $env:UNITY_EDITOR)) {
  throw "UNITY_EDITOR not found at '$env:UNITY_EDITOR'. Set UNITY_EDITOR or update the fallback path."
}

$ws = (Get-Location).Path
$buildDir = Join-Path $ws "build\Android"
$apkPath = Join-Path $buildDir "castlebound-debug.apk"
$logPath = Join-Path $ws "build\AndroidBuildLog.txt"

New-Item -ItemType Directory -Force -Path $buildDir | Out-Null
if (Test-Path $apkPath) { Remove-Item -Force $apkPath }
if (Test-Path $logPath) { Remove-Item -Force $logPath }

$env:CB_ANDROID_APK_PATH = $apkPath

# Derive Android SDK/NDK/JDK from the Unity installation (bundled with Unity)
$unityEditorDir   = Split-Path $env:UNITY_EDITOR
$androidPlayerDir = Join-Path $unityEditorDir "Data\PlaybackEngines\AndroidPlayer"
$androidSdk       = Join-Path $androidPlayerDir "SDK"
$androidNdk       = Join-Path $androidPlayerDir "NDK"
$jdk              = Join-Path $androidPlayerDir "OpenJDK"

Step "Workspace : $ws"
Step "APK Path   : $apkPath"
Step "Log Path   : $logPath"
Step "Unity      : $env:UNITY_EDITOR"
Step "SDK        : $androidSdk"
Step "NDK        : $androidNdk"
Step "JDK        : $jdk"

$arguments = @(
  '-batchmode', '-nographics',
  '-projectPath', $ws,
  '-executeMethod', 'CI.AndroidCiBuildRunner.Run',
  '-logFile', $logPath,
  '-androidsdk', $androidSdk,
  '-androidndk', $androidNdk,
  '-jdk', $jdk
)

# Patch AndroidTargetArchitectures to ARM64 (2) directly in ProjectSettings.asset.
# SwitchActiveBuildTarget resets this value to 0 regardless of in-memory or disk state.
# Writing to disk here ensures the correct value survives any reimport during BuildPlayer.
$settingsPath = Join-Path $ws "ProjectSettings\ProjectSettings.asset"
if (Test-Path $settingsPath) {
  $content = Get-Content $settingsPath -Raw
  $patched  = $content -replace 'AndroidTargetArchitectures:\s*\d+', 'AndroidTargetArchitectures: 2'
  [System.IO.File]::WriteAllText($settingsPath, $patched)
  Step "Patched AndroidTargetArchitectures to ARM64 (2) in ProjectSettings.asset"
}

Step "Launching Unity..."
$proc = Start-Process -FilePath $env:UNITY_EDITOR -ArgumentList $arguments -Wait -PassThru -NoNewWindow
$unityCode = $proc.ExitCode
Step "Unity exited with code: $unityCode"

if ($unityCode -ne 0) {
  Write-Error "Unity Android build failed with exit code $unityCode"
  if (Test-Path $logPath) { Get-Content $logPath -Tail 300 }
  exit $unityCode
}

if (-not (Test-Path $apkPath)) {
  Write-Error "Expected APK not found at '$apkPath'"
  if (Test-Path $logPath) { Get-Content $logPath -Tail 300 }
  exit 2
}

Step "Android APK build completed successfully."
exit 0
