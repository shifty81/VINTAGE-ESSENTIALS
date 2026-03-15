using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace VintageEssentials
{
    public class InventoryLockDialog : GuiDialogGeneric
    {
        private new ICoreClientAPI capi;
        private LockedSlotsManager lockedSlotsManager;
        private ModConfig config;
        private bool lockingMode = false;
        private Action<bool> onModeChanged;

        public InventoryLockDialog(ICoreClientAPI capi, LockedSlotsManager lockedSlotsManager, ModConfig config, Action<bool> onModeChanged) 
            : base(Lang.Get("vintageessentials:lock-mode-on"), capi)
        {
            this.capi = capi;
            this.lockedSlotsManager = lockedSlotsManager;
            this.config = config;
            this.onModeChanged = onModeChanged;
        }

        public bool IsLockingMode()
        {
            return lockingMode;
        }

        public void ToggleLockingMode()
        {
            lockingMode = !lockingMode;
            onModeChanged?.Invoke(lockingMode);
            
            if (lockingMode)
            {
                if (capi?.World?.Player == null) return;
                string playerUid = capi.World.Player.PlayerUID;
                int lockedCount = lockedSlotsManager.GetLockedSlotsCount(playerUid);
                int maxSlots = config.MaxLockedSlots;
                capi.ShowChatMessage(Lang.Get("vintageessentials:lock-enabled", lockedCount, maxSlots));
            }
            else
            {
                capi.ShowChatMessage(Lang.Get("vintageessentials:lock-disabled"));
            }
        }

        public bool TryToggleSlotLock(int slotId)
        {
            if (!lockingMode) return false;
            if (capi?.World?.Player == null) return false;

            string playerUid = capi.World.Player.PlayerUID;

            // Check if the slot is currently locked
            bool wasLocked = lockedSlotsManager.IsSlotLocked(playerUid, slotId);

            // ToggleSlotLock returns true if the slot is now locked, false otherwise.
            // It also returns false when the max locked slots limit is reached
            // and the slot could not be locked.
            bool nowLocked = lockedSlotsManager.ToggleSlotLock(playerUid, slotId);
            
            if (nowLocked)
            {
                capi.ShowChatMessage(Lang.Get("vintageessentials:lock-slot-locked", slotId));
            }
            else if (wasLocked)
            {
                // Was locked before, now unlocked
                capi.ShowChatMessage(Lang.Get("vintageessentials:lock-slot-unlocked", slotId));
            }
            else
            {
                // Was not locked, and still not locked — max slots reached
                capi.ShowChatMessage(Lang.Get("vintageessentials:lock-max-reached", config.MaxLockedSlots));
                return false;
            }
            
            return true;
        }

        public bool IsSlotLocked(int slotId)
        {
            if (capi?.World?.Player == null) return false;
            string playerUid = capi.World.Player.PlayerUID;
            return lockedSlotsManager.IsSlotLocked(playerUid, slotId);
        }
    }
}
