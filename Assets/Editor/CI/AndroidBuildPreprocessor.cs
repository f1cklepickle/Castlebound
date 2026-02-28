using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace CI
{
    /// <summary>
    /// Belt-and-suspenders ARM64 enforcement inside BuildPipeline.BuildPlayer.
    /// Fires with callbackOrder=int.MinValue (first among IPreprocessBuildWithReport callbacks)
    /// before PrepareForBuild runs. Deletes the stale 54320bc artifact so PrepareForBuild
    /// is forced to reimport ProjectSettings.asset fresh with the ARM64=2 value that
    /// AndroidArchitectureInitializer already stamped in-memory and on disk.
    /// </summary>
    public class AndroidBuildPreprocessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => int.MinValue;

        private const string BadArtifactRelPath = @"Library\Artifacts\54\54320bc962bffbb4d33b9c405bfb6b11";

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.Android)
                return;

            Debug.Log($"[CI] AndroidBuildPreprocessor: current arch = {PlayerSettings.Android.targetArchitectures}");

            // Delete stale artifact so PrepareForBuild must reimport from disk (ARM64=2).
            var artifactPath = Path.Combine(Directory.GetCurrentDirectory(), BadArtifactRelPath);
            if (File.Exists(artifactPath))
            {
                File.Delete(artifactPath);
                Debug.Log("[CI] AndroidBuildPreprocessor: deleted stale 54320bc artifact.");
            }
            else
            {
                Debug.Log("[CI] AndroidBuildPreprocessor: 54320bc artifact not present (already clean).");
            }

            // Re-patch disk value and flush native state as belt-and-suspenders.
            var settingsPath = Path.Combine(Directory.GetCurrentDirectory(), "ProjectSettings", "ProjectSettings.asset");
            if (File.Exists(settingsPath))
            {
                var content = File.ReadAllText(settingsPath);
                var patched = System.Text.RegularExpressions.Regex.Replace(
                    content, @"AndroidTargetArchitectures:\s*\d+", "AndroidTargetArchitectures: 2");
                File.WriteAllText(settingsPath, patched);
            }

            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            AssetDatabase.SaveAssets();
            Debug.Log($"[CI] AndroidBuildPreprocessor: confirmed arch = {PlayerSettings.Android.targetArchitectures}");
        }
    }
}
