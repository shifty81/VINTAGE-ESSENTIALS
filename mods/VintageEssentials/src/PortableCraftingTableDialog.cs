using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace VintageEssentials
{
    /// <summary>
    /// GUI dialog for the Portable Crafting Table.
    /// Displays the table's internal 72-slot storage grid and player inventory.
    /// Phase 1: Basic storage access and interaction.
    /// </summary>
    public class PortableCraftingTableDialog : GuiDialogBlockEntity
    {
        private BlockEntityPortableCraftingTable blockEntity;
        private const int STORAGE_COLS = 12;
        private const int STORAGE_ROWS = 6;

        public PortableCraftingTableDialog(ICoreClientAPI capi, BlockEntityPortableCraftingTable blockEntity)
            : base("Portable Crafting Table", blockEntity.Inventory, blockEntity.Pos, capi)
        {
            this.blockEntity = blockEntity;
            ComposeDialog();
        }

        private void ComposeDialog()
        {
            double elemWidth = STORAGE_COLS * 51 + 40;
            double slotSize = 51;

            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);
            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);

            // Storage grid label
            ElementBounds storageLabelBounds = ElementBounds.Fixed(0, 30, elemWidth, 20);

            // Storage grid (12x6 = 72 slots)
            ElementBounds storageSlotBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 0, 55, STORAGE_COLS, STORAGE_ROWS);

            // Player inventory label
            double storageHeight = STORAGE_ROWS * slotSize;
            ElementBounds playerLabelBounds = ElementBounds.Fixed(0, storageHeight + 65, elemWidth, 20);

            // Player inventory
            ElementBounds playerInvBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 0, storageHeight + 90, 10, 4);

            bgBounds.BothSizing = ElementSizing.FitToChildren;
            bgBounds.WithChildren(storageLabelBounds, storageSlotBounds, playerLabelBounds, playerInvBounds);

            SingleComposer = capi.Gui.CreateCompo("portablecraftingtable" + blockEntity.Pos, dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar("Portable Crafting Table", OnTitleBarClose)
                .BeginChildElements(bgBounds)
                    .AddStaticText("Table Storage:", CairoFont.WhiteDetailText(), storageLabelBounds)
                    .AddItemSlotGrid(blockEntity.Inventory, SendInvPacket, STORAGE_COLS, storageSlotBounds, "storageSlots")
                    .AddStaticText("Player Inventory:", CairoFont.WhiteDetailText(), playerLabelBounds)
                    .AddInset(playerInvBounds, 3)
                .EndChildElements()
                .Compose();
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
