using UnityEditor;
using UnityEngine;

namespace CI
{
    /// <summary>
    /// Sets ARM64 as the Android target architecture before Unity's initial asset database
    /// refresh (InitialRefreshV2). Fires after domain reload but before the 262-second
    /// startup import that creates artifact 54320bc.
    ///
    /// IMPORTANT: Do NOT call AssetDatabase.SaveAssets() here.
    /// SaveAssets() triggers a reimport of ProjectSettings.asset. Unity has a batchmode
    /// serialization bug where that reimport writes AndroidTargetArchitectures=0 to disk,
    /// causing a fresh 54320bc to be created with ARM64=0. Instead, just set the native
    /// value — the startup InitialRefreshV2 reads the correct ARM64=2 from disk and
    /// bakes it into 54320bc correctly without any SaveAssets() interference.
    /// </summary>
    [InitializeOnLoad]
    public static class AndroidArchitectureInitializer
    {
        static AndroidArchitectureInitializer()
        {
            if (PlayerSettings.Android.targetArchitectures == AndroidArchitecture.ARM64)
            {
                Debug.Log("[CI] AndroidArchitectureInitializer: ARM64 already set.");
                return;
            }

            Debug.Log($"[CI] AndroidArchitectureInitializer: was {PlayerSettings.Android.targetArchitectures}, setting ARM64.");
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            Debug.Log($"[CI] AndroidArchitectureInitializer: confirmed = {PlayerSettings.Android.targetArchitectures}");
        }
    }
}
