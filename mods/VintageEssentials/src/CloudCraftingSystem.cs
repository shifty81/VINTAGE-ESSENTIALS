using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace VintageEssentials
{
    /// <summary>
    /// Provides helper methods for the "Cloud Crafting" feature:
    /// scanning nearby containers for ingredients and gathering them
    /// into the crafting grid of a Portable Crafting Table.
    /// </summary>
    public static class CloudCraftingSystem
    {
        /// <summary>Default search radius in blocks.</summary>
        public const int DEFAULT_RADIUS = 15;

        /// <summary>
        /// Scans all containers within <paramref name="radius"/> blocks of <paramref name="center"/>
        /// (excluding <paramref name="exclude"/>) and returns every non-empty <see cref="ItemSlot"/>.
        /// </summary>
        public static List<ItemSlot> CollectNearbySlots(IWorldAccessor world, BlockPos center, int radius, BlockEntityContainer exclude = null)
        {
            List<ItemSlot> result = new List<ItemSlot>();
            if (world == null || center == null) return result;

            BlockPos minPos = new BlockPos(center.X - radius, center.Y - radius, center.Z - radius);
            BlockPos maxPos = new BlockPos(center.X + radius, center.Y + radius, center.Z + radius);

            world.BlockAccessor.WalkBlocks(minPos, maxPos, (block, x, y, z) =>
            {
                BlockPos pos = new BlockPos(x, y, z);
                BlockEntity be = world.BlockAccessor.GetBlockEntity(pos);
                if (be is BlockEntityContainer container && container != exclude && container.Inventory != null)
                {
                    foreach (ItemSlot slot in container.Inventory)
                    {
                        if (slot != null && !slot.Empty)
                        {
                            result.Add(slot);
                        }
                    }
                }
            });

            return result;
        }

        /// <summary>
        /// Tries to find a specific item (matching <paramref name="sample"/>) in the given
        /// <paramref name="sources"/> list and transfers up to <paramref name="quantity"/>
        /// units into <paramref name="target"/>.
        /// Returns the number of items actually moved.
        /// </summary>
        public static int GatherItem(IWorldAccessor world, ItemStack sample, int quantity, ItemSlot target, IEnumerable<ItemSlot> sources)
        {
            if (world == null || sample == null || target == null || sources == null) return 0;

            int remaining = quantity;

            foreach (var src in sources)
            {
                if (remaining <= 0) break;
                if (src == null || src.Empty) continue;

                if (src.Itemstack.Satisfies(sample))
                {
                    int canTake = Math.Min(remaining, src.StackSize);
                    ItemStack taken = src.TakeOut(canTake);
                    if (taken != null && taken.StackSize > 0)
                    {
                        if (target.Empty)
                        {
                            target.Itemstack = taken;
                        }
                        else
                        {
                            target.Itemstack.StackSize += taken.StackSize;
                        }

                        remaining -= taken.StackSize;
                        src.MarkDirty();
                        target.MarkDirty();
                    }
                }
            }

            return quantity - remaining;
        }

        /// <summary>
        /// Attempts to fill every non-empty crafting grid slot to its current stack size + 1
        /// using items from the table's own storage and from nearby containers.
        /// Useful for repeating the same recipe multiple times.
        /// </summary>
        public static int RefillCraftingGrid(BlockEntityPortableCraftingTable tableEntity, IWorldAccessor world)
        {
            if (tableEntity == null || world == null) return 0;

            int totalMoved = 0;

            // Gather all candidate source slots: table storage first, then nearby containers
            List<ItemSlot> sources = new List<ItemSlot>();

            // Table's own storage slots
            for (int i = tableEntity.StorageSlotStart; i < tableEntity.StorageSlotStart + tableEntity.StorageSlotCount; i++)
            {
                ItemSlot slot = tableEntity.Inventory[i];
                if (slot != null && !slot.Empty)
                {
                    sources.Add(slot);
                }
            }

            // Nearby container slots
            sources.AddRange(tableEntity.GetNearbyContainerSlots(DEFAULT_RADIUS));

            // For each occupied crafting grid slot, try to top it up
            for (int i = 0; i < BlockEntityPortableCraftingTable.CRAFT_GRID_SLOTS; i++)
            {
                ItemSlot gridSlot = tableEntity.Inventory[tableEntity.CraftGridSlotStart + i];
                if (gridSlot == null || gridSlot.Empty) continue;

                int moved = GatherItem(world, gridSlot.Itemstack, gridSlot.Itemstack.Collectible.MaxStackSize - gridSlot.StackSize, gridSlot, sources);
                totalMoved += moved;
            }

            return totalMoved;
        }
    }
}
