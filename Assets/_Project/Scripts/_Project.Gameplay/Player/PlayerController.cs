using UnityEngine;
using UnityEngine.InputSystem;
using Castlebound.Gameplay.Inventory;
using Castlebound.Gameplay.Combat;
using Castlebound.Gameplay.Input;

public class PlayerController : MonoBehaviour
{
    public Animator animator;
    public GameObject hitboxObject;

    [Header("Repair")]
    [SerializeField] private RepairSensor _repairSensor;

    [Header("Potions")]
    [SerializeField] private PotionUseController potionUseController;

    [Header("Weapons")]
    [SerializeField] private InventoryStateComponent inventorySource;
    [SerializeField] private WeaponSlotSwapHandler weaponSlotSwapHandler = new WeaponSlotSwapHandler();
    [SerializeField] private PlayerWeaponController playerWeaponController;
    [SerializeField] private MobileInputDriver mobileInputDriver;
    [SerializeField] private PlayerFireInputController fireInputController;
    [SerializeField] private PlayerAimInputResolver aimInputResolver;
    [SerializeField] private PlayerFacingPolicyResolver facingPolicyResolver;
    [SerializeField] private PlayerAttackAnimationDriver attackAnimationDriver;
    [SerializeField] private float baseAttackRate = 1.5f;
    
    [Header("Movement")]
    [SerializeField] private PlayerMovementOrchestrator movementOrchestrator = new PlayerMovementOrchestrator();

    private Rigidbody2D rb;
    private Vector2 movementInput;
    private Vector2 aimInput;
    private PlayerCollisionMove2D mover;
    private InventoryState inventoryState;
    private bool inputLocked;
    private readonly PlayerAttackCooldownGate attackCooldownGate = new PlayerAttackCooldownGate();
    private float appliedMobileAttackRate = -1f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponent<Animator>();
        mover = GetComponent<PlayerCollisionMove2D>();
        if (potionUseController == null) potionUseController = GetComponent<PotionUseController>();
        if (inventorySource == null) inventorySource = GetComponent<InventoryStateComponent>();
        if (playerWeaponController == null) playerWeaponController = GetComponent<PlayerWeaponController>();
        if (mobileInputDriver == null) mobileInputDriver = FindObjectOfType<MobileInputDriver>();
        if (fireInputController == null) fireInputController = GetComponent<PlayerFireInputController>();
        if (aimInputResolver == null) aimInputResolver = GetComponent<PlayerAimInputResolver>();
        if (facingPolicyResolver == null) facingPolicyResolver = GetComponent<PlayerFacingPolicyResolver>();
        if (attackAnimationDriver == null) attackAnimationDriver = GetComponent<PlayerAttackAnimationDriver>();
        inventoryState = inventorySource != null ? inventorySource.State : null;
        if (weaponSlotSwapHandler == null) weaponSlotSwapHandler = new WeaponSlotSwapHandler();
        if (movementOrchestrator == null) movementOrchestrator = new PlayerMovementOrchestrator();
        if (fireInputController != null)
            fireInputController.Configure(TryTriggerAttack);
        SyncMobileAttackRate();
    }


    public void OnMove(InputValue value)
    {
        if (inputLocked)
        {
            return;
        }

        movementInput = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        if (inputLocked)
            return;

        aimInput = value.Get<Vector2>();
    }

    public void OnFire(InputValue value)
    {
        if (inputLocked)
        {
            fireInputController?.ClearHeldFire();
            return;
        }

        fireInputController?.OnFirePressedStateChanged(value.isPressed);

        if (!value.isPressed)
            return;

        TryTriggerAttack();
    }

    void FixedUpdate()
    {
        if (inputLocked)
            return;

        fireInputController?.Tick();

        movementOrchestrator.Tick(mover, transform, movementInput, ResolveFacingInput(), Time.fixedDeltaTime);
        SyncMobileAttackRate();
        SyncAttackAnimationSpeed();
    }


    public void EnableHitbox()
    {
        hitboxObject.GetComponent<Hitbox>()?.Activate();
    }

    public void DisableHitbox()
    {
        hitboxObject.GetComponent<Hitbox>()?.Deactivate();
    }

    /// <summary>
    /// Returns true if there is at least one broken barrier within repair range.
    /// </summary>
    public bool HasRepairableBarrierInRange()
    {
        return _repairSensor.HasRepairableBarrierInRange(transform.position);
    }

    public void OnRepair(InputValue value)
    {
        if (inputLocked)
            return;

        if (!value.isPressed)
            return;

        _repairSensor.TryRepairNearest(transform.position);
    }

    public void OnUsePotion(InputValue value)
    {
        if (inputLocked)
            return;

        if (!value.isPressed)
            return;

        potionUseController?.TryConsume();
    }

    public void OnSwapWeaponSlot(InputValue value)
    {
        if (inputLocked)
            return;

        var scroll = value.Get<Vector2>().y;
        HandleWeaponSlotSwap(scroll, Time.time);
    }

    public void HandleWeaponSlotSwap(float scrollDelta, float time)
    {
        if (!TryEnsureInventoryState())
            return;

        weaponSlotSwapHandler.HandleWeaponSlotSwap(scrollDelta, time, inventoryState);
    }

    public bool TrySwapWeaponSlotWithoutCooldown()
    {
        if (!TryEnsureInventoryState())
            return false;

        return weaponSlotSwapHandler.TrySwapWeaponSlotWithoutCooldown(inventoryState);
    }

    private bool TryEnsureInventoryState()
    {
        if (inventoryState != null)
            return true;

        inventorySource = inventorySource != null ? inventorySource : GetComponent<InventoryStateComponent>();
        inventoryState = inventorySource != null ? inventorySource.State : null;
        return inventoryState != null;
    }

    public void StopMovement()
    {
        movementInput = Vector2.zero;
        if (mover != null)
        {
            mover.SetMoveInput(Vector2.zero);
        }

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
    }

    public void SetInputLocked(bool locked)
    {
        inputLocked = locked;
        if (locked)
            fireInputController?.ClearHeldFire();
    }

    private float GetEffectiveAttackRate()
    {
        var weaponAttackSpeed = playerWeaponController != null
            ? playerWeaponController.CurrentWeaponStats.AttackSpeed
            : 1f;

        return PlayerAttackRateCalculator.ComputeEffectiveRate(baseAttackRate, weaponAttackSpeed);
    }

    private void SyncMobileAttackRate()
    {
        if (mobileInputDriver == null)
            return;

        var rate = GetEffectiveAttackRate();
        if (Mathf.Approximately(rate, appliedMobileAttackRate))
            return;

        mobileInputDriver.SetAttackRate(rate);
        appliedMobileAttackRate = rate;
    }

    private void SyncAttackAnimationSpeed()
    {
        if (attackAnimationDriver == null)
            attackAnimationDriver = GetComponent<PlayerAttackAnimationDriver>();

        if (attackAnimationDriver == null)
            return;

        attackAnimationDriver.ApplyAttackSpeed(animator, GetEffectiveAttackRate(), baseAttackRate);
    }

    private bool TryTriggerAttack()
    {
        if (!attackCooldownGate.TryConsume(Time.time, GetEffectiveAttackRate()))
            return false;

        animator.SetTrigger("Attack");
        return true;
    }

    private Vector2 ResolveAimInput()
    {
        if (aimInputResolver == null)
            aimInputResolver = GetComponent<PlayerAimInputResolver>();

        return aimInputResolver != null
            ? aimInputResolver.Resolve(transform.position, aimInput)
            : aimInput;
    }

    private Vector2 ResolveFacingInput()
    {
        var resolvedAimInput = ResolveAimInput();
        if (facingPolicyResolver == null)
            facingPolicyResolver = GetComponent<PlayerFacingPolicyResolver>();

        if (facingPolicyResolver == null)
            return resolvedAimInput;

        var aimIntentActive = fireInputController != null && fireInputController.IsFireHeld;
        var currentFacing = movementOrchestrator != null ? movementOrchestrator.LastMoveDirection : (Vector2)transform.up;
        return facingPolicyResolver.ResolveFacing(
            currentFacing,
            movementInput,
            resolvedAimInput,
            aimInput,
            aimIntentActive,
            Time.fixedDeltaTime);
    }
}
