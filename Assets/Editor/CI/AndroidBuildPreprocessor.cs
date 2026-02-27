using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace CI
{
    /// <summary>
    /// Forces ARM64 target architecture for every Android build from inside
    /// BuildPipeline.BuildPlayer via IPreprocessBuildWithReport.
    ///
    /// The problem: BuildPlayer.PrepareForBuild internally creates a platform-specific
    /// Library artifact for ProjectSettings that always contains AndroidTargetArchitectures=0,
    /// regardless of what is set in memory or on disk before BuildPlayer is called.
    /// AssetDatabase.ImportAsset cannot update this artifact — it lives in a separate
    /// build-pipeline import context.
    ///
    /// This callback fires inside BuildPlayer (callbackOrder=int.MinValue runs first).
    /// If it executes before PrepareForBuild's CheckPrerequisites, the architecture
    /// value will be correct when the prerequisite check reads it.
    /// </summary>
    public class AndroidBuildPreprocessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => int.MinValue;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.Android)
                return;

            Debug.Log($"[CI] AndroidBuildPreprocessor.OnPreprocessBuild: current arch = {PlayerSettings.Android.targetArchitectures}");
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            Debug.Log($"[CI] AndroidBuildPreprocessor.OnPreprocessBuild: set to ARM64, confirmed = {PlayerSettings.Android.targetArchitectures}");
        }
    }
}
