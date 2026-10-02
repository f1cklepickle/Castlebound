using System.Collections;
using System.Collections.Generic;
using Castlebound.Gameplay.AI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Castlebound.Tests.PlayMode.AI
{
    public class EnemyOverlapChainRecoveryPlayTests
    {
        private readonly List<GameObject> objects = new List<GameObject>();
        private readonly Vector2 origin = new Vector2(2200f, 2200f);

        [TearDown]
        public void TearDown()
        {
            foreach (var item in objects) if (item != null) Object.DestroyImmediate(item);
            objects.Clear(); Physics2D.SyncTransforms();
        }

        [UnityTest]
        public IEnumerator ThreeBodyExactStack_RecoversThroughBoundedOwnedSteps() => Recover(false, false);

        [UnityTest]
        public IEnumerator WallAdjacentStack_UsesLegalSidesWithoutPenetration() => Recover(true, false);

        [UnityTest]
        public IEnumerator LockedMiddleStack_RecoversWithoutMovingLockedBody() => Recover(false, true);

        private IEnumerator Recover(bool besideWall, bool lockMiddle)
        {
            BoxCollider2D wall = null;
            if (besideWall)
            {
                var go = new GameObject("OverlapChainWall"); objects.Add(go);
                go.layer = LayerMask.NameToLayer("Walls");
                go.transform.position = origin + Vector2.left * 1.09f;
                wall = go.AddComponent<BoxCollider2D>(); wall.size = new Vector2(0.4f, 4f);
            }
            var actors = new EnemySeparationCollider[3];
            var before = new Vector2[3];
            for (int i = 0; i < actors.Length; i++)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Enemy_Lurker.prefab");
                Assert.NotNull(prefab);
                var go = Object.Instantiate(prefab, origin, Quaternion.identity); objects.Add(go);
                go.GetComponent<EnemyController2D>().enabled = false;
                var attack = go.GetComponent<EnemyAttack>(); if (attack != null) attack.enabled = false;
                actors[i] = go.GetComponentInChildren<EnemySeparationCollider>();
                actors[i].Owner.Speed = 0.5f;
            }
            if (lockMiddle)
            {
                var body = actors[1].Body;
                var root = body.GetComponent<EnemyRootReceiver>() ?? body.gameObject.AddComponent<EnemyRootReceiver>();
                root.RootAt(body.position, 100f);
            }
            Physics2D.SyncTransforms();
            for (int tick = 0; tick < 100; tick++)
            {
                for (int i = 0; i < actors.Length; i++)
                {
                    before[i] = actors[i].Body.position;
                    if (wall != null) Assert.That(Physics2D.Distance(actors[i].Body.GetComponent<CircleCollider2D>(), wall).distance,
                        Is.GreaterThanOrEqualTo(-0.002f));
                }
                yield return new WaitForFixedUpdate();
                for (int i = 0; i < actors.Length; i++)
                {
                    Assert.That(Vector2.Distance(before[i], actors[i].Body.position),
                        Is.LessThanOrEqualTo(actors[i].Owner.Speed * Time.fixedDeltaTime + 0.001f));
                    Assert.That(actors[i].DebugFallbackCorrectionCount, Is.Zero);
                    if (lockMiddle && i == 1) Assert.That(actors[i].Body.position, Is.EqualTo(origin));
                    if (wall != null) Assert.That(Physics2D.Distance(actors[i].Body.GetComponent<CircleCollider2D>(), wall).distance,
                        Is.GreaterThanOrEqualTo(-0.002f));
                    for (int j = i + 1; j < actors.Length; j++)
                        Assert.That(Vector2.Distance(actors[i].Body.position, actors[j].Body.position),
                            Is.GreaterThanOrEqualTo(Mathf.Min(Vector2.Distance(before[i], before[j]), 0.4f) - 0.001f));
                }
            }
            for (int i = 0; i < actors.Length; i++)
            for (int j = i + 1; j < actors.Length; j++)
                Assert.That(Vector2.Distance(actors[i].Body.position, actors[j].Body.position), Is.GreaterThanOrEqualTo(0.395f));
        }
    }
}
