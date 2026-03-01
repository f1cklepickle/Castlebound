using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace CI
{
    /// <summary>
    /// Belt-and-suspenders ARM64 enforcement that fires inside BuildPipeline.BuildPlayer,
    /// before PrepareForBuild runs.
    ///
    /// IMPORTANT: Do NOT delete the 54320bc artifact here and do NOT call SaveAssets().
    ///
    /// The 262-second startup InitialRefreshV2 already created 54320bc correctly with
    /// ARM64=2 (because AndroidArchitectureInitializer set ARM64 before that refresh ran,
    /// and the disk file had AndroidTargetArchitectures: 2).
    ///
    /// Previous attempts that deleted 54320bc + called SaveAssets() here triggered a fresh
    /// reimport of ProjectSettings.asset. Due to a Unity batchmode serialization bug,
    /// that reimport writes AndroidTargetArchitectures=0 to disk and creates a new 54320bc
    /// with ARM64=0, causing CheckPrerequisites to throw "Target architecture not specified".
    ///
    /// The correct behaviour is: leave 54320bc alone, just confirm the native value is ARM64.
    /// </summary>
    public class AndroidBuildPreprocessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => int.MinValue;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.Android)
                return;

            // Set native value as belt-and-suspenders. Do NOT call SaveAssets() —
            // that triggers a reimport which overwrites the correct 54320bc with ARM64=0.
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            Debug.Log($"[CI] AndroidBuildPreprocessor: confirmed arch = {PlayerSettings.Android.targetArchitectures}");
        }
    }
}
