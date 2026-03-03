using System;
using Castlebound.Gameplay.Inventory;
using UnityEngine;

[Serializable]
public class WeaponSlotSwapHandler
{
    [SerializeField] private float weaponSlotSwapCooldown = 0.5f;
    private float lastWeaponSlotSwapTime = float.NegativeInfinity;

    public bool HandleWeaponSlotSwap(float scrollDelta, float time, InventoryState inventoryState)
    {
        if (Mathf.Approximately(scrollDelta, 0f))
            return false;

        if (inventoryState == null)
            return false;

        if (time - lastWeaponSlotSwapTime < weaponSlotSwapCooldown)
            return false;

        int nextIndex = inventoryState.ActiveWeaponSlotIndex == 0 ? 1 : 0;
        if (!inventoryState.SetActiveWeaponSlot(nextIndex))
            return false;

        lastWeaponSlotSwapTime = time;
        return true;
    }

    public bool TrySwapWeaponSlotWithoutCooldown(InventoryState inventoryState)
    {
        if (inventoryState == null)
            return false;

        int nextIndex = inventoryState.ActiveWeaponSlotIndex == 0 ? 1 : 0;
        return inventoryState.SetActiveWeaponSlot(nextIndex);
    }
}
