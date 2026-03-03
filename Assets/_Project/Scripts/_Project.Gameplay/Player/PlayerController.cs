using UnityEngine;
using UnityEngine.InputSystem;
using Castlebound.Gameplay.Inventory;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float deadZone = 0.1f;
    public float rotationSpeed = 720f;
    public float facingDirectionOffset = -90f;
    public bool flipDirection = false;

    public Animator animator;
    public GameObject hitboxObject;

    [Header("Repair")]
    [SerializeField] private RepairSensor _repairSensor;

    [Header("Potions")]
    [SerializeField] private PotionUseController potionUseController;

    [Header("Weapons")]
    [SerializeField] private InventoryStateComponent inventorySource;
    [SerializeField] private float weaponSlotSwapCooldown = 0.5f;

    private Rigidbody2D rb;
    private Vector2 movementInput;
    private Vector2 aimInput;
    private Vector2 lastMoveDirection;

  private PlayerCollisionMove2D mover;
    private float lastWeaponSlotSwapTime = float.NegativeInfinity;
    private InventoryState inventoryState;
    private bool inputLocked;

void Awake() {
    rb = GetComponent<Rigidbody2D>();
    if (animator == null) animator = GetComponent<Animator>();
    mover = GetComponent<PlayerCollisionMove2D>();   // NEW
    if (potionUseController == null) potionUseController = GetComponent<PotionUseController>();
    if (inventorySource == null) inventorySource = GetComponent<InventoryStateComponent>();
    inventoryState = inventorySource != null ? inventorySource.State : null;
    lastMoveDirection = new Vector2(0, 1);
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

        animator.SetTrigger("Attack");
    }

   void FixedUpdate()
{
    if (inputLocked)
    {
        return;
    }

    // Pass input to the collision-clamped mover
    if (mover != null)
    {
        // deadzone
        var mag = movementInput.magnitude;
        mover.SetMoveInput(mag < deadZone ? Vector2.zero : movementInput);
    }

    // Aim/rotate: prefer explicit aim input (right stick/touch zone) when active,
    // otherwise fall back to movement direction.
    if (movementInput.magnitude > 0f)
        lastMoveDirection = movementInput;

    Vector2 facingSource = aimInput.magnitude > deadZone ? aimInput : lastMoveDirection;
    Vector2 angleDirection = flipDirection ? -facingSource : facingSource;
    float targetAngle = Mathf.Atan2(angleDirection.y, angleDirection.x) * Mathf.Rad2Deg + facingDirectionOffset;
    var targetRotation = Quaternion.Euler(0, 0, targetAngle);
    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
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
        if (Mathf.Approximately(scrollDelta, 0f))
            return;

        if (inventoryState == null)
        {
            inventorySource = inventorySource != null ? inventorySource : GetComponent<InventoryStateComponent>();
            inventoryState = inventorySource != null ? inventorySource.State : null;
        }

        if (inventoryState == null)
            return;

        if (time - lastWeaponSlotSwapTime < weaponSlotSwapCooldown)
            return;

        int nextIndex = inventoryState.ActiveWeaponSlotIndex == 0 ? 1 : 0;
        if (inventoryState.SetActiveWeaponSlot(nextIndex))
        {
            lastWeaponSlotSwapTime = time;
        }
    }

    public bool TrySwapWeaponSlotWithoutCooldown()
    {
        if (inventoryState == null)
        {
            inventorySource = inventorySource != null ? inventorySource : GetComponent<InventoryStateComponent>();
            inventoryState = inventorySource != null ? inventorySource.State : null;
        }

        if (inventoryState == null)
            return false;

        int nextIndex = inventoryState.ActiveWeaponSlotIndex == 0 ? 1 : 0;
        return inventoryState.SetActiveWeaponSlot(nextIndex);
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
}
