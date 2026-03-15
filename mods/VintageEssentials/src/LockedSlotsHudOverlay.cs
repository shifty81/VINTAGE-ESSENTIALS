using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace VintageEssentials
{
    /// <summary>
    /// HUD overlay that displays locked slot indicators on the player inventory
    /// </summary>
    public class LockedSlotsHudOverlay : HudElement
    {
        private LockedSlotsManager lockedSlotsManager;
        private LockedSlotRenderer renderer;
        private LoadedTexture lockedSlotTexture;
        private GuiDialog characterDialog;

        public LockedSlotsHudOverlay(ICoreClientAPI capi, LockedSlotsManager lockedSlotsManager) : base(capi)
        {
            this.lockedSlotsManager = lockedSlotsManager;
            this.renderer = new LockedSlotRenderer(capi, lockedSlotsManager);
            
            // Create the locked slot texture (60x60 is a typical slot size in Vintage Story)
            this.lockedSlotTexture = renderer.CreateLockedSlotTexture(60, 60);
        }

        public override void OnRenderGUI(float deltaTime)
        {
            base.OnRenderGUI(deltaTime);

            // Only render if the character/inventory dialog is open
            if (!IsInventoryOpen()) return;

            string playerUid = capi.World.Player?.PlayerUID;
            if (playerUid == null) return;

            HashSet<int> lockedSlots = lockedSlotsManager.GetLockedSlots(playerUid);
            if (lockedSlots.Count == 0) return;

            // Get the player inventory
            IInventory playerInv = capi.World.Player?.InventoryManager?.GetOwnInventory(GlobalConstants.characterInvClassName);
            if (playerInv == null) return;

            // Try to find the inventory dialog to get slot positions
            // Note: This is a simplified approach. In a real implementation, you'd need to
            // hook into the actual inventory GUI to get exact slot positions
            GuiDialog invDialog = FindCharacterDialog();
            if (invDialog == null || !invDialog.IsOpened()) return;

            // Render locked slot overlays
            RenderLockedSlots(lockedSlots, invDialog);
        }

        private bool IsInventoryOpen()
        {
            // Check if the character dialog (inventory) is open
            GuiDialog dialog = FindCharacterDialog();
            return dialog != null && dialog.IsOpened();
        }

        private GuiDialog FindCharacterDialog()
        {
            // Try to find the character dialog
            // The character dialog is typically named "characterdlg" in Vintage Story
            foreach (var dialog in capi.Gui.OpenedGuis)
            {
                if (dialog is GuiDialog guiDialog)
                {
                    string toggleKey = guiDialog.ToggleKeyCombinationCode;
                    if (toggleKey != null && (toggleKey.Contains("character") || toggleKey.Contains("inventory")))
                    {
                        return guiDialog;
                    }
                }
            }
            return null;
        }

        private void RenderLockedSlots(HashSet<int> lockedSlots, GuiDialog invDialog)
        {
            // Access the dialog's composer to find slot grid elements
            var composer = invDialog.SingleComposer;
            if (composer == null) return;

            // Try common slot grid element key names used in the character dialog
            string[] gridKeys = { "inventory", "inventoryslots", "invslots", "slotgrid", "playerinvslots" };
            
            foreach (string key in gridKeys)
            {
                var grid = composer.GetSlotGrid(key);
                if (grid == null) continue;

                var slotBounds = grid.SlotBounds;
                if (slotBounds == null) continue;

                foreach (int slotId in lockedSlots)
                {
                    if (slotId >= 0 && slotId < slotBounds.Length)
                    {
                        var bounds = slotBounds[slotId];
                        if (bounds != null && lockedSlotTexture != null)
                        {
                            capi.Render.Render2DTexture(
                                lockedSlotTexture.TextureId,
                                (float)bounds.renderX,
                                (float)bounds.renderY,
                                (float)bounds.OuterWidth,
                                (float)bounds.OuterHeight
                            );
                        }
                    }
                }

                // Found a matching grid, no need to try other keys
                return;
            }
        }

        public override void Dispose()
        {
            lockedSlotTexture?.Dispose();
            base.Dispose();
        }

        public override string ToggleKeyCombinationCode => null;
    }
}
