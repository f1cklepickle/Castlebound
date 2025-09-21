using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class BarrierSegment : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private Sprite brokenSprite;
    [SerializeField] private GameObject breakVfx;
    [SerializeField] private bool destroyOnBreak = false;

    private int health;
    private Collider2D col;
    private SpriteRenderer sr;
    public bool IsBroken => health <= 0;
    public System.Action<BarrierSegment> onBroken; // used later by gate

    void Awake() {
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        health = Mathf.Max(1, maxHealth);
        gameObject.layer = LayerMask.NameToLayer("Walls");
        gameObject.tag = "Wall";
    }

    public void TakeDamage(int amount) {
        if (IsBroken) return;
        health = Mathf.Max(0, health - Mathf.Max(1, amount));
        if (IsBroken) BreakNow();
    }

    internal void BreakNow() {
        if (breakVfx) Instantiate(breakVfx, transform.position, Quaternion.identity);
        if (col) col.enabled = false;
        if (sr && brokenSprite) sr.sprite = brokenSprite;
        onBroken?.Invoke(this);
        if (destroyOnBreak) Destroy(gameObject);
    }

#if UNITY_EDITOR
    [ContextMenu("DEBUG: Break Now")]
    void DebugBreakNow()
    {
        if (!IsBroken) { health = 0; BreakNow(); }
    }
#endif

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        // Simple HP dot: green (unbroken) -> red (broken)
        Color c = IsBroken ? Color.red : Color.green;
        c.a = 0.9f;
        Gizmos.color = c;
        Gizmos.DrawSphere(transform.position, 0.08f);
    }
#endif
}
