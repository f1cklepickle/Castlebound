using System.Collections;
using System.Collections.Generic;
using System.Reflection;
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
                    enemyTypeId = EnemyArchetypeIds.GoblinMelee,
                    spawnCount = 2,
                    intervalSeconds = 0.25f,
                    initialDelaySeconds = 0.1f
                }
            };
            sequencesField.SetValue(scheduleAsset, sequenceList);

            // Markers.
            var markerA = new GameObject("MarkerA").AddComponent<SpawnPointMarker>();
            markerA.transform.position = new Vector2(-1f, 0f);
            markerA.gameObject.AddComponent<GateIdProvider>().Initialize("GateA");

            var markerB = new GameObject("MarkerB").AddComponent<SpawnPointMarker>();
            markerB.transform.position = new Vector2(2f, 0f);
            markerB.gameObject.AddComponent<GateIdProvider>().Initialize("GateB");

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
            mappingType.GetField("enemyTypeId").SetValue(mapping, EnemyArchetypeIds.GoblinMelee);
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

        [UnityTest]
        public IEnumerator SpawnsMixedGoblinArchetypesFromAuthoredWave()
        {
            var meleePrefab = new GameObject("GoblinMeleePrefab");
            var rangedPrefab = new GameObject("GoblinRangedPrefab");
            var scheduleAsset = ScriptableObject.CreateInstance<EnemySpawnScheduleAsset>();
            var wavesField = typeof(EnemySpawnScheduleAsset).GetField(
                "waves",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            wavesField.SetValue(scheduleAsset, new List<WaveConfig>
            {
                new WaveConfig
                {
                    sequences = new List<SpawnSequenceConfig>
                    {
                        new SpawnSequenceConfig
                        {
                            enemyTypeId = EnemyArchetypeIds.GoblinMelee,
                            spawnCount = 1,
                            intervalSeconds = 1f,
                            initialDelaySeconds = 0f
                        },
                        new SpawnSequenceConfig
                        {
                            enemyTypeId = EnemyArchetypeIds.GoblinRanged,
                            spawnCount = 1,
                            intervalSeconds = 1f,
                            initialDelaySeconds = 0f
                        }
                    },
                    waitForClear = false
                }
            });

            var marker = new GameObject("MixedMarker").AddComponent<SpawnPointMarker>();
            marker.transform.position = new Vector2(1f, 0f);
            marker.gameObject.AddComponent<GateIdProvider>().Initialize("GateA");

            var runnerGO = new GameObject("MixedSpawnerRunner");
            var runner = runnerGO.AddComponent<EnemySpawnerRunner>();

            typeof(EnemySpawnerRunner).GetField("scheduleAsset", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(runner, scheduleAsset);
            typeof(EnemySpawnerRunner).GetField("spawnMarkers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(runner, new List<SpawnPointMarker> { marker });

            var mappingType = typeof(EnemySpawnerRunner).GetNestedType("EnemyPrefabMapping", System.Reflection.BindingFlags.NonPublic);
            var mappingListType = typeof(List<>).MakeGenericType(mappingType);
            var mappingList = (IList)System.Activator.CreateInstance(mappingListType);
            AddMapping(mappingType, mappingList, EnemyArchetypeIds.GoblinMelee, meleePrefab);
            AddMapping(mappingType, mappingList, EnemyArchetypeIds.GoblinRanged, rangedPrefab);
            typeof(EnemySpawnerRunner).GetField("prefabMappings", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(runner, mappingList);

            yield return null;
            yield return new WaitForSeconds(0.2f);

            Assert.NotNull(GameObject.Find("GoblinMeleePrefab(Clone)"));
            Assert.NotNull(GameObject.Find("GoblinRangedPrefab(Clone)"));

            Object.DestroyImmediate(runnerGO);
            Object.DestroyImmediate(marker.gameObject);
            DestroyClone("GoblinMeleePrefab(Clone)");
            DestroyClone("GoblinRangedPrefab(Clone)");
            Object.DestroyImmediate(meleePrefab);
            Object.DestroyImmediate(rangedPrefab);
            Object.DestroyImmediate(scheduleAsset);
        }

        [UnityTest]
        public IEnumerator SpawnReady_InitializesFacingFromEveryCardinalMarkerDirection()
        {
            var enemyPrefab = new GameObject("DirectionalEnemyPrefab");
            enemyPrefab.AddComponent<EnemyFacing>();
            var runnerObject = new GameObject("DirectionalSpawnerRunner");
            var runner = runnerObject.AddComponent<EnemySpawnerRunner>();
            var prefabMap = new Dictionary<string, GameObject> { [EnemyArchetypeIds.GoblinMelee] = enemyPrefab };
            typeof(EnemySpawnerRunner).GetField("_prefabMap", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(runner, prefabMap);

            var directions = new[] { Vector2.up, Vector2.right, Vector2.down, Vector2.left };
            var requests = new List<SpawnRequest>();
            for (var i = 0; i < directions.Length; i++)
            {
                requests.Add(new SpawnRequest(EnemyArchetypeIds.GoblinMelee, $"Gate{i}", "Center", new Vector2(i * 2f, 5f), directions[i]));
            }

            var spawnReady = typeof(EnemySpawnerRunner).GetMethod("SpawnReady", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(spawnReady);
            spawnReady.Invoke(runner, new object[] { requests });
            yield return null;

            var clones = new List<GameObject>();
            foreach (var candidate in Object.FindObjectsOfType<EnemyFacing>())
            {
                if (candidate.gameObject.name.Contains("(Clone)"))
                {
                    clones.Add(candidate.gameObject);
                }
            }

            Assert.That(clones.Count, Is.EqualTo(4));
            clones.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));
            for (var i = 0; i < directions.Length; i++)
            {
                Assert.That(clones[i].GetComponent<EnemyFacing>().AimDirection, Is.EqualTo(directions[i]));
            }

            foreach (var clone in clones)
            {
                Object.DestroyImmediate(clone);
            }
            Object.DestroyImmediate(runnerObject);
            Object.DestroyImmediate(enemyPrefab);
        }

        private static void AddMapping(System.Type mappingType, IList mappingList, string enemyTypeId, GameObject prefab)
        {
            var mapping = System.Activator.CreateInstance(mappingType);
            mappingType.GetField("enemyTypeId").SetValue(mapping, enemyTypeId);
            mappingType.GetField("prefab").SetValue(mapping, prefab);
            mappingList.Add(mapping);
        }

        private static void DestroyClone(string objectName)
        {
            var clone = GameObject.Find(objectName);
            if (clone != null)
            {
                Object.DestroyImmediate(clone);
            }
        }
    }
}
