using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace VintageEssentials
{
    /// <summary>
    /// Handles inventory slot click events for the locking system.
    /// When locking mode is active, clicking on an inventory slot toggles its lock state
    /// instead of performing the normal item interaction.
    /// </summary>
    public class InventorySlotClickHandler
    {
        private ICoreClientAPI capi;
        private InventoryLockDialog lockDialog;
        private LockedSlotsManager lockedSlotsManager;

        public InventorySlotClickHandler(ICoreClientAPI capi, InventoryLockDialog lockDialog, LockedSlotsManager lockedSlotsManager)
        {
            this.capi = capi;
            this.lockDialog = lockDialog;
            this.lockedSlotsManager = lockedSlotsManager;
        }

        public void Initialize()
        {
            capi.Event.MouseDown += OnMouseDown;
        }

        private void OnMouseDown(MouseEvent e)
        {
            // Only process if we're in locking mode
            if (!lockDialog.IsLockingMode()) return;

            // Check if the inventory is open
            IInventory playerInv = capi.World.Player?.InventoryManager?.GetOwnInventory(GlobalConstants.characterInvClassName);
            if (playerInv == null) return;

            // Try to determine which slot was clicked
            int? clickedSlotId = DetermineClickedSlot(e);
            
            if (clickedSlotId.HasValue)
            {
                lockDialog.TryToggleSlotLock(clickedSlotId.Value);
                
                // Prevent the normal click action when in locking mode
                e.Handled = true;
            }
        }

        private int? DetermineClickedSlot(MouseEvent e)
        {
            // Search through all open GUIs to find the character/inventory dialog
            foreach (var gui in capi.Gui.OpenedGuis)
            {
                if (gui is GuiDialog guiDialog)
                {
                    string toggleKey = guiDialog.ToggleKeyCombinationCode;
                    if (toggleKey == null) continue;
                    
                    // Look for the character dialog (inventory screen)
                    if (!toggleKey.Contains("character") && !toggleKey.Contains("inventory")) continue;

                    // Access the dialog's composer to find slot grid elements
                    var composer = guiDialog.SingleComposer;
                    if (composer == null) continue;

                    // Walk through inventory slots and check if the click position
                    // falls within any slot's rendered bounds
                    IInventory playerInv = capi.World.Player?.InventoryManager?.GetOwnInventory(GlobalConstants.characterInvClassName);
                    if (playerInv == null) return null;

                    int mouseX = e.X;
                    int mouseY = e.Y;

                    // Try to find the slot grid element by checking interactive elements
                    // The slot grid stores slot bounds internally
                    var interactiveElement = composer.GetSlotGrid("inventory");
                    if (interactiveElement != null)
                    {
                        // Use the slot grid to determine which slot index was clicked
                        // by checking each slot's rendered bounds against the mouse position
                        for (int slotId = 0; slotId < playerInv.Count; slotId++)
                        {
                            // GuiElementItemSlotGrid provides slot bounds through its API
                            // Check if mouse position is within this slot's area
                            var slotBounds = interactiveElement.SlotBounds;
                            if (slotBounds != null && slotId < slotBounds.Length)
                            {
                                var bounds = slotBounds[slotId];
                                if (bounds != null &&
                                    mouseX >= bounds.renderX && mouseX <= bounds.renderX + bounds.OuterWidth &&
                                    mouseY >= bounds.renderY && mouseY <= bounds.renderY + bounds.OuterHeight)
                                {
                                    return slotId;
                                }
                            }
                        }
                    }

                    // Fallback: try common slot grid key names
                    string[] gridKeys = { "inventoryslots", "invslots", "slotgrid", "playerinvslots" };
                    foreach (string key in gridKeys)
                    {
                        var grid = composer.GetSlotGrid(key);
                        if (grid == null) continue;

                        var slotBounds = grid.SlotBounds;
                        if (slotBounds == null) continue;

                        for (int slotId = 0; slotId < slotBounds.Length && slotId < playerInv.Count; slotId++)
                        {
                            var bounds = slotBounds[slotId];
                            if (bounds != null &&
                                mouseX >= bounds.renderX && mouseX <= bounds.renderX + bounds.OuterWidth &&
                                mouseY >= bounds.renderY && mouseY <= bounds.renderY + bounds.OuterHeight)
                            {
                                return slotId;
                            }
                        }
                    }
                }
            }

            return null;
        }

        public void Dispose()
        {
            if (capi != null)
            {
                capi.Event.MouseDown -= OnMouseDown;
            }
        }
    }
}
