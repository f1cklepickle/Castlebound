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

    private Rigidbody2D rb;
    private Vector2 movementInput;
    private Vector2 lastMoveDirection;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
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
        Vector2 movement = movementInput * moveSpeed;

        if (movement.magnitude < deadZone)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0;
        }
        else
        {
            rb.linearVelocity = movement;

            if (movementInput.magnitude > 0)
            {
                lastMoveDirection = movementInput;
            }

            Vector2 angleDirection = lastMoveDirection;
            if (flipDirection)
            {
                angleDirection *= -1;
            }

            float targetAngle = Mathf.Atan2(angleDirection.y, angleDirection.x) * Mathf.Rad2Deg + facingDirectionOffset;

            Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    public void EnableHitbox()
    {
        hitboxObject.GetComponent<Hitbox>()?.Activate();
    }

    public void DisableHitbox()
    {
        hitboxObject.GetComponent<Hitbox>()?.Deactivate();
    }

}