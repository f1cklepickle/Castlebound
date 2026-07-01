using Castlebound.Gameplay.Inventory;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Inventory
{
    public class BackpackVaultTransferTests
    {
        [Test]
        public void TransferBackpackToVault_MovesEntriesAndClearsBackpack()
        {
            var go = CreateTransfer(out var backpack, out var vault, out var transfer);

            try
            {
                backpack.State.AddItem("weapon_sword", 1);
                backpack.State.AddItem("weapon_axe", 1);

                var transferred = transfer.TransferBackpackToVault();

                Assert.IsTrue(transferred);
                Assert.That(vault.State.GetCount("weapon_sword"), Is.EqualTo(1));
                Assert.That(vault.State.GetCount("weapon_axe"), Is.EqualTo(1));
                Assert.That(backpack.State.ItemCount, Is.EqualTo(0));
                Assert.That(backpack.State.EntryCount, Is.EqualTo(0));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void TransferBackpackToVault_EmptyBackpack_IsNoOp()
        {
            var go = CreateTransfer(out var backpack, out var vault, out var transfer);

            try
            {
                var transferred = transfer.TransferBackpackToVault();

                Assert.IsFalse(transferred);
                Assert.That(vault.State.EntryCount, Is.EqualTo(0));
                Assert.That(backpack.State.ItemCount, Is.EqualTo(0));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void TransferBackpackToVault_RepeatedAfterClear_DoesNotDuplicate()
        {
            var go = CreateTransfer(out var backpack, out var vault, out var transfer);

            try
            {
                backpack.State.AddItem("weapon_sword", 1);

                Assert.IsTrue(transfer.TransferBackpackToVault());
                Assert.IsFalse(transfer.TransferBackpackToVault());

                Assert.That(vault.State.GetCount("weapon_sword"), Is.EqualTo(1));
                Assert.That(backpack.State.ItemCount, Is.EqualTo(0));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void TransferBackpackToVault_LeavesActiveInventorySlotsUntouched()
        {
            var go = CreateTransfer(out var backpack, out var vault, out var transfer);
            var activeInventory = go.AddComponent<InventoryStateComponent>();

            try
            {
                activeInventory.State.AddWeapon("weapon_equipped");
                backpack.State.AddItem("weapon_backpack", 1);

                transfer.TransferBackpackToVault();

                Assert.That(activeInventory.State.GetWeaponId(0), Is.EqualTo("weapon_equipped"));
                Assert.That(vault.State.GetCount("weapon_backpack"), Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void Configure_AssignsSources()
        {
            var go = new GameObject("Transfer");
            var backpack = go.AddComponent<BackpackInventoryStateComponent>();
            var vault = go.AddComponent<CastleInventoryStateComponent>();
            var transfer = go.AddComponent<BackpackVaultTransfer>();

            try
            {
                transfer.Configure(backpack, vault);

                Assert.AreSame(backpack, transfer.BackpackSource);
                Assert.AreSame(vault, transfer.CastleInventorySource);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        private static GameObject CreateTransfer(
            out BackpackInventoryStateComponent backpack,
            out CastleInventoryStateComponent vault,
            out BackpackVaultTransfer transfer)
        {
            var go = new GameObject("PlayerInventory");
            backpack = go.AddComponent<BackpackInventoryStateComponent>();
            vault = go.AddComponent<CastleInventoryStateComponent>();
            transfer = go.AddComponent<BackpackVaultTransfer>();
            transfer.Configure(backpack, vault);
            return go;
        }
    }
}
