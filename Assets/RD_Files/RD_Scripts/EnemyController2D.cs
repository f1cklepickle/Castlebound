// Assets/Scripts/Enemies/EnemyController2D.cs
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController2D : MonoBehaviour
{
    [Header("Chase Tuning")]
    [SerializeField] float moveSpeed = 2.5f;
    [SerializeField] float stopDistance = 1.1f;          // stop when <= this
    [SerializeField] float resumeDistance = 1.6f;         // start moving again when > this
    [SerializeField] float maxChaseDistance = 25f;        // leash

    [Header("Ring & Separation")]
    [SerializeField] float desiredRingRadius = 1.25f;     // target circle around player
    [SerializeField] float tangentialStrength = 0.6f;     // sideways slide
    [SerializeField] float separationRadius = 0.8f;       // neighbor radius
    [SerializeField] float separationStrength = 1.1f;     // neighbor push
    [SerializeField] LayerMask enemiesMask;               // set to "Enemies" layer in Inspector

    [Header("Visuals")]
    [SerializeField] Transform spriteChild;               // rotate this child (keep RB2D Z frozen)
    [SerializeField] float facingOffset = -90f;           // set for your art orientation
    [SerializeField] float faceSmoothTime = 0.08f;        // smooth facing (seconds)
    [SerializeField] Animator animator;

    Rigidbody2D rb;
    Transform target;
    bool inHoldRange;

    static readonly Collider2D[] overlapBuf = new Collider2D[16];
    float faceVel; // for SmoothDampAngle

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (!animator) animator = GetComponentInChildren<Animator>();
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p) target = p.transform;
    }

    void FixedUpdate()
    {
        if (!target) return;

        Vector2 pos = rb.position;
        Vector2 toTarget = (Vector2)target.position - pos;
        float dist = toTarget.magnitude;

        // leash
        if (dist > maxChaseDistance) { StopMoving(); return; }

        // --- hysteresis to prevent jitter ---
        if (inHoldRange)
        {
            if (dist > resumeDistance) inHoldRange = false;
            StopMoving();
            SmoothFace(toTarget);
            return;
        }
        else if (dist <= stopDistance)
        {
            inHoldRange = true;
            StopMoving();
            SmoothFace(toTarget);
            return;
        }

        // --- steering (ring + tangential + separation) ---
        Vector2 dirToPlayer = (dist > 0.0001f) ? toTarget / dist : Vector2.right;

        // 1) ring goal
        Vector2 desiredPos = (Vector2)target.position - dirToPlayer * desiredRingRadius;
        Vector2 ringDir = (desiredPos - pos);
        if (ringDir.sqrMagnitude > 0.0001f) ringDir.Normalize();

        // 2) tangential slide (fade out near stop radius)
        float holdT = Mathf.InverseLerp(stopDistance, resumeDistance, dist); // 0 near stop, 1 near resume
        Vector2 tangent = new Vector2(-dirToPlayer.y, dirToPlayer.x);
        Vector2 tangential = tangent * (tangentialStrength * holdT);

        // 3) soft separation (inverse-square, clamped)
        Collider2D[] hits = Physics2D.OverlapCircleAll(pos, separationRadius, enemiesMask);
        Vector2 separation = Vector2.zero;
        for (int i = 0; i < hits.Length; i++)
        {
            var c = hits[i];
            if (!c || c.attachedRigidbody == rb) continue;  // skip self
            Vector2 away = pos - (Vector2)c.transform.position;
            float d = away.magnitude;
            if (d > 0.0001f) separation += away / (d * d);
        }
        if (separation.sqrMagnitude > 1f) separation = separation.normalized;
        separation *= separationStrength;

        // tiny forward bias so they don't orbit forever
        Vector2 forwardBias = dirToPlayer * 0.25f;

        // combine & move
        Vector2 steer = ringDir + tangential + separation + forwardBias;
        if (steer.sqrMagnitude > 0.0001f) steer.Normalize();

        Vector2 step = steer * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(pos + step);

        SmoothFace(step.sqrMagnitude > 0.0001f ? steer : dirToPlayer);
        SetAnim(step.sqrMagnitude);
    }

    void StopMoving()
    {
        rb.linearVelocity = Vector2.zero;     // kill drift so they truly hold position
        SetAnim(0f);
    }

    void SmoothFace(Vector2 dir)
    {
        if (!spriteChild || dir.sqrMagnitude < 0.0001f) return;
        float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + facingOffset;
        float current = spriteChild.eulerAngles.z;
        float smoothed = Mathf.SmoothDampAngle(current, targetAngle, ref faceVel, faceSmoothTime);
        spriteChild.rotation = Quaternion.Euler(0, 0, smoothed);
    }

    void SetAnim(float moveAmount)
    {
        if (animator) animator.SetFloat("MoveSpeed", moveAmount);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, stopDistance);
        Gizmos.color = Color.green; Gizmos.DrawWireSphere(transform.position, resumeDistance);
        Gizmos.color = Color.cyan; Gizmos.DrawWireSphere(transform.position, separationRadius);
    }
}
