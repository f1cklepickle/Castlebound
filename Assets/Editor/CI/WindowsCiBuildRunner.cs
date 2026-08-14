using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace CI
{
    public static class WindowsCiBuildRunner
    {
        [MenuItem("CI/Build Windows Player (CI)")]
        public static void Run()
        {
            var outputDirectory = Environment.GetEnvironmentVariable("CB_WINDOWS_BUILD_DIR");
            if (string.IsNullOrWhiteSpace(outputDirectory))
                outputDirectory = "build/Windows/Castlebound";

            outputDirectory = outputDirectory.Replace('\\', '/');
            Directory.CreateDirectory(outputDirectory);
            var executablePath = Path.Combine(outputDirectory, "Castlebound.exe").Replace('\\', '/');

            var scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                Debug.LogError("[CI][Windows] No enabled scenes found in Build Settings.");
                EditorApplication.Exit(1);
                return;
            }

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = executablePath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development
            };

            Debug.Log($"[CI][Windows] Building player to: {executablePath}");
            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;

            Debug.Log($"[CI][Windows] Build result: {summary.result}, size: {summary.totalSize} bytes");
            Debug.Log($"[CI][Windows] Build output path: {summary.outputPath}");

            EditorApplication.Exit(summary.result == BuildResult.Succeeded ? 0 : 1);
        }
    }
}
