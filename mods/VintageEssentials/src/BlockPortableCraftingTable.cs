using System;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace VintageEssentials
{
    /// <summary>
    /// Block class for the Portable Crafting Table.
    /// A placeable block that functions as a crafting station with internal storage
    /// and the ability to pull ingredients from nearby containers.
    /// </summary>
    public class BlockPortableCraftingTable : Block
    {
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            BlockEntityPortableCraftingTable be = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BlockEntityPortableCraftingTable;
            if (be != null)
            {
                be.OnBlockInteract(byPlayer);
                return true;
            }

            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }

        public override void OnBlockRemoved(IWorldAccessor world, BlockPos pos)
        {
            BlockEntityPortableCraftingTable be = world.BlockAccessor.GetBlockEntity(pos) as BlockEntityPortableCraftingTable;
            if (be != null)
            {
                // Drop all stored items when block is broken
                be.DropContents(world, pos);
            }

            base.OnBlockRemoved(world, pos);
        }

        public override string GetPlacedBlockInfo(IWorldAccessor world, BlockPos pos, IPlayer forPlayer)
        {
            BlockEntityPortableCraftingTable be = world.BlockAccessor.GetBlockEntity(pos) as BlockEntityPortableCraftingTable;
            if (be != null)
            {
                int usedSlots = be.GetUsedSlotCount();
                int totalSlots = be.GetTotalSlotCount();
                return $"Storage: {usedSlots}/{totalSlots} slots used";
            }

            return base.GetPlacedBlockInfo(world, pos, forPlayer);
        }
    }
}
