using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CI
{
    public static class AndroidCiBuildRunner
    {
        [MenuItem("CI/Build Android APK (CI)")]
        public static void Run()
        {
            var outputPath = Environment.GetEnvironmentVariable("CB_ANDROID_APK_PATH");
            if (string.IsNullOrWhiteSpace(outputPath))
            {
                outputPath = "build/Android/castlebound-debug.apk";
            }

            outputPath = outputPath.Replace('\\', '/');
            var outDir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrWhiteSpace(outDir))
            {
                Directory.CreateDirectory(outDir);
            }

            // Step 1: Switch to Android platform explicitly.
            // This makes the editor context Android so subsequent asset imports produce the
            // Android-platform Library artifact (54320bc...) that BuildPlayer.PrepareForBuild
            // reads — NOT the editor artifact (58fd4f58...) that ForceUpdate produces when
            // the platform is PC/editor.
            Debug.Log("[CI][Android] Switching active build target to Android...");
            var switched = EditorUserBuildSettings.SwitchActiveBuildTarget(
                BuildTargetGroup.Android, BuildTarget.Android);
            if (!switched)
            {
                Debug.LogError("[CI][Android] Failed to switch build target to Android.");
                EditorApplication.Exit(1);
                return;
            }
            Debug.Log("[CI][Android] Build target switched to Android.");

            // Step 2: Re-patch ProjectSettings.asset on disk.
            // SwitchActiveBuildTarget resets AndroidTargetArchitectures to 0 and may write it
            // back to the source file. Re-writing ARM64 (2) here ensures the ForceUpdate
            // reimport below reads the correct value from disk.
            var settingsPath = Path.Combine(Directory.GetCurrentDirectory(), "ProjectSettings", "ProjectSettings.asset");
            if (File.Exists(settingsPath))
            {
                var content = File.ReadAllText(settingsPath);
                var patched = System.Text.RegularExpressions.Regex.Replace(
                    content, @"AndroidTargetArchitectures:\s*\d+", "AndroidTargetArchitectures: 2");
                File.WriteAllText(settingsPath, patched);
                Debug.Log("[CI][Android] Re-patched AndroidTargetArchitectures to ARM64 (2) after platform switch.");
            }

            // Step 3: Force-reimport ProjectSettings from disk while the editor is on Android.
            // This updates the Android-platform Library artifact with ARM64=2 from disk.
            // BuildPlayer.PrepareForBuild will find this artifact current and use ARM64=2.
            // Since the editor is already on Android, BuildPlayer needs no internal switch,
            // so it won't regenerate the artifact and overwrite our value.
            AssetDatabase.ImportAsset("ProjectSettings/ProjectSettings.asset", ImportAssetOptions.ForceUpdate);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            Debug.Log($"[CI][Android] Target architectures after re-patch + reimport: {PlayerSettings.Android.targetArchitectures}");

            var scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                Debug.LogError("[CI][Android] No enabled scenes found in Build Settings.");
                EditorApplication.Exit(1);
                return;
            }

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = BuildTarget.Android,
                options = BuildOptions.Development
            };

            Debug.Log($"[CI][Android] Building APK to: {outputPath}");
            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;

            Debug.Log($"[CI][Android] Build result: {summary.result}, size: {summary.totalSize} bytes");
            Debug.Log($"[CI][Android] Build output path: {summary.outputPath}");

            var exitCode = summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded ? 0 : 1;
            EditorApplication.Exit(exitCode);
        }
    }
}
