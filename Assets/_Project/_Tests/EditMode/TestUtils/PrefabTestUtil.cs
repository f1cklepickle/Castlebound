using UnityEditor;
using UnityEngine;

public static class PrefabTestUtil
{
    public static GameObject Load(string assetPath)
    {
        var go = PrefabUtility.LoadPrefabContents(assetPath);
        if (!go) throw new System.Exception($"Failed to load prefab at: {assetPath}");
        return go;
    }

    public static void Unload(GameObject go)
    {
        if (go) PrefabUtility.UnloadPrefabContents(go);
    }
}
