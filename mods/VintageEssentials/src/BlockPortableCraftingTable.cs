using System;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace VintageEssentials
{
    /// <summary>
    /// Block class for the Portable Crafting Table.
    /// A placeable block that functions as a crafting station with internal storage
    /// and the ability to pull ingredients from nearby containers.
    /// Supports shift-right-click pickup with inventory preservation.
    /// </summary>
    public class BlockPortableCraftingTable : Block
    {
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            BlockEntityPortableCraftingTable be = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BlockEntityPortableCraftingTable;
            if (be != null)
            {
                // Shift+right-click: pick up the block with its inventory
                if (byPlayer.Entity.Controls.ShiftKey)
                {
                    return TryPickupBlock(world, byPlayer, blockSel.Position, be);
                }

                be.OnBlockInteract(byPlayer);
                return true;
            }

            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }

        /// <summary>
        /// Attempts to pick up the crafting table, preserving its inventory contents
        /// in the item stack's tree attributes.
        /// </summary>
        private bool TryPickupBlock(IWorldAccessor world, IPlayer byPlayer, BlockPos pos, BlockEntityPortableCraftingTable be)
        {
            // Create the item stack for this block
            ItemStack blockStack = new ItemStack(this, 1);

            // Serialize the block entity's inventory into the item stack's attributes
            be.SaveInventoryToItemStack(blockStack);

            // Try to give the item to the player
            if (!byPlayer.InventoryManager.TryGiveItemstack(blockStack))
            {
                // If player inventory is full, drop it on the ground
                world.SpawnItemEntity(blockStack, pos.ToVec3d().Add(0.5, 0.5, 0.5));
            }

            // Remove the block without dropping contents (they're in the item stack)
            be.PreventDropOnRemoval = true;
            world.BlockAccessor.SetBlock(0, pos);

            return true;
        }

        public override void OnBlockRemoved(IWorldAccessor world, BlockPos pos)
        {
            BlockEntityPortableCraftingTable be = world.BlockAccessor.GetBlockEntity(pos) as BlockEntityPortableCraftingTable;
            if (be != null && !be.PreventDropOnRemoval)
            {
                // Drop all stored items when block is broken normally
                be.DropContents(world, pos);
            }

            base.OnBlockRemoved(world, pos);
        }

        public override void OnBlockPlaced(IWorldAccessor world, BlockPos blockPos, ItemStack byItemStack)
        {
            base.OnBlockPlaced(world, blockPos, byItemStack);

            // If the item stack has saved inventory data, restore it
            if (byItemStack?.Attributes != null && byItemStack.Attributes.HasAttribute("craftingTableInventory"))
            {
                BlockEntityPortableCraftingTable be = world.BlockAccessor.GetBlockEntity(blockPos) as BlockEntityPortableCraftingTable;
                if (be != null)
                {
                    be.RestoreInventoryFromItemStack(byItemStack);
                }
            }
        }

        public override string GetPlacedBlockInfo(IWorldAccessor world, BlockPos pos, IPlayer forPlayer)
        {
            BlockEntityPortableCraftingTable be = world.BlockAccessor.GetBlockEntity(pos) as BlockEntityPortableCraftingTable;
            if (be != null)
            {
                int usedSlots = be.GetUsedSlotCount();
                int totalSlots = be.GetTotalSlotCount();
                return $"Storage: {usedSlots}/{totalSlots} slots used\nShift+Right-click to pick up";
            }

            return base.GetPlacedBlockInfo(world, pos, forPlayer);
        }

        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            return new WorldInteraction[]
            {
                new WorldInteraction
                {
                    ActionLangCode = "blockhelp-portablecraftingtable-rightclick",
                    MouseButton = EnumMouseButton.Right
                },
                new WorldInteraction
                {
                    ActionLangCode = "vintageessentials:crafttable-pickup-help",
                    MouseButton = EnumMouseButton.Right,
                    HotKeyCode = "shift"
                }
            };
        }
    }
}
