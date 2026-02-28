using UnityEditor;
using UnityEngine;

namespace CI
{
    /// <summary>
    /// Sets ARM64 as the Android target architecture before Unity's initial asset database
    /// refresh (InitialRefreshV2). This is the earliest hook available in managed C# code.
    ///
    /// Initialization order (Unity 2022.3 batchmode):
    ///   1. Unity native init
    ///   2. Script compilation (if ScriptAssemblies missing) + domain reload
    ///   3. [InitializeOnLoad] static constructors  ← we fire here
    ///   4. InitialRefreshV2 — the 169-second startup asset import that creates artifact
    ///      54320bc (Android context for ProjectSettings.asset). Reading ARM64 from the
    ///      native C++ PlayerSettings structure at this point produces ARM64=2.
    ///   5. -executeMethod (AndroidCiBuildRunner.Run) is called
    ///
    /// Without this class, the value baked into 54320bc during step 4 reflects whatever
    /// AndroidTargetArchitectures was on disk when the Library rebuilt — which could be 0
    /// from a previous failed run — causing CheckPrerequisites to throw
    /// "Target architecture not specified".
    /// </summary>
    [InitializeOnLoad]
    public static class AndroidArchitectureInitializer
    {
        static AndroidArchitectureInitializer()
        {
            if (PlayerSettings.Android.targetArchitectures == AndroidArchitecture.ARM64)
            {
                Debug.Log("[CI] AndroidArchitectureInitializer: ARM64 already set, nothing to do.");
                return;
            }

            Debug.Log($"[CI] AndroidArchitectureInitializer: arch was {PlayerSettings.Android.targetArchitectures}, forcing ARM64.");
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            AssetDatabase.SaveAssets();
            Debug.Log($"[CI] AndroidArchitectureInitializer: arch after SaveAssets = {PlayerSettings.Android.targetArchitectures}");
        }
    }
}
