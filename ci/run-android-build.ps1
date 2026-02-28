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
  '-buildTarget', 'Android',   # Unity initialises in Android platform mode so the Library
                                # rebuild imports ProjectSettings.asset in Android context.
  '-projectPath', $ws,
  '-executeMethod', 'CI.AndroidCiBuildRunner.Run',
  '-logFile', $logPath,
  '-androidsdk', $androidSdk,
  '-androidndk', $androidNdk,
  '-jdk', $jdk
)

# Force Unity to recompile all C# scripts from source so that new/changed CI scripts
# (including AndroidArchitectureInitializer.cs) are always compiled in.
$scriptAssemblies = Join-Path $ws "Library\ScriptAssemblies"
if (Test-Path $scriptAssemblies) {
  Remove-Item -Recurse -Force $scriptAssemblies
  Step "Cleared Library/ScriptAssemblies to force script recompile from source"
}

# Delete ArtifactDB so the Library rebuilds from the current disk state.
# ProjectSettings.asset has AndroidTargetArchitectures: 2 on disk (committed value).
# AndroidArchitectureInitializer [InitializeOnLoad] fires BEFORE InitialRefreshV2 and
# re-confirms ARM64 in memory, so the 54320bc artifact is created fresh with ARM64=2.
$artifactDb = Join-Path $ws "Library\ArtifactDB"
if (Test-Path $artifactDb) {
  Remove-Item -Force $artifactDb
  Step "Deleted Library/ArtifactDB to force Library rebuild"
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
