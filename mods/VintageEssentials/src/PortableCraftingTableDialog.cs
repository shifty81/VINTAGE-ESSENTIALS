using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace VintageEssentials
{
    /// <summary>
    /// GUI dialog for the Portable Crafting Table.
    /// Displays a 3x3 crafting grid with output slot, the table's internal 72-slot
    /// storage grid, "Pull from Storage" and "Handbook" buttons, and the player inventory.
    /// Phase 2: Crafting grid + storage integration.
    /// Phase 3: Cloud crafting (pull ingredients from nearby containers).
    /// Phase 4: Handbook integration (open handbook, auto-fill recipes).
    /// </summary>
    public class PortableCraftingTableDialog : GuiDialogBlockEntity
    {
        private BlockEntityPortableCraftingTable blockEntity;
        private HandbookIntegration handbookIntegration;
        private const int STORAGE_COLS = 12;
        private const int STORAGE_ROWS = 6;
        private const int CRAFT_GRID_SIZE = 3;
        private const int NEARBY_RADIUS = 15;

        public PortableCraftingTableDialog(ICoreClientAPI capi, BlockEntityPortableCraftingTable blockEntity)
            : base("Portable Crafting Table", blockEntity.Inventory, blockEntity.Pos, capi)
        {
            this.blockEntity = blockEntity;
            this.handbookIntegration = new HandbookIntegration(capi);
            ComposeDialog();
        }

        private void ComposeDialog()
        {
            double slotSize = 51;
            double pad = 10;

            // ---- Sizing ----
            double storageWidth = STORAGE_COLS * slotSize;
            double elemWidth = storageWidth + 40;

            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);
            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);

            double yOffset = 30;

            // ---- Crafting section label ----
            ElementBounds craftLabelBounds = ElementBounds.Fixed(0, yOffset, elemWidth, 20);
            yOffset += 25;

            // ---- Crafting grid (3x3) – left side ----
            ElementBounds craftGridBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 0, yOffset, CRAFT_GRID_SIZE, CRAFT_GRID_SIZE);

            // ---- Arrow between grid and output ----
            double arrowX = CRAFT_GRID_SIZE * slotSize + pad;
            double arrowY = yOffset + slotSize; // vertically centred on middle row
            ElementBounds arrowBounds = ElementBounds.Fixed(arrowX, arrowY, 30, 20);

            // ---- Output slot – right of arrow ----
            double outputX = arrowX + 40;
            ElementBounds outputSlotBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, outputX, yOffset + slotSize - slotSize / 2 + 5, 1, 1);

            // ---- "Pull from Storage" button ----
            double pullBtnX = outputX + slotSize + pad;
            ElementBounds pullBtnBounds = ElementBounds.Fixed(pullBtnX, yOffset + slotSize - 5, 170, 30);

            // ---- "Handbook" button ----
            double handbookBtnX = pullBtnX;
            ElementBounds handbookBtnBounds = ElementBounds.Fixed(handbookBtnX, yOffset + slotSize + 30, 170, 30);

            double craftSectionHeight = CRAFT_GRID_SIZE * slotSize;
            yOffset += craftSectionHeight + pad;

            // ---- Storage section label ----
            ElementBounds storageLabelBounds = ElementBounds.Fixed(0, yOffset, elemWidth, 20);
            yOffset += 25;

            // ---- Storage grid (12×6 = 72 slots) ----
            ElementBounds storageSlotBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 0, yOffset, STORAGE_COLS, STORAGE_ROWS);

            double storageHeight = STORAGE_ROWS * slotSize;
            yOffset += storageHeight + pad;

            // ---- Player inventory label ----
            ElementBounds playerLabelBounds = ElementBounds.Fixed(0, yOffset, elemWidth, 20);
            yOffset += 25;

            // ---- Player inventory (hotbar 10×1, backpack 10×3) ----
            ElementBounds playerHotbarBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 0, yOffset, 10, 1);
            yOffset += slotSize + pad;
            ElementBounds playerBackpackBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 0, yOffset, 10, 3);

            bgBounds.BothSizing = ElementSizing.FitToChildren;
            bgBounds.WithChildren(craftLabelBounds, craftGridBounds, arrowBounds, outputSlotBounds,
                                  pullBtnBounds, handbookBtnBounds, storageLabelBounds, storageSlotBounds,
                                  playerLabelBounds, playerHotbarBounds, playerBackpackBounds);

            // Get the player's own inventory for display
            IInventory playerInv = capi.World.Player.InventoryManager.GetOwnInventory(GlobalConstants.characterInvClassName);

            // Build slot ranges for the block entity inventory
            // Slots 0-71 = storage, 72-80 = crafting grid, 81 = output
            SingleComposer = capi.Gui.CreateCompo("portablecraftingtable" + blockEntity.Pos, dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar(Lang.Get("vintageessentials:crafttable-title"), OnTitleBarClose)
                .BeginChildElements(bgBounds)
                    // Crafting section
                    .AddStaticText(Lang.Get("vintageessentials:crafttable-craftgrid"), CairoFont.WhiteDetailText(), craftLabelBounds)
                    .AddItemSlotGrid(blockEntity.Inventory, SendInvPacket, CRAFT_GRID_SIZE, craftGridBounds, "craftGrid")
                    .AddStaticText("→", CairoFont.WhiteDetailText().WithFontSize(24), arrowBounds)
                    .AddItemSlotGrid(blockEntity.Inventory, SendInvPacket, 1, outputSlotBounds, "outputSlot")
                    .AddSmallButton(Lang.Get("vintageessentials:crafttable-pull"), OnPullFromStorage, pullBtnBounds, EnumButtonStyle.Normal, "pullBtn")
                    .AddSmallButton(Lang.Get("vintageessentials:crafttable-handbook"), OnOpenHandbook, handbookBtnBounds, EnumButtonStyle.Normal, "handbookBtn")

                    // Storage section
                    .AddStaticText(Lang.Get("vintageessentials:crafttable-storage"), CairoFont.WhiteDetailText(), storageLabelBounds)
                    .AddItemSlotGrid(blockEntity.Inventory, SendInvPacket, STORAGE_COLS, storageSlotBounds, "storageSlots")

                    // Player inventory
                    .AddStaticText(Lang.Get("vintageessentials:crafttable-playerinv"), CairoFont.WhiteDetailText(), playerLabelBounds)
                    .AddItemSlotGrid(playerInv, SendInvPacket, 10, playerHotbarBounds, "playerHotbar")
                    .AddItemSlotGrid(playerInv, SendInvPacket, 10, playerBackpackBounds, "playerBackpack")
                .EndChildElements()
                .Compose();
        }

        /// <summary>
        /// Opens the in-game handbook dialog for recipe browsing.
        /// </summary>
        private bool OnOpenHandbook()
        {
            if (handbookIntegration == null) return true;

            handbookIntegration.OpenHandbook();
            return true;
        }

        /// <summary>
        /// Cloud Crafting – pull missing ingredients from the table's own storage
        /// and from nearby containers within 15 blocks.
        /// </summary>
        private bool OnPullFromStorage()
        {
            if (blockEntity == null) return true;

            // Determine which crafting grid slots are empty and what the existing pattern needs
            // For simplicity, if there's already a partial recipe in the grid,
            // try to fill identical stacks from storage / nearby containers.
            int filled = 0;

            for (int i = 0; i < BlockEntityPortableCraftingTable.CRAFT_GRID_SLOTS; i++)
            {
                ItemSlot gridSlot = blockEntity.Inventory[blockEntity.CraftGridSlotStart + i];
                if (gridSlot == null || gridSlot.Empty) continue;

                // Try to top-up this stack from storage
                filled += PullMatchingItem(gridSlot);
            }

            // Also scan the table storage for any non-empty slots and
            // try to place them into empty grid slots (basic auto-fill)
            // This is most useful when the player has manually set up a recipe pattern

            blockEntity.UpdateCraftingOutput();

            if (filled > 0)
            {
                capi.ShowChatMessage(Lang.Get("vintageessentials:crafttable-pulled", filled));
            }
            else
            {
                capi.ShowChatMessage(Lang.Get("vintageessentials:crafttable-nopull"));
            }

            return true;
        }

        /// <summary>
        /// Attempts to fill <paramref name="gridSlot"/> from the table's internal
        /// storage first, then from nearby containers within 15 blocks.
        /// Returns the number of items moved.
        /// </summary>
        private int PullMatchingItem(ItemSlot gridSlot)
        {
            if (gridSlot == null || gridSlot.Empty) return 0;

            int totalMoved = 0;

            // 1) Search table's internal storage (slots 0 – 71)
            for (int i = blockEntity.StorageSlotStart; i < blockEntity.StorageSlotStart + blockEntity.StorageSlotCount; i++)
            {
                ItemSlot src = blockEntity.Inventory[i];
                if (src == null || src.Empty) continue;

                if (src.Itemstack.Satisfies(gridSlot.Itemstack))
                {
                    int moved = src.TryPutInto(capi.World, gridSlot);
                    if (moved > 0)
                    {
                        totalMoved += moved;
                        src.MarkDirty();
                        gridSlot.MarkDirty();
                    }
                }
            }

            // 2) Search nearby containers
            List<ItemSlot> nearbySlots = blockEntity.GetNearbyContainerSlots(NEARBY_RADIUS);
            foreach (var src in nearbySlots)
            {
                if (src == null || src.Empty) continue;

                if (src.Itemstack.Satisfies(gridSlot.Itemstack))
                {
                    int moved = src.TryPutInto(capi.World, gridSlot);
                    if (moved > 0)
                    {
                        totalMoved += moved;
                        src.MarkDirty();
                        gridSlot.MarkDirty();
                    }
                }
            }

            return totalMoved;
        }

        private void SendInvPacket(object packet)
        {
            capi.Network.SendBlockEntityPacket(blockEntity.Pos.X, blockEntity.Pos.Y, blockEntity.Pos.Z, packet);
        }

        private void OnTitleBarClose()
        {
            TryClose();
        }

        public override string ToggleKeyCombinationCode => null;
    }
}
