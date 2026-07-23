using UnityEngine;
using Castlebound.Gameplay.AI;

public class EnemyTargeting : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform homeBarrier;
    [SerializeField] private float passThroughRadius = 0.6f;
    [SerializeField] private bool useBarrierTargeting = true;

    private bool committedThroughBrokenBarrier;

    public Transform Player => player;
    public Transform HomeBarrier => homeBarrier;
    public bool UsesBarrierTargeting => useBarrierTargeting;
    public Transform SteerTarget { get; private set; }
    public Transform AttackTarget { get; private set; }
    public EnemyTargetType CurrentTargetType { get; private set; } = EnemyTargetType.None;

    public void Initialize()
    {
        EnsurePlayerReference();
        ApplyFallbackDecision();
    }

    public void AssignHomeBarrierIfNeeded(Vector2 enemyPosition)
    {
        if (!useBarrierTargeting || homeBarrier != null)
            return;

        homeBarrier = CastleTargetSelector.AssignHomeBarrier(enemyPosition, GetAllBarrierTransforms());
    }

    public void Refresh(Vector2 enemyPosition, bool playerInside, bool enemyInside)
    {
        EnsurePlayerReference();
        AssignHomeBarrierIfNeeded(enemyPosition);

        if (!playerInside)
        {
            committedThroughBrokenBarrier = false;
        }
        else if (IsAtBrokenHomeBarrier(enemyPosition))
        {
            committedThroughBrokenBarrier = true;
        }

        if (committedThroughBrokenBarrier)
        {
            ApplyFallbackDecision();
            return;
        }

        EnemyTargetSelector.Decision decision = EnemyTargetSelector.Select(new EnemyTargetSelector.Input
        {
            EnemyPosition = enemyPosition,
            EnemyInside = enemyInside,
            PlayerInside = playerInside,
            Player = player,
            HomeBarrier = useBarrierTargeting ? homeBarrier : null,
            PassThroughRadius = passThroughRadius
        });

        SteerTarget = decision.SteerTarget;
        AttackTarget = decision.AttackTarget;
        CurrentTargetType = decision.TargetType;
    }

    public void Debug_Setup(Transform playerReference, Transform homeBarrierReference = null)
    {
        player = playerReference;
        homeBarrier = homeBarrierReference;
        ApplyFallbackDecision();
    }

    public void Debug_SetDecision(Transform steer, Transform attack, EnemyTargetType targetType)
    {
        SteerTarget = steer;
        AttackTarget = attack;
        CurrentTargetType = targetType;
    }

    public void Debug_SetUseBarrierTargeting(bool value)
    {
        useBarrierTargeting = value;
    }

    private void ApplyFallbackDecision()
    {
        SteerTarget = player;
        AttackTarget = player;
        CurrentTargetType = player != null ? EnemyTargetType.Player : EnemyTargetType.None;
    }

    private void EnsurePlayerReference()
    {
        if (player != null)
            return;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            return;
        }

        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
            player = playerController.transform;
    }

    private bool IsAtBrokenHomeBarrier(Vector2 enemyPosition)
    {
        if (!useBarrierTargeting || homeBarrier == null || passThroughRadius <= 0f)
            return false;

        BarrierHealth health = homeBarrier.GetComponent<BarrierHealth>();
        if (health == null || !health.IsBroken)
            return false;

        return ((Vector2)homeBarrier.position - enemyPosition).sqrMagnitude <=
               passThroughRadius * passThroughRadius;
    }

    private static Transform[] GetAllBarrierTransforms()
    {
        var barriers = BarrierHealth.All;
        if (barriers == null || barriers.Count == 0)
            return System.Array.Empty<Transform>();

        var result = new Transform[barriers.Count];
        for (int i = 0; i < barriers.Count; i++)
            result[i] = barriers[i] != null ? barriers[i].transform : null;

        return result;
    }
}
