using UnityEngine;
using UnityEngine.InputSystem;

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
    [SerializeField] private float repairRadius = 1.5f;
    [SerializeField] private LayerMask barrierMask;

    private Rigidbody2D rb;
    private Vector2 movementInput;
    private Vector2 lastMoveDirection;

  private PlayerCollisionMove2D mover;

void Awake() {
    rb = GetComponent<Rigidbody2D>();
    if (animator == null) animator = GetComponent<Animator>();
    mover = GetComponent<PlayerCollisionMove2D>();   // NEW
    lastMoveDirection = new Vector2(0, 1);
}


    public void OnMove(InputValue value)
    {
        movementInput = value.Get<Vector2>();
    }

    public void OnFire(InputValue value)
    {
        animator.SetTrigger("Attack");
    }

   void FixedUpdate()
{
    // Pass input to the collision-clamped mover
    if (mover != null)
    {
        // deadzone
        var mag = movementInput.magnitude;
        mover.SetMoveInput(mag < deadZone ? Vector2.zero : movementInput);
    }

    // Aim/rotate same as before
    if (movementInput.magnitude > 0f)
        lastMoveDirection = movementInput;

    Vector2 angleDirection = flipDirection ? -lastMoveDirection : lastMoveDirection;
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

    public void OnRepair(InputValue value)
    {
        // We only care about the press event.
        if (!value.isPressed)
            return;

        // Find any barriers within repairRadius on the configured barrierMask.
        var hits = Physics2D.OverlapCircleAll(transform.position, repairRadius, barrierMask);
        if (hits == null || hits.Length == 0)
            return;

        for (int i = 0; i < hits.Length; i++)
        {
            var hit = hits[i];
            if (!hit) continue;

            // Look for BarrierHealth on this object or its parent.
            var barrier = hit.GetComponentInParent<BarrierHealth>();
            if (barrier == null)
                continue;

            // Only repair broken barriers.
            if (!barrier.IsBroken)
                continue;

            barrier.Repair();
            break; // Repair one barrier per key press.
        }
    }

}
