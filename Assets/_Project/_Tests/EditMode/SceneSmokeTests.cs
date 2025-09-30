using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;

[Category("Smoke/Scene")]
public class SceneSmokeTests
{
    [Test]
    public void SampleScene_Loads()
    {
        var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/SampleScene.unity", OpenSceneMode.Single);
        Assert.IsTrue(scene.isLoaded, "Scene failed to load.");
    }

    [Test]
    public void SampleScene_Has_No_MissingScripts_OnRoots()
    {
        var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/SampleScene.unity", OpenSceneMode.Single);
        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var c in root.GetComponentsInChildren<Component>(true))
                Assert.IsFalse(c == null, $"Missing script under root: {root.name}");
        }
    }
}
