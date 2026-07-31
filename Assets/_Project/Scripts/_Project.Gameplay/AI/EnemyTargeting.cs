using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Castlebound.Gameplay.AI;

public class EnemyTargeting : MonoBehaviour
{
    [SerializeField] private Transform player;
    [FormerlySerializedAs("homeBarrier")]
    [SerializeField] private Transform selectedBarrier;
    [SerializeField] private float passThroughRadius = 0.6f;
    [SerializeField] private bool useBarrierTargeting = true;

    private bool committedThroughBrokenBarrier;
    private bool retargetRequested = true;
    private IReadOnlyList<BarrierHealth> barrierCandidatesOverride;

    public Transform Player => player;
    public Transform SelectedBarrier => selectedBarrier;
    public bool UsesBarrierTargeting => useBarrierTargeting;
    public Transform SteerTarget { get; private set; }
    public Transform AttackTarget { get; private set; }
    public EnemyTargetType CurrentTargetType { get; private set; } = EnemyTargetType.None;
    public int TargetRevision { get; private set; }

    public void Initialize()
    {
        EnsurePlayerReference();
        ApplyFallbackDecision();
        RequestRetarget();
    }

    public void RequestRetarget()
    {
        retargetRequested = true;
    }

    public void Refresh(Vector2 enemyPosition, bool playerInside, bool enemyInside)
    {
        EnsurePlayerReference();

        if (!retargetRequested && CurrentTargetType == EnemyTargetType.Barrier && IsAtBrokenSelectedBarrier(enemyPosition))
        {
            committedThroughBrokenBarrier = true;
            retargetRequested = true;
        }

        if (!retargetRequested && SteerTarget != null && AttackTarget != null)
            return;

        retargetRequested = false;
        TargetRevision++;

        if (!playerInside)
        {
            committedThroughBrokenBarrier = false;
        }
        else if (committedThroughBrokenBarrier && !enemyInside && !IsSelectedBarrierBroken())
        {
            committedThroughBrokenBarrier = false;
        }
        else if (IsAtBrokenSelectedBarrier(enemyPosition))
        {
            committedThroughBrokenBarrier = true;
        }

        if (committedThroughBrokenBarrier)
        {
            ApplyFallbackDecision();
            return;
        }

        if (!playerInside || enemyInside)
        {
            ApplyFallbackDecision();
            return;
        }

        selectedBarrier = useBarrierTargeting
            ? CastleTargetSelector.SelectNearestBarrier(
                enemyPosition,
                barrierCandidatesOverride ?? BarrierHealth.All)
            : null;

        if (IsAtBrokenSelectedBarrier(enemyPosition))
        {
            committedThroughBrokenBarrier = true;
            ApplyFallbackDecision();
            return;
        }

        EnemyTargetSelector.Decision decision = EnemyTargetSelector.Select(new EnemyTargetSelector.Input
        {
            EnemyPosition = enemyPosition,
            EnemyInside = enemyInside,
            PlayerInside = playerInside,
            Player = player,
            BarrierTarget = useBarrierTargeting ? selectedBarrier : null,
            PassThroughRadius = passThroughRadius
        });

        SteerTarget = decision.SteerTarget;
        AttackTarget = decision.AttackTarget;
        CurrentTargetType = decision.TargetType;
    }

    public void Debug_Setup(Transform playerReference, Transform selectedBarrierReference = null)
    {
        player = playerReference;
        selectedBarrier = selectedBarrierReference;
        BarrierHealth explicitBarrier = selectedBarrierReference != null
            ? selectedBarrierReference.GetComponent<BarrierHealth>()
            : null;
        barrierCandidatesOverride = explicitBarrier != null
            ? new[] { explicitBarrier }
            : null;
        ApplyFallbackDecision();
        RequestRetarget();
    }

    public void Debug_SetBarrierCandidates(IReadOnlyList<BarrierHealth> barriers)
    {
        barrierCandidatesOverride = barriers;
        RequestRetarget();
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

    private bool IsAtBrokenSelectedBarrier(Vector2 enemyPosition)
    {
        if (!useBarrierTargeting || selectedBarrier == null || passThroughRadius <= 0f)
            return false;

        if (!IsSelectedBarrierBroken())
            return false;

        return ((Vector2)selectedBarrier.position - enemyPosition).sqrMagnitude <=
               passThroughRadius * passThroughRadius;
    }

    private bool IsSelectedBarrierBroken()
    {
        if (!useBarrierTargeting || selectedBarrier == null)
            return false;

        BarrierHealth health = selectedBarrier.GetComponent<BarrierHealth>();
        return health != null && health.IsBroken;
    }

}
