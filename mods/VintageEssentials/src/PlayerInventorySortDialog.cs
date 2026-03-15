using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace VintageEssentials
{
    public class PlayerInventorySortDialog
    {
        private ICoreClientAPI capi;
        private LockedSlotsManager lockedSlotsManager;

        public PlayerInventorySortDialog(ICoreClientAPI capi, LockedSlotsManager lockedSlotsManager)
        {
            this.capi = capi;
            this.lockedSlotsManager = lockedSlotsManager;
        }

        public void SortPlayerInventory()
        {
            IPlayer player = capi.World.Player;
            if (player?.InventoryManager == null) return;

            // Sort the backpack (bag storage) inventory.
            // hotBarInvClassName ("hotbar") is intentionally left in place.
            // characterInvClassName ("character") holds clothing/armor — never sort that.
            IInventory playerInv = player.InventoryManager.GetOwnInventory(GlobalConstants.backpackInvClassName);
            if (playerInv == null) return;

            // Collect the indices of non-empty, non-locked slots and clone their contents
            List<int> slotIndices = new List<int>();
            List<ItemStack> stacks = new List<ItemStack>();

            string playerUid = player.PlayerUID;
            HashSet<int> lockedSlots = lockedSlotsManager.GetLockedSlots(playerUid);

            for (int i = 0; i < playerInv.Count; i++)
            {
                ItemSlot slot = playerInv[i];
                if (slot != null && !slot.Empty && slot.Itemstack != null)
                {
                    if (!lockedSlots.Contains(i))
                    {
                        slotIndices.Add(i);
                        stacks.Add(slot.Itemstack.Clone());
                    }
                }
            }

            if (stacks.Count == 0)
            {
                capi.ShowChatMessage(Lang.Get("vintageessentials:sort-noitems"));
                return;
            }

            // Sort by name A-Z (null-safe)
            stacks = stacks.OrderBy(stack => stack.GetName() ?? "").ToList();

            // Assign sorted items directly back into their inventory slots by index.
            // Access slots via playerInv[index] so MarkDirty() correctly identifies
            // the slot as belonging to this inventory. No intermediate clear step —
            // each slot is overwritten atomically to prevent item loss on errors.
            for (int i = 0; i < slotIndices.Count && i < stacks.Count; i++)
            {
                playerInv[slotIndices[i]].Itemstack = stacks[i];
                playerInv[slotIndices[i]].MarkDirty();
            }

            capi.ShowChatMessage(Lang.Get("vintageessentials:sort-done"));
        }

        public void Dispose()
        {
            // Cleanup if needed
        }
    }
}
