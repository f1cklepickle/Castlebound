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
