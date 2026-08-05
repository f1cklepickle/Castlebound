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
    [SerializeField] private float repairCooldownSeconds = 1f;

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
    [SerializeField] private PlayerAttackLoop attackLoop;
    [SerializeField] private PlayerDefenseController defenseController;
    [SerializeField, Min(0)] private int baseAttackDamage = 1;
    [SerializeField] private float baseAttackRate = 1.5f;
    
    [Header("Movement")]
    [SerializeField] private PlayerMovementOrchestrator movementOrchestrator = new PlayerMovementOrchestrator();

    private Rigidbody2D rb;
    private Vector2 movementInput;
    private Vector2 aimInput;
    private PlayerCollisionMove2D mover;
    private InventoryState inventoryState;
    private bool inputLocked;
    private float repairCooldownRemaining;
    private readonly PlayerAttackRuntime attackRuntime = new PlayerAttackRuntime();

    public float RepairRange
    {
        get => _repairSensor != null ? _repairSensor.RepairRadius : 0f;
        set
        {
            if (_repairSensor == null)
            {
                _repairSensor = new RepairSensor();
            }

            _repairSensor.RepairRadius = value;
        }
    }

    public LayerMask RepairBarrierMask
    {
        get => _repairSensor != null ? _repairSensor.BarrierMask : default(LayerMask);
        set
        {
            EnsureRepairSensor();
            _repairSensor.BarrierMask = value;
        }
    }

    public float RepairCooldownSeconds
    {
        get => repairCooldownSeconds;
        set => repairCooldownSeconds = Mathf.Max(0f, value);
    }

    public float RepairCooldownRemaining => repairCooldownRemaining;
    public bool IsRepairOnCooldown => repairCooldownRemaining > 0f;
    public Vector2 CurrentFacingDirection => movementOrchestrator != null
        ? movementOrchestrator.LastFacingDirection
        : -(Vector2)transform.up;
    public int BaseAttackDamage
    {
        get => baseAttackDamage;
        set => baseAttackDamage = Mathf.Max(0, value);
    }

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
        if (attackLoop == null) attackLoop = GetComponent<PlayerAttackLoop>();
        if (defenseController == null) defenseController = GetComponent<PlayerDefenseController>();
        inventoryState = inventorySource != null ? inventorySource.State : null;
        if (weaponSlotSwapHandler == null) weaponSlotSwapHandler = new WeaponSlotSwapHandler();
        if (movementOrchestrator == null) movementOrchestrator = new PlayerMovementOrchestrator();
        if (fireInputController != null)
            fireInputController.Configure(null);
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
        if (inputLocked || (defenseController != null && !defenseController.CanAttack))
        {
            fireInputController?.ClearHeldFire();
            return;
        }

        fireInputController?.OnFirePressedStateChanged(value.isPressed);
    }

    void FixedUpdate()
    {
        TickRepairCooldown(Time.fixedDeltaTime);

        if (inputLocked)
            return;

        fireInputController?.Tick();

        if (attackLoop == null)
            attackLoop = GetComponent<PlayerAttackLoop>();

        bool canAttack = defenseController == null || defenseController.CanAttack;
        var isFireHeld = canAttack && fireInputController != null && fireInputController.IsFireHeld;
        attackRuntime.Tick(
            Time.fixedDeltaTime,
            baseAttackDamage,
            baseAttackRate,
            isFireHeld,
            playerWeaponController,
            attackLoop,
            attackAnimationDriver,
            animator,
            hitboxObject);

        float movementSpeedMultiplier = defenseController != null
            ? defenseController.MovementSpeedMultiplier
            : 1f;
        movementOrchestrator.Tick(
            mover,
            transform,
            movementInput,
            ResolveFacingInput(),
            Time.fixedDeltaTime,
            movementSpeedMultiplier);
    }

    /// <summary>
    /// Returns true if there is at least one damaged barrier within repair range.
    /// </summary>
    public bool HasRepairableBarrierInRange()
    {
        return _repairSensor != null && _repairSensor.HasRepairableBarrierInRange(transform.position);
    }

    public void OnRepair(InputValue value)
    {
        if (inputLocked)
            return;

        if (!value.isPressed)
            return;

        TryRepair();
    }

    public bool TryRepair()
    {
        if (inputLocked || IsRepairOnCooldown || _repairSensor == null)
        {
            return false;
        }

        if (!_repairSensor.TryRepairNearest(transform.position))
        {
            return false;
        }

        repairCooldownRemaining = repairCooldownSeconds;
        return true;
    }

    public void TickRepairCooldown(float deltaTime)
    {
        if (repairCooldownRemaining <= 0f)
        {
            return;
        }

        repairCooldownRemaining = Mathf.Max(0f, repairCooldownRemaining - Mathf.Max(0f, deltaTime));
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

    private void EnsureRepairSensor()
    {
        if (_repairSensor == null)
        {
            _repairSensor = new RepairSensor();
        }
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
        {
            ClearAttackInputState();
        }
    }

    public void ClearAttackInputState()
    {
        if (fireInputController == null)
            fireInputController = GetComponent<PlayerFireInputController>();

        if (attackLoop == null)
            attackLoop = GetComponent<PlayerAttackLoop>();

        fireInputController?.ClearHeldFire();
        attackRuntime.Reset(
            baseAttackDamage,
            baseAttackRate,
            playerWeaponController,
            attackLoop,
            attackAnimationDriver,
            animator,
            hitboxObject);
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

        var aimIntentActive = (fireInputController != null && fireInputController.IsFireHeld)
            || (defenseController != null && defenseController.IsGuarding);
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
