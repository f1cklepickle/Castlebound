using System.Reflection;
using Castlebound.Gameplay.Spawning;
using Castlebound.Gameplay.UI;
using NUnit.Framework;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Castlebound.Tests.UI
{
    public class WaveCountHudTests
    {
        [Test]
        public void Initialize_DisplaysCurrentWave_FromProvider()
        {
            var root = new GameObject("WaveHud");
            var providerObject = new GameObject("WaveProvider");
            var provider = providerObject.AddComponent<WaveIndexProviderComponent>();
            provider.CurrentWaveIndex = 4;

            var text = CreateText("WaveText", root.transform);
            var hud = root.AddComponent<WaveCountHud>();
            SetPrivateField(hud, "waveText", text);
            hud.SetWaveIndexProvider(provider);

            hud.Initialize();

            Assert.AreEqual("Wave 4", text.text);

            Object.DestroyImmediate(root);
            Object.DestroyImmediate(providerObject);
        }

        [Test]
        public void WaveStarted_UpdatesDisplayedWave_FromRunnerEvent()
        {
            var runnerObject = new GameObject("EnemySpawnerRunner");
            var runner = runnerObject.AddComponent<EnemySpawnerRunner>();
            var provider = runnerObject.AddComponent<WaveIndexProviderComponent>();
            provider.CurrentWaveIndex = 1;

            var hudObject = new GameObject("WaveHud");
            var text = CreateText("WaveText", hudObject.transform);
            var hud = hudObject.AddComponent<WaveCountHud>();
            SetPrivateField(hud, "waveText", text);
            hud.SetWaveRunner(runner);
            hud.Initialize();

            InvokeWaveStarted(runner, 3);

            Assert.AreEqual("Wave 3", text.text);
            Assert.AreEqual(3, provider.CurrentWaveIndex);

            Object.DestroyImmediate(hudObject);
            Object.DestroyImmediate(runnerObject);
        }

        [Test]
        public void Initialize_ClearsText_WhenProviderMissing()
        {
            var root = new GameObject("WaveHud");
            var text = CreateText("WaveText", root.transform);
            text.text = "Wave 9";

            var hud = root.AddComponent<WaveCountHud>();
            SetPrivateField(hud, "waveText", text);

            hud.Initialize();

            Assert.AreEqual(string.Empty, text.text);

            Object.DestroyImmediate(root);
        }

        [Test]
        public void MainPrototype_WaveCounterHud_IsWiredBesidePotionHud()
        {
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/MainPrototype.unity", OpenSceneMode.Single);
            Assert.IsTrue(scene.isLoaded, "MainPrototype scene should load.");

            var waveHud = FindInScene<WaveCountHud>("WaveCounterHud");
            var potionHud = GameObject.Find("PotionHud");

            Assert.NotNull(waveHud, "MainPrototype should include a WaveCounterHud.");
            Assert.NotNull(potionHud, "MainPrototype should include the existing PotionHud.");
            Assert.NotNull(GetPrivateField<TextMeshProUGUI>(waveHud, "waveText"), "WaveCounterHud should reference its TMP label.");
            Assert.NotNull(GetPrivateField<EnemySpawnerRunner>(waveHud, "waveRunner"), "WaveCounterHud should reference the scene wave runner.");

            var waveRect = waveHud.GetComponent<RectTransform>();
            var potionRect = potionHud.GetComponent<RectTransform>();
            Assert.That(waveRect.anchoredPosition.x, Is.GreaterThan(potionRect.anchoredPosition.x), "Wave counter should sit beside the potion slot on the bottom-left HUD.");
            Assert.That(waveRect.anchoredPosition.y, Is.EqualTo(potionRect.anchoredPosition.y), "Wave counter should align with the potion slot baseline.");
        }

        private static TextMeshProUGUI CreateText(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            return go.AddComponent<TextMeshProUGUI>();
        }

        private static void InvokeWaveStarted(EnemySpawnerRunner runner, int waveIndex)
        {
            var method = typeof(EnemySpawnerRunner).GetMethod(
                "HandleWaveStarted",
                BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.NotNull(method, "Expected EnemySpawnerRunner.HandleWaveStarted(int).");
            method.Invoke(runner, new object[] { waveIndex });
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(
                fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);

            field.SetValue(target, value);
        }

        private static T GetPrivateField<T>(object target, string fieldName)
        {
            var field = target.GetType().GetField(
                fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);

            return (T)field.GetValue(target);
        }

        private static T FindInScene<T>(string objectName) where T : Component
        {
            var go = GameObject.Find(objectName);
            return go != null ? go.GetComponent<T>() : null;
        }
    }
}
