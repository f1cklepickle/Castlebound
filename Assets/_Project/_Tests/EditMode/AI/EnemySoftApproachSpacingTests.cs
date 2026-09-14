using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class EnemySoftApproachSpacingTests
{
    private GameObject subject;
    private GameObject neighbor;
    private EnemyController2D controller;
    private EnemyController2D other;
    private EnemyApproachSpread spread;
    private static readonly BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
    private static readonly BindingFlags PrivateStatic = BindingFlags.Static | BindingFlags.NonPublic;

    [SetUp]
    public void SetUp()
    {
        subject = new GameObject("SoftSpacingSubject");
        neighbor = new GameObject("SoftSpacingNeighbor");
        spread = subject.AddComponent<EnemyApproachSpread>();
        controller = subject.AddComponent<EnemyController2D>();
        typeof(EnemyController2D).GetField("approachSpread", PrivateInstance).SetValue(controller, spread);
        other = neighbor.AddComponent<EnemyController2D>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(subject);
        Object.DestroyImmediate(neighbor);
    }

    [TestCase("Assets/_Project/Prefabs/Enemy_Goblin_Melee.prefab")]
    [TestCase("Assets/_Project/Prefabs/Enemy_Lurker.prefab")]
    public void MeleePrefabs_LoadSeparateSoftAndLegacyRadiusDefaults(string path)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        Assert.IsNotNull(prefab);
        var prefabSpread = prefab.GetComponent<EnemyApproachSpread>();
        Assert.IsNotNull(prefabSpread);
        Assert.That(prefabSpread.ChaseSoftInfluenceRadius, Is.EqualTo(2f));
        Assert.That(prefabSpread.NeighborSeparationRadius, Is.EqualTo(1.5f));
        Assert.That(new SerializedObject(prefabSpread).FindProperty("meleeChaseSpacingMultiplier").floatValue,
            Is.EqualTo(1.5f));
    }

    [TestCase(0.75f, 0.625f, true)]
    [TestCase(1.5f, 0.25f, true)]
    [TestCase(1.75f, 0.125f, false)]
    [TestCase(2f, 0f, false)]
    [TestCase(2.1f, 0f, false)]
    public void Feed_UsesTwoUnitLinearWeightWithoutExtendingNeighborSignal(float distance, float weight, bool eligibleNeighbor)
    {
        Feed(distance);
        Assert.That(Read<Vector2>("_chaseSoftSeparation").y, Is.EqualTo(weight).Within(0.000001f));
        Assert.That(Read<Vector2>("_approachSeparation").y,
            Is.EqualTo(Mathf.Max(0f, 1f - distance / 1.5f)).Within(0.000001f));
        Assert.That(Read<bool>("_hasApproachNeighbors"), Is.EqualTo(eligibleNeighbor));
        Assert.That(controller.ApproachSeparationRadius, Is.EqualTo(1.5f), "Bypass must retain its original radius.");
        Assert.That(controller.ChaseSoftSeparationRadius, Is.EqualTo(2f));
    }

    [TestCase(1f)]
    [TestCase(1.5f)]
    [TestCase(1.75f)]
    public void MeleeStrength_IncreasesLocalContributionByHalfAtIdenticalDistance(float distance)
    {
        Feed(distance);
        Vector2 weightBefore = Read<Vector2>("_chaseSoftSeparation");
        Compute(out Vector2 oldRadial, out Vector2 oldTangent, meleePlayerChase: false);
        Compute(out Vector2 newRadial, out Vector2 newTangent);
        float oldRatio = oldTangent.y / oldRadial.x;
        float newRatio = newTangent.y / newRadial.x;
        Assert.That(oldRatio, Is.EqualTo(0.8f * 0.35f * (1f - distance / 2f)).Within(0.000001f));
        Assert.That(newRatio, Is.EqualTo(oldRatio * 1.5f).Within(0.000001f));
        Assert.That(newTangent.y, Is.GreaterThan(oldTangent.y));
        Assert.That((oldRadial + oldTangent).magnitude, Is.EqualTo(8f).Within(0.0001f));
        Assert.That((newRadial + newTangent).magnitude, Is.EqualTo(8f).Within(0.0001f));
        Assert.That(newRadial.x, Is.GreaterThanOrEqualTo(6.4f));
        Assert.That(Read<Vector2>("_chaseSoftSeparation"), Is.EqualTo(weightBefore), "The falloff feed must not be scaled or rewritten.");
    }

    [Test]
    public void StrongDensePreference_StillUsesExistingLateralCapAndForwardProgress()
    {
        Feed(0.1f);
        Compute(out Vector2 radial, out Vector2 tangent);
        Assert.That(tangent.y / radial.x, Is.EqualTo(0.35f).Within(0.000001f));
        Assert.That((radial + tangent).magnitude, Is.EqualTo(8f).Within(0.0001f));
        Assert.That(radial.x, Is.GreaterThanOrEqualTo(6.4f));
    }

    [Test]
    public void Multiplier_DoesNotScaleAngularSteeringOrHoldSeparation()
    {
        spread.Compute(Vector2.right * 8f, Vector2.right, Vector2.zero, false,
            Vector2.zero, 4f, 0.5f, 0f, 0.5f, true, 8f, out Vector2 oldRadial, out Vector2 oldTangent);
        spread.Compute(Vector2.right * 8f, Vector2.right, Vector2.zero, false,
            Vector2.zero, 4f, 0.5f, 0f, 0.5f, true, 8f, out Vector2 newRadial, out Vector2 newTangent,
            meleePlayerChase: true);
        Assert.That(newRadial, Is.EqualTo(oldRadial));
        Assert.That(newTangent, Is.EqualTo(oldTangent));
        Assert.That(oldTangent.sqrMagnitude, Is.GreaterThan(0f));
        Vector2 hold = spread.ComputeHoldSeparation(Vector2.right, Vector2.up, true, Vector2.zero, 8f);
        Assert.That(hold.y, Is.EqualTo(8f * 0.8f * 0.35f).Within(0.000001f));
    }

    [Test]
    public void EqualOuterNeighbors_CancelWithoutActivatingCloseNeighborFallback()
    {
        neighbor.transform.position = Vector2.down * 1.75f;
        var opposite = new GameObject("OppositeSoftNeighbor");
        try
        {
            opposite.transform.position = Vector2.up * 1.75f;
            FeedControllers(controller, other, opposite.AddComponent<EnemyController2D>());
            Assert.That(Read<Vector2>("_chaseSoftSeparation"), Is.EqualTo(Vector2.zero));
            Assert.IsFalse(Read<bool>("_hasApproachNeighbors"));
            Compute(out Vector2 radial, out Vector2 tangent);
            Assert.That(radial, Is.EqualTo(Vector2.right * 8f));
            Assert.That(tangent, Is.EqualTo(Vector2.zero));
        }
        finally { Object.DestroyImmediate(opposite); }
    }

    [TestCase(1f)]
    [TestCase(1.75f)]
    public void RangedSpacingInputs_RetainOriginalRadiusAndWeights(float distance)
    {
        subject.GetComponent<EnemyLocomotion>().Debug_SetHoldMovementPolicy(subject.AddComponent<EnemyRangedEngagement>());
        Feed(distance);
        Vector2 expected = Vector2.up * Mathf.Max(0f, 1f - distance / 1.5f);
        Assert.That(controller.ChaseSoftSeparationRadius, Is.EqualTo(1.5f));
        Assert.That(Read<Vector2>("_approachSeparation"), Is.EqualTo(expected));
        Assert.That(Read<Vector2>("_chaseSoftSeparation"), Is.EqualTo(expected));
    }

    [Test]
    public void HoldInput_RemainsLegacyWhileChaseInputIsAvailableSeparately()
    {
        var locomotion = subject.GetComponent<EnemyLocomotion>();
        locomotion.SetMovementState(EnemyController2D.State.HOLD);
        Feed(1.75f);
        Assert.That(Read<Vector2>("_approachSeparation"), Is.EqualTo(Vector2.zero));
        Assert.IsFalse(Read<bool>("_hasApproachNeighbors"));
        Assert.That(Read<Vector2>("_chaseSoftSeparation").y, Is.EqualTo(0.125f));
    }

    [Test]
    public void LegacyFixtureInjectionAndDisable_ClearBothSpacingFeeds()
    {
        controller.SetApproachSeparation(Vector2.up, true);
        Assert.That(Read<Vector2>("_chaseSoftSeparation"), Is.EqualTo(Vector2.up));
        typeof(EnemyController2D).GetMethod("OnDisable", PrivateInstance).Invoke(controller, null);
        Assert.That(Read<Vector2>("_chaseSoftSeparation"), Is.EqualTo(Vector2.zero));
        Assert.That(Read<Vector2>("_approachSeparation"), Is.EqualTo(Vector2.zero));
        Assert.IsFalse(Read<bool>("_hasApproachNeighbors"));
    }

    private void Feed(float distance)
    {
        neighbor.transform.position = Vector2.down * distance;
        FeedControllers(controller, other);
    }

    private static void FeedControllers(params EnemyController2D[] controllers)
    {
        var field = typeof(EnemyRingManager).GetField("sEnemies", PrivateStatic);
        object previous = field.GetValue(null);
        try
        {
            field.SetValue(null, controllers);
            typeof(EnemyRingManager).GetMethod("FeedApproachSeparation", PrivateStatic)
                .Invoke(null, new object[] { controllers.Length });
        }
        finally { field.SetValue(null, previous); }
    }

    private T Read<T>(string field) => (T)typeof(EnemyController2D).GetField(field, PrivateInstance).GetValue(controller);

    private void Compute(out Vector2 radial, out Vector2 tangent, bool meleePlayerChase = true)
    {
        spread.Compute(Vector2.right * 8f, Vector2.right, Read<Vector2>("_chaseSoftSeparation"),
            Read<bool>("_hasApproachNeighbors"), Vector2.up, 13f, 2.6f, 0f, 0f, false, 8f,
            out radial, out tangent, meleePlayerChase: meleePlayerChase);
    }
}
