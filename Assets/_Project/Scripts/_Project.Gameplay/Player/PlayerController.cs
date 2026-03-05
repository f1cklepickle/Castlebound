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
        inventoryState = inventorySource != null ? inventorySource.State : null;
        if (weaponSlotSwapHandler == null) weaponSlotSwapHandler = new WeaponSlotSwapHandler();
        if (movementOrchestrator == null) movementOrchestrator = new PlayerMovementOrchestrator();
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
            return;

        // Guard against the release event: InputSystem calls OnFire for both
        // performed (press) and canceled (release). Only swing on press.
        if (!value.isPressed)
            return;

        if (!attackCooldownGate.TryConsume(Time.time, GetEffectiveAttackRate()))
            return;

        animator.SetTrigger("Attack");
    }

    void FixedUpdate()
    {
        if (inputLocked)
            return;

        movementOrchestrator.Tick(mover, transform, movementInput, aimInput, Time.fixedDeltaTime);
        SyncMobileAttackRate();
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
}
