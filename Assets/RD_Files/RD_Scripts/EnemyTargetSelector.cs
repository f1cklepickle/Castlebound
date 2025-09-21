using UnityEngine;

[RequireComponent(typeof(EnemyController2D))]
public class EnemyTargetSelector : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";

    private EnemyController2D controller;
    private Transform player;

    void Awake() {
        controller = GetComponent<EnemyController2D>();
        var p = GameObject.FindGameObjectWithTag(playerTag);
        if (p) player = p.transform;
    }

    void OnEnable() {
        GateManager.onGatesChanged += Retarget;
        Retarget(); // initial pick
    }

    void OnDisable() {
        GateManager.onGatesChanged -= Retarget;
    }

    void Retarget() {
        // If any gate is open -> chase the player
        if (GateManager.AnyGateOpen()) {
            SetTarget(player);
            return;
        }
        // Otherwise -> nearest unbroken barrier segment
        var seg = GateManager.NearestClosedBarrier(transform.position);
        SetTarget(seg ? seg.transform : player);
    }

    void SetTarget(Transform t) {
        var f = typeof(EnemyController2D).GetField("target",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (f != null) f.SetValue(controller, t);
    }
}

