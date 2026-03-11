using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Castlebound.Tests.PlayMode.Combat
{
    public class PlayerHighRateCadenceConsistencyPlayTests
    {
        [UnityTest]
        public IEnumerator Rate10_ProducesStableIntervals_OverTimeWindow()
        {
            var root = new GameObject("PlayerAttackLoop");
            var loop = root.AddComponent<PlayerAttackLoop>();

            var intervals = new List<float>();
            float simTime = 0f;
            float lastCompletionTime = -1f;
            int lastCompleted = 0;
            const float dt = 0.01f;

            while (simTime < 2.5f)
            {
                loop.Tick(dt, 10f, true);
                simTime += dt;

                if (loop.CompletedSwingCount > lastCompleted)
                {
                    for (int i = lastCompleted; i < loop.CompletedSwingCount; i++)
                    {
                        if (lastCompletionTime >= 0f)
                            intervals.Add(simTime - lastCompletionTime);
                        lastCompletionTime = simTime;
                    }

                    lastCompleted = loop.CompletedSwingCount;
                }

                yield return null;
            }

            Assert.GreaterOrEqual(intervals.Count, 18, "Expected enough swing completions to evaluate cadence stability.");
            Assert.That(Mean(intervals), Is.EqualTo(0.1f).Within(0.03f),
                "Medium-rate cadence should remain near expected interval.");
            Assert.Less(Max(intervals) - Min(intervals), 0.06f,
                "Cadence intervals should remain reasonably stable at medium rate.");

            Object.Destroy(root);
        }

        [UnityTest]
        public IEnumerator Rate20_ProducesStableIntervals_WithoutTwoSwingDrift()
        {
            var root = new GameObject("PlayerAttackLoop");
            var loop = root.AddComponent<PlayerAttackLoop>();

            var intervals = new List<float>();
            float simTime = 0f;
            float lastCompletionTime = -1f;
            int lastCompleted = 0;
            const float dt = 0.005f;

            while (simTime < 2f)
            {
                loop.Tick(dt, 20f, true);
                simTime += dt;

                if (loop.CompletedSwingCount > lastCompleted)
                {
                    for (int i = lastCompleted; i < loop.CompletedSwingCount; i++)
                    {
                        if (lastCompletionTime >= 0f)
                            intervals.Add(simTime - lastCompletionTime);
                        lastCompletionTime = simTime;
                    }

                    lastCompleted = loop.CompletedSwingCount;
                }

                yield return null;
            }

            Assert.GreaterOrEqual(intervals.Count, 25, "Expected enough high-rate completions for drift validation.");
            Assert.That(Mean(intervals), Is.EqualTo(0.05f).Within(0.015f),
                "High-rate cadence should remain near expected interval.");
            Assert.Less(Max(intervals), 0.08f,
                "High-rate cadence should not drift into periodic long gaps.");

            Object.Destroy(root);
        }

        [UnityTest]
        public IEnumerator HoldRelease_StopsFurtherSwingsPromptly()
        {
            var root = new GameObject("PlayerAttackLoop");
            var loop = root.AddComponent<PlayerAttackLoop>();

            for (int i = 0; i < 30; i++)
            {
                loop.Tick(0.01f, 4f, true);
                yield return null;
            }

            var atRelease = loop.CompletedSwingCount;
            for (int i = 0; i < 40; i++)
            {
                loop.Tick(0.01f, 4f, false);
                yield return null;
            }

            Assert.LessOrEqual(loop.CompletedSwingCount, atRelease + 1,
                "Release should stop further chaining after at most the in-flight swing.");

            Object.Destroy(root);
        }

        private static float Mean(List<float> values)
        {
            float sum = 0f;
            for (int i = 0; i < values.Count; i++)
                sum += values[i];
            return values.Count > 0 ? sum / values.Count : 0f;
        }

        private static float Min(List<float> values)
        {
            float min = float.MaxValue;
            for (int i = 0; i < values.Count; i++)
                min = Mathf.Min(min, values[i]);
            return values.Count > 0 ? min : 0f;
        }

        private static float Max(List<float> values)
        {
            float max = float.MinValue;
            for (int i = 0; i < values.Count; i++)
                max = Mathf.Max(max, values[i]);
            return values.Count > 0 ? max : 0f;
        }
    }
}
