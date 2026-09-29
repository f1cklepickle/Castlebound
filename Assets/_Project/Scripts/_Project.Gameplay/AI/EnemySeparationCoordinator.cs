using System;
using System.Collections.Generic;
using UnityEngine;

namespace Castlebound.Gameplay.AI
{
    // After controller submissions, before physics. No targeting or crowd-flow policy.
    [DefaultExecutionOrder(32000)]
    public sealed class EnemySeparationCoordinator : MonoBehaviour
    {
        private static EnemySeparationCoordinator instance;
        private readonly List<EnemySeparationCollider> registered = new List<EnemySeparationCollider>(256);
        private readonly EnemySeparationCollider[] actors = new EnemySeparationCollider[EnemySeparationDiscovery.BodyCapacity];
        private readonly EnemySeparationBody[] bodies = new EnemySeparationBody[EnemySeparationDiscovery.BodyCapacity];
        private readonly EnemySeparationDiscovery discovery = new EnemySeparationDiscovery();
        private readonly EnemySeparationSolver solver = new EnemySeparationSolver();
        private readonly Vector2[] guardInputs = new Vector2[EnemySeparationDiscovery.BodyCapacity];
        private readonly Vector2[] guardOutputs = new Vector2[EnemySeparationDiscovery.BodyCapacity];
        private readonly bool[] guardCached = new bool[EnemySeparationDiscovery.BodyCapacity];
        private const int GuardCallsPerStep = 256;
        private int guardCalls;
        private Func<int, Vector2, Vector2> guard;
#if UNITY_EDITOR
        public int DebugPairCount => discovery.PairCount;
        public int DebugSaturatedSteps { get; private set; }
        public int DebugGuardBudgetRefusals { get; private set; }
#endif
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetInstance() => instance = null;
        public static void Register(EnemySeparationCollider sensor)
        {
            if (!Application.isPlaying) return;
            if (instance == null)
            {
                instance = FindObjectOfType<EnemySeparationCoordinator>();
                if (instance == null)
                {
                    var host = new GameObject("Enemy separation movement") { hideFlags = HideFlags.HideInHierarchy };
                    instance = host.AddComponent<EnemySeparationCoordinator>();
                    DontDestroyOnLoad(host);
                }
            }
            if (instance.registered.Contains(sensor)) return;
            int index = 0;
            while (index < instance.registered.Count && instance.registered[index] != null &&
                instance.registered[index].GetInstanceID() < sensor.GetInstanceID()) index++;
            instance.registered.Insert(index, sensor);
        }
        public static void Unregister(EnemySeparationCollider sensor)
        { if (instance != null) instance.registered.Remove(sensor); }
        private void Awake() => guard = Guard;
        private void OnDestroy() { if (instance == this) instance = null; }

        private void FixedUpdate()
        {
            int count = 0;
            guardCalls = 0;
#if UNITY_EDITOR
            DebugGuardBudgetRefusals = 0;
#endif
            bool overflow = false;
            for (int i = registered.Count - 1; i >= 0; i--)
            {
                var actor = registered[i];
                if (actor == null) { registered.RemoveAt(i); continue; }
                if (!actor.Participates) continue;
                if (count == bodies.Length) { overflow = true; break; }
                actors[count] = actor; bodies[count] = actor.Snapshot(Time.fixedDeltaTime);
                guardCached[count] = false; count++;
            }
            if (overflow)
            {
#if UNITY_EDITOR
                DebugSaturatedSteps++;
#endif
                foreach (var actor in registered)
                {
                    if (actor == null || !actor.Participates) continue;
                    var step = actor.Snapshot(Time.fixedDeltaTime); step.Displacement = Vector2.zero;
                    actor.Apply(step, true);
                }
                return;
            }
            solver.Solve(bodies, count, discovery, guard);
#if UNITY_EDITOR
            if (discovery.Saturated) DebugSaturatedSteps++;
#endif
            for (int i = 0; i < count; i++) actors[i].Apply(bodies[i], discovery.Saturated);
        }
        private Vector2 Guard(int index, Vector2 displacement)
        {
            if (guardCached[index] && guardInputs[index].Equals(displacement)) return guardOutputs[index];
            if (guardCalls == GuardCallsPerStep)
            {
#if UNITY_EDITOR
                DebugGuardBudgetRefusals++;
#endif
                return Vector2.zero;
            }
            guardCalls++;
            guardCached[index] = true; guardInputs[index] = displacement;
            return guardOutputs[index] = actors[index].Guard(displacement);
        }
    }
}
