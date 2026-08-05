using System;
using Castlebound.Gameplay.Combat;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Health))]
public class PlayerDefenseController : MonoBehaviour, IPlayerHitReceiver
{
    [Header("Timing")]
    [SerializeField, Min(0f)] private float parryWindowDuration = 0.15f;
    [SerializeField, Min(0f)] private float recoveryDuration = 0.15f;

    [Header("Guard")]
    [SerializeField, Range(0f, 360f)] private float blockArcDegrees = 120f;
    [SerializeField, Range(0f, 1f)] private float guardingMovementMultiplier = 0.6f;

    [Header("References")]
    [SerializeField] private Health health;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private bool useReleaseFallbackPolling = true;

    private PlayerDefenseStateMachine stateMachine;
    private Func<bool> isDefenseStillPressedEvaluator;
    private bool releaseFallbackArmed;

    public PlayerDefenseState State => EnsureStateMachine().State;
    public bool IsGuarding => EnsureStateMachine().IsGuarding;
    public bool CanAttack => EnsureStateMachine().CanAttack;
    public float MovementSpeedMultiplier => IsGuarding ? guardingMovementMultiplier : 1f;
    public float BlockArcDegrees => blockArcDegrees;

    public event Action<PlayerDefenseState> StateChanged;
    public event Action<PlayerHitResult> HitResolved;

    private void Awake()
    {
        EnsureReferences();
        RebuildStateMachine();
    }

    private void FixedUpdate()
    {
        Tick(Time.fixedDeltaTime);
    }

    private void OnValidate()
    {
        parryWindowDuration = Mathf.Max(0f, parryWindowDuration);
        recoveryDuration = Mathf.Max(0f, recoveryDuration);
        blockArcDegrees = Mathf.Clamp(blockArcDegrees, 0f, 360f);
        guardingMovementMultiplier = Mathf.Clamp01(guardingMovementMultiplier);
    }

    public void OnDefend(InputValue value)
    {
        OnDefensePressedStateChanged(value.isPressed);
    }

    public void OnDefensePressedStateChanged(bool isPressed)
    {
        releaseFallbackArmed = isPressed;
        SetDefensePressed(isPressed);
    }

    public void SetDefensePressed(bool isPressed)
    {
        PlayerDefenseState previous = State;
        bool changed = isPressed
            ? stateMachine.BeginDefense()
            : stateMachine.ReleaseDefense();

        if (!changed)
            return;

        if (isPressed)
        {
            EnsureReferences();
            playerController?.ClearAttackInputState();
        }

        PublishStateChange(previous);
    }

    public void Tick(float deltaTime)
    {
        if (releaseFallbackArmed && IsGuarding && useReleaseFallbackPolling && !IsDefenseStillPressed())
        {
            releaseFallbackArmed = false;
            SetDefensePressed(false);
        }

        PlayerDefenseState previous = State;
        stateMachine.Advance(deltaTime);
        PublishStateChange(previous);
    }

    public PlayerHitResult ReceiveHit(PlayerHitRequest request)
    {
        EnsureReferences();
        PlayerHitResult result = PlayerDefenseHitResolver.Resolve(
            request,
            State,
            transform.position,
            ResolveFacingDirection(),
            blockArcDegrees);

        if (result.AppliedDamage > 0)
        {
            int appliedDamage = health != null ? health.ApplyDamage(result.AppliedDamage) : 0;
            result = new PlayerHitResult(request, result.Outcome, appliedDamage);
        }

        HitResolved?.Invoke(result);
        return result;
    }

    public void Configure(
        float parryWindowSeconds,
        float recoverySeconds,
        float arcDegrees,
        float movementMultiplier,
        Func<bool> pressedEvaluator = null)
    {
        parryWindowDuration = Mathf.Max(0f, parryWindowSeconds);
        recoveryDuration = Mathf.Max(0f, recoverySeconds);
        blockArcDegrees = Mathf.Clamp(arcDegrees, 0f, 360f);
        guardingMovementMultiplier = Mathf.Clamp01(movementMultiplier);
        isDefenseStillPressedEvaluator = pressedEvaluator;
        RebuildStateMachine();
    }

    private PlayerDefenseStateMachine EnsureStateMachine()
    {
        if (stateMachine == null)
            RebuildStateMachine();
        return stateMachine;
    }

    private void RebuildStateMachine()
    {
        stateMachine = new PlayerDefenseStateMachine(parryWindowDuration, recoveryDuration);
    }

    private void EnsureReferences()
    {
        if (health == null)
            health = GetComponent<Health>();
        if (playerController == null)
            playerController = GetComponent<PlayerController>();
    }

    private Vector2 ResolveFacingDirection()
    {
        return playerController != null
            ? playerController.CurrentFacingDirection
            : -(Vector2)transform.up;
    }

    private bool IsDefenseStillPressed()
    {
        if (isDefenseStillPressedEvaluator != null)
            return isDefenseStillPressedEvaluator();

        if (Mouse.current != null && Mouse.current.rightButton.isPressed)
            return true;

        foreach (Gamepad gamepad in Gamepad.all)
        {
            if (gamepad.leftTrigger.ReadValue() > 0.5f)
                return true;
        }

        return false;
    }

    private void PublishStateChange(PlayerDefenseState previous)
    {
        if (previous != State)
            StateChanged?.Invoke(State);
    }
}
