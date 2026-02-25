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

Step "Workspace : $ws"
Step "APK Path   : $apkPath"
Step "Log Path   : $logPath"
Step "Unity      : $env:UNITY_EDITOR"

& "$env:UNITY_EDITOR" -batchmode -nographics `
  -projectPath $ws `
  -buildTarget Android `
  -executeMethod CI.AndroidCiBuildRunner.Run `
  -logFile $logPath
$unityCode = $LASTEXITCODE

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
