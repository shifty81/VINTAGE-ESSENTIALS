using System;
using Vintagestory.API.Common;
using Vintagestory.API.Client;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace VintageEssentials
{
    /// <summary>
    /// Block entity for the Portable Crafting Table.
    /// Manages a 72-slot internal storage inventory and handles GUI interactions.
    /// </summary>
    public class BlockEntityPortableCraftingTable : BlockEntityContainer
    {
        private const int STORAGE_COLS = 12;
        private const int STORAGE_ROWS = 6;
        private const int TOTAL_SLOTS = STORAGE_COLS * STORAGE_ROWS;

        private InventoryGeneric storageInventory;
        private PortableCraftingTableDialog dialog;

        public override InventoryBase Inventory => storageInventory;
        public override string InventoryClassName => "portablecraftingtable";

        public BlockEntityPortableCraftingTable()
        {
        }

        public override void Initialize(ICoreAPI api)
        {
            if (storageInventory == null)
            {
                storageInventory = new InventoryGeneric(TOTAL_SLOTS, InventoryClassName + "-" + Pos?.ToString(), api);
            }

            base.Initialize(api);
        }

        public void OnBlockInteract(IPlayer byPlayer)
        {
            if (Api.Side == EnumAppSide.Client)
            {
                ICoreClientAPI capi = Api as ICoreClientAPI;
                if (capi == null) return;

                if (dialog == null || !dialog.IsOpened())
                {
                    dialog = new PortableCraftingTableDialog(capi, this);
                    dialog.TryOpen();
                }
                else
                {
                    dialog.TryClose();
                }
            }
        }

        public int GetUsedSlotCount()
        {
            if (storageInventory == null) return 0;
            int count = 0;
            for (int i = 0; i < storageInventory.Count; i++)
            {
                if (storageInventory[i] != null && !storageInventory[i].Empty)
                {
                    count++;
                }
            }
            return count;
        }

        public int GetTotalSlotCount()
        {
            return TOTAL_SLOTS;
        }

        public void DropContents(IWorldAccessor world, BlockPos pos)
        {
            if (storageInventory == null) return;
            
            for (int i = 0; i < storageInventory.Count; i++)
            {
                ItemSlot slot = storageInventory[i];
                if (slot != null && !slot.Empty)
                {
                    world.SpawnItemEntity(slot.Itemstack, pos.ToVec3d().Add(0.5, 0.5, 0.5));
                    slot.Itemstack = null;
                    slot.MarkDirty();
                }
            }
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            if (storageInventory == null)
            {
                storageInventory = new InventoryGeneric(TOTAL_SLOTS, InventoryClassName + "-" + Pos?.ToString(), worldForResolving.Api);
            }
            
            base.FromTreeAttributes(tree, worldForResolving);
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
        }

        public override void OnBlockUnloaded()
        {
            dialog?.TryClose();
            base.OnBlockUnloaded();
        }
    }
}
