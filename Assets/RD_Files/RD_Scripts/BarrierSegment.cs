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

    void BreakNow() {
        if (breakVfx) Instantiate(breakVfx, transform.position, Quaternion.identity);
        if (col) col.enabled = false;
        if (sr && brokenSprite) sr.sprite = brokenSprite;
        onBroken?.Invoke(this);
        if (destroyOnBreak) Destroy(gameObject);
    }
}

