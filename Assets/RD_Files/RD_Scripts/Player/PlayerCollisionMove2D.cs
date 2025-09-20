using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerCollisionMove2D : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private LayerMask solidMask;
    [SerializeField] private float skin = 0.02f;

    private Rigidbody2D _rb;
    private Collider2D _col;
    private Vector2 _input;

    // Reused, no per-frame allocations
    private static readonly RaycastHit2D[] sHits = new RaycastHit2D[8];

    public void SetMoveInput(Vector2 input)
    {
        // Clamp per-axis to [-1, 1] without allocating
        _input.x = Mathf.Clamp(input.x, -1f, 1f);
        _input.y = Mathf.Clamp(input.y, -1f, 1f);
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider2D>();

        if (_rb != null && _rb.bodyType != RigidbodyType2D.Kinematic)
            _rb.bodyType = RigidbodyType2D.Kinematic;
    }

    private void Reset()
    {
        var rb = GetComponent<Rigidbody2D>();
        if (rb) rb.bodyType = RigidbodyType2D.Kinematic;
    }

    private void FixedUpdate()
    {
        if (_rb == null || _col == null) return;

        float dt = Time.fixedDeltaTime;
        Vector2 delta = _input * moveSpeed * dt;

        // Prepare filter (stack struct, no allocation)
        ContactFilter2D filter = new ContactFilter2D();
        filter.useLayerMask = true;
        filter.layerMask = solidMask;
        filter.useTriggers = false;

        Vector2 pos = _rb.position;

        // X pass
        float dx = 0f;
        float ax = Mathf.Abs(delta.x);
        if (ax > 0f)
        {
            float signX = delta.x > 0f ? 1f : -1f;
            Vector2 dirX = new Vector2(signX, 0f);
            int countX = _col.Cast(dirX, filter, sHits, ax + skin);
            float allowX = ax;
            for (int i = 0; i < countX; i++)
            {
                float d = sHits[i].distance - skin;
                if (d < allowX) allowX = d < 0f ? 0f : d;
            }
            dx = signX * allowX;
        }

        // Y pass
        float dy = 0f;
        float ay = Mathf.Abs(delta.y);
        if (ay > 0f)
        {
            float signY = delta.y > 0f ? 1f : -1f;
            Vector2 dirY = new Vector2(0f, signY);
            int countY = _col.Cast(dirY, filter, sHits, ay + skin);
            float allowY = ay;
            for (int i = 0; i < countY; i++)
            {
                float d = sHits[i].distance - skin;
                if (d < allowY) allowY = d < 0f ? 0f : d;
            }
            dy = signY * allowY;
        }

        // Apply final clamped move
        _rb.MovePosition(pos + new Vector2(dx, dy));
    }
}
