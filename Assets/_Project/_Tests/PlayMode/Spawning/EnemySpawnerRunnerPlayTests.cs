using System.Collections;
using System.Collections.Generic;
using Castlebound.Gameplay.Spawning;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Castlebound.Tests.PlayMode.Spawning
{
    public class EnemySpawnerRunnerPlayTests
    {
        [UnityTest]
        public IEnumerator SpawnsEnemiesAtMarkersOverTime()
        {
            // Temp prefab.
            var enemyPrefab = new GameObject("EnemyPrefab");

            // Schedule asset (in-memory).
            var scheduleAsset = ScriptableObject.CreateInstance<EnemySpawnScheduleAsset>();
            var sequencesField = typeof(EnemySpawnScheduleAsset).GetField("sequences", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var sequenceList = new List<SpawnSequenceConfig>
            {
                new SpawnSequenceConfig
                {
                    enemyTypeId = "grunt",
                    spawnCount = 2,
                    intervalSeconds = 0.25f,
                    initialDelaySeconds = 0.1f
                }
            };
            sequencesField.SetValue(scheduleAsset, sequenceList);

            // Markers.
            var markerA = new GameObject("MarkerA").AddComponent<SpawnPointMarker>();
            markerA.transform.position = new Vector2(-1f, 0f);
            typeof(SpawnPointMarker).GetField("gateId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(markerA, "GateA");

            var markerB = new GameObject("MarkerB").AddComponent<SpawnPointMarker>();
            markerB.transform.position = new Vector2(2f, 0f);
            typeof(SpawnPointMarker).GetField("gateId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(markerB, "GateB");

            // Runner setup.
            var runnerGO = new GameObject("SpawnerRunner");
            var runner = runnerGO.AddComponent<EnemySpawnerRunner>();

            typeof(EnemySpawnerRunner).GetField("scheduleAsset", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(runner, scheduleAsset);

            var markersList = new List<SpawnPointMarker> { markerA, markerB };
            typeof(EnemySpawnerRunner).GetField("spawnMarkers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(runner, markersList);

            var mappingType = typeof(EnemySpawnerRunner).GetNestedType("EnemyPrefabMapping", System.Reflection.BindingFlags.NonPublic);
            var mappingListType = typeof(List<>).MakeGenericType(mappingType);
            var mappingList = (IList)System.Activator.CreateInstance(mappingListType);
            var mapping = System.Activator.CreateInstance(mappingType);
            mappingType.GetField("enemyTypeId").SetValue(mapping, "grunt");
            mappingType.GetField("prefab").SetValue(mapping, enemyPrefab);
            mappingList.Add(mapping);
            typeof(EnemySpawnerRunner).GetField("prefabMappings", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(runner, mappingList);

            // Allow Awake/Start.
            yield return null;

            // Wait enough for initial delay + two spawns.
            yield return new WaitForSeconds(0.6f);

            var spawned = GameObject.FindObjectsOfType<GameObject>();
            var spawnedPrefabs = new List<GameObject>();
            foreach (var go in spawned)
            {
                if (go.name.StartsWith("EnemyPrefab") && go.name.Contains("(Clone)"))
                {
                    spawnedPrefabs.Add(go);
                }
            }

            Assert.AreEqual(2, spawnedPrefabs.Count, "Runner should have spawned two enemies per schedule.");
            // First spawn at marker A, second at marker B (round-robin).
            spawnedPrefabs.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));
            Assert.That(spawnedPrefabs[0].transform.position, Is.EqualTo((Vector3)markerA.transform.position));
            Assert.That(spawnedPrefabs[1].transform.position, Is.EqualTo((Vector3)markerB.transform.position));

            Object.DestroyImmediate(runnerGO);
            Object.DestroyImmediate(markerA.gameObject);
            Object.DestroyImmediate(markerB.gameObject);
            foreach (var spawnedGo in spawnedPrefabs)
            {
                Object.DestroyImmediate(spawnedGo);
            }
            Object.DestroyImmediate(enemyPrefab);
            Object.DestroyImmediate(scheduleAsset);
        }
    }
}
