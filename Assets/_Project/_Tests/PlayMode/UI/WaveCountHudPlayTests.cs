using System.Collections;
using System.Reflection;
using Castlebound.Gameplay.Spawning;
using Castlebound.Gameplay.UI;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;

namespace Castlebound.Tests.PlayMode.UI
{
    public class WaveCountHudPlayTests
    {
        [UnityTest]
        public IEnumerator WaveCountHud_RefreshesWhenRunnerStartsWave()
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
            yield return null;

            InvokeWaveStarted(runner, 2);
            yield return null;

            Assert.AreEqual("Wave 2", text.text);

            Object.DestroyImmediate(hudObject);
            Object.DestroyImmediate(runnerObject);
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
    }
}
