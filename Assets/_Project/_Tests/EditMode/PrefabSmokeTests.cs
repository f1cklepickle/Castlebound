using NUnit.Framework;
using UnityEngine;

[Category("Smoke/Prefab")]
public class PrefabSmokeTests
{
    const string PlayerPath = "Assets/_Project/Prefabs/Player.prefab";
    const string EnemyPath  = "Assets/_Project/Prefabs/Enemy.prefab";

    // --- Player ---
    [Test] public void PlayerPrefab_Loads() {
        var go = PrefabTestUtil.Load(PlayerPath); PrefabTestUtil.Unload(go);
    }

    [Test] public void Player_Has_Rigidbody2D() {
        var go = PrefabTestUtil.Load(PlayerPath);
        Assert.NotNull(go.GetComponent<Rigidbody2D>());
        PrefabTestUtil.Unload(go);
    }

    [Test] public void Player_Has_Collider2D() {
        var go = PrefabTestUtil.Load(PlayerPath);
        Assert.NotNull(go.GetComponent<Collider2D>());
        PrefabTestUtil.Unload(go);
    }

    [Test] public void Player_Has_Animator() {
        var go = PrefabTestUtil.Load(PlayerPath);
        Assert.NotNull(go.GetComponent<Animator>());
        PrefabTestUtil.Unload(go);
    }

    [Test] public void Player_Has_PlayerController() {
        var go = PrefabTestUtil.Load(PlayerPath);
        Assert.NotNull(go.GetComponent<PlayerController>());
        PrefabTestUtil.Unload(go);
    }

    [Test] public void Player_Has_Health() {
        var go = PrefabTestUtil.Load(PlayerPath);
        Assert.NotNull(go.GetComponent<Health>());
        PrefabTestUtil.Unload(go);
    }

    [Test] public void Player_Has_HitboxChild() {
        var go = PrefabTestUtil.Load(PlayerPath);
        Assert.NotNull(go.GetComponentInChildren<Hitbox>(true), "Player must contain a Hitbox child.");
        PrefabTestUtil.Unload(go);
    }

    [Test] public void Player_Hitbox_IsTrigger() {
        var go = PrefabTestUtil.Load(PlayerPath);
        var hb = go.GetComponentInChildren<Hitbox>(true);
        Assert.NotNull(hb, "No Hitbox child found.");
        var col = hb.GetComponent<Collider2D>();
        Assert.NotNull(col, "Hitbox needs a Collider2D.");
        Assert.IsTrue(col.isTrigger, "Hitbox collider must be set to IsTrigger.");
        PrefabTestUtil.Unload(go);
    }

    // --- Enemy ---
    [Test] public void EnemyPrefab_Loads() {
        var go = PrefabTestUtil.Load(EnemyPath); PrefabTestUtil.Unload(go);
    }

    [Test] public void Enemy_Has_Rigidbody2D() {
        var go = PrefabTestUtil.Load(EnemyPath);
        Assert.NotNull(go.GetComponent<Rigidbody2D>());
        PrefabTestUtil.Unload(go);
    }

    [Test] public void Enemy_Has_Collider2D() {
        var go = PrefabTestUtil.Load(EnemyPath);
        Assert.NotNull(go.GetComponent<Collider2D>());
        PrefabTestUtil.Unload(go);
    }

    [Test] public void Enemy_Has_Controller() {
        var go = PrefabTestUtil.Load(EnemyPath);
        Assert.NotNull(go.GetComponent<EnemyController2D>());
        PrefabTestUtil.Unload(go);
    }

    [Test] public void Enemy_Has_Attack() {
        var go = PrefabTestUtil.Load(EnemyPath);
        Assert.NotNull(go.GetComponent<EnemyAttack>());
        PrefabTestUtil.Unload(go);
    }

    [Test] public void Enemy_Has_Health() {
        var go = PrefabTestUtil.Load(EnemyPath);
        Assert.NotNull(go.GetComponent<Health>());
        PrefabTestUtil.Unload(go);
    }
}
