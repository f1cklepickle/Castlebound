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

    [Test] public void Player_RepairCooldown_IsAuthored() {
        var go = PrefabTestUtil.Load(PlayerPath);
        var controller = go.GetComponent<PlayerController>();
        Assert.NotNull(controller);
        Assert.That(controller.RepairCooldownSeconds, Is.EqualTo(1f).Within(0.001f));
        PrefabTestUtil.Unload(go);
    }

    [Test] public void Player_Has_BackpackInventory() {
        var go = PrefabTestUtil.Load(PlayerPath);
        var backpack = go.GetComponent<Castlebound.Gameplay.Inventory.BackpackInventoryStateComponent>();
        Assert.NotNull(backpack);
        Assert.That(backpack.MaxItemCount, Is.EqualTo(8));
        PrefabTestUtil.Unload(go);
    }

    [Test] public void Player_Has_BackpackVaultTransfer() {
        var go = PrefabTestUtil.Load(PlayerPath);
        var transfer = go.GetComponent<Castlebound.Gameplay.Inventory.BackpackVaultTransfer>();
        Assert.NotNull(transfer);
        Assert.NotNull(transfer.BackpackSource);
        Assert.NotNull(transfer.CastleInventorySource);
        PrefabTestUtil.Unload(go);
    }

    [Test] public void Player_WeaponResolver_ResolvesPrototypeWeapons() {
        var go = PrefabTestUtil.Load(PlayerPath);
        var resolver = go.GetComponent<Castlebound.Gameplay.Combat.WeaponDefinitionResolverComponent>();
        Assert.NotNull(resolver);
        Assert.That(resolver.Resolve("weapon_sword")?.Damage, Is.EqualTo(5));
        Assert.That(resolver.Resolve("weapon_iron_club")?.Damage, Is.EqualTo(5));
        Assert.That(resolver.Resolve("weapon_club")?.Damage, Is.EqualTo(3));
        Assert.That(resolver.Resolve("weapon_rusty_dagger")?.Damage, Is.EqualTo(3));
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
