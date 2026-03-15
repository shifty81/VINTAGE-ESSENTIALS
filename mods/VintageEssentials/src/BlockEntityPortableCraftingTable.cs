using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Client;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace VintageEssentials
{
    /// <summary>
    /// Block entity for the Portable Crafting Table.
    /// Manages a 72-slot internal storage inventory, a 9-slot crafting grid,
    /// and a 1-slot output area. Handles GUI interactions and crafting logic.
    /// </summary>
    public class BlockEntityPortableCraftingTable : BlockEntityContainer
    {
        private const int STORAGE_COLS = 12;
        private const int STORAGE_ROWS = 6;
        private const int STORAGE_SLOTS = STORAGE_COLS * STORAGE_ROWS;
        public const int CRAFT_GRID_SLOTS = 9;
        public const int OUTPUT_SLOTS = 1;
        private const int TOTAL_SLOTS = STORAGE_SLOTS + CRAFT_GRID_SLOTS + OUTPUT_SLOTS;

        private InventoryGeneric inventory;
        private PortableCraftingTableDialog dialog;

        /// <summary>
        /// When true, prevents items from being dropped when the block is removed.
        /// Used during pickup to preserve inventory in the item stack.
        /// </summary>
        public bool PreventDropOnRemoval { get; set; } = false;

        /// <summary>Set to true while UpdateCraftingOutput() is writing to the output slot,
        /// so that the slot-modified handler can distinguish programmatic writes from player
        /// interactions (which should trigger ingredient consumption).</summary>
        private bool outputSetBySystem = false;

        /// <summary>Re-entrancy guard: true while HandleOutputTaken() is running, which
        /// causes all further SlotModified events to be suppressed.</summary>
        private bool isHandlingOutputTaken = false;

        public override InventoryBase Inventory => inventory;
        public override string InventoryClassName => "portablecraftingtable";

        /// <summary>Index of the first storage slot.</summary>
        public int StorageSlotStart => 0;
        /// <summary>Number of storage slots (72).</summary>
        public int StorageSlotCount => STORAGE_SLOTS;
        /// <summary>Index of the first crafting grid slot.</summary>
        public int CraftGridSlotStart => STORAGE_SLOTS;
        /// <summary>Index of the output slot.</summary>
        public int OutputSlotStart => STORAGE_SLOTS + CRAFT_GRID_SLOTS;

        private string GetInventoryId()
        {
            return InventoryClassName + "-" + Pos?.ToString();
        }

        public BlockEntityPortableCraftingTable()
        {
        }

        public override void Initialize(ICoreAPI api)
        {
            if (inventory == null)
            {
                inventory = new InventoryGeneric(TOTAL_SLOTS, GetInventoryId(), api);
            }

            base.Initialize(api);

            // Listen for slot changes to update crafting output
            inventory.SlotModified += OnSlotModified;
        }

        private void OnSlotModified(int slotId)
        {
            // Suppress all events while HandleOutputTaken is running (avoids re-entrancy)
            if (isHandlingOutputTaken) return;

            if (slotId >= CraftGridSlotStart && slotId < OutputSlotStart)
            {
                // A crafting grid slot changed – refresh the output preview
                UpdateCraftingOutput();
            }
            else if (slotId == OutputSlotStart && !outputSetBySystem)
            {
                // The output slot was changed by the player (not by UpdateCraftingOutput).
                // If the slot is now empty the player just took the crafted item, so we must
                // consume one set of ingredients from the crafting grid.
                if (inventory[OutputSlotStart].Empty)
                {
                    isHandlingOutputTaken = true;
                    try
                    {
                        HandleOutputTaken();
                    }
                    finally
                    {
                        isHandlingOutputTaken = false;
                    }
                }
            }
        }

        /// <summary>
        /// Called when the player removes the crafted item from the output slot.
        /// Consumes one unit of each occupied crafting grid slot and refreshes
        /// the output preview.
        /// </summary>
        private void HandleOutputTaken()
        {
            // Build array of live references to the current crafting grid slots.
            // These point directly into the inventory, so ResolveMatchingRecipe and
            // the consumption loop both operate on the same up-to-date state.
            ItemSlot[] gridSlots = new ItemSlot[CRAFT_GRID_SLOTS];
            for (int i = 0; i < CRAFT_GRID_SLOTS; i++)
                gridSlots[i] = inventory[CraftGridSlotStart + i];

            // Only consume if a valid recipe is currently in the grid
            if (ResolveMatchingRecipe(gridSlots) != null)
            {
                for (int i = 0; i < CRAFT_GRID_SLOTS; i++)
                {
                    ItemSlot slot = gridSlots[i];
                    if (slot != null && !slot.Empty)
                    {
                        slot.TakeOut(1);
                        slot.MarkDirty();
                    }
                }
            }

            // Refresh output based on the (now reduced) grid contents
            UpdateCraftingOutput();
        }

        /// <summary>
        /// Checks the crafting grid against known recipes and places the result in the output slot.
        /// </summary>
        public void UpdateCraftingOutput()
        {
            if (Api == null) return;

            // Build an array of the 9 crafting grid stacks
            ItemSlot[] gridSlots = new ItemSlot[CRAFT_GRID_SLOTS];
            for (int i = 0; i < CRAFT_GRID_SLOTS; i++)
            {
                gridSlots[i] = inventory[CraftGridSlotStart + i];
            }

            // Try to find a matching grid recipe
            ItemSlot outputSlot = inventory[OutputSlotStart];
            GridRecipe matchedRecipe = ResolveMatchingRecipe(gridSlots);

            // Guard: treat this write as a system operation so OnSlotModified will not
            // treat it as a player taking the item.
            outputSetBySystem = true;
            if (matchedRecipe != null)
            {
                ItemStack outputStack = matchedRecipe.Output.ResolvedItemstack?.Clone();
                outputSlot.Itemstack = outputStack;
            }
            else
            {
                outputSlot.Itemstack = null;
            }

            outputSlot.MarkDirty();
            outputSetBySystem = false;
        }

        private GridRecipe ResolveMatchingRecipe(ItemSlot[] gridSlots)
        {
            if (Api?.World == null) return null;

            var recipes = Api.World.GridRecipes;
            if (recipes == null) return null;

            foreach (var recipe in recipes)
            {
                if (recipe == null) continue;
                if (MatchesRecipe(recipe, gridSlots))
                {
                    return recipe;
                }
            }

            return null;
        }

        private bool MatchesRecipe(GridRecipe recipe, ItemSlot[] gridSlots)
        {
            if (recipe.resolvedIngredients == null) return false;

            int rw = recipe.Width;
            int rh = recipe.Height;

            // Try every valid offset in the 3x3 grid
            for (int ox = 0; ox <= 3 - rw; ox++)
            {
                for (int oy = 0; oy <= 3 - rh; oy++)
                {
                    if (MatchesAtOffset(recipe, gridSlots, ox, oy))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool MatchesAtOffset(GridRecipe recipe, ItemSlot[] gridSlots, int ox, int oy)
        {
            int rw = recipe.Width;
            int rh = recipe.Height;

            for (int gy = 0; gy < 3; gy++)
            {
                for (int gx = 0; gx < 3; gx++)
                {
                    int slotIndex = gy * 3 + gx;
                    ItemSlot slot = gridSlots[slotIndex];

                    int rx = gx - ox;
                    int ry = gy - oy;

                    if (rx >= 0 && rx < rw && ry >= 0 && ry < rh)
                    {
                        int recipeIndex = ry * rw + rx;
                        CraftingRecipeIngredient ingredient = recipe.resolvedIngredients[recipeIndex];

                        if (ingredient == null || ingredient.IsTool)
                        {
                            // Null ingredient means empty slot expected (unless tool)
                            if (ingredient == null && slot != null && !slot.Empty)
                            {
                                return false;
                            }
                            continue;
                        }

                        if (slot == null || slot.Empty)
                        {
                            return false;
                        }

                        if (!ingredient.SatisfiesAsIngredient(slot.Itemstack))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        // Outside the recipe area – slot must be empty
                        if (slot != null && !slot.Empty)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Consumes one set of ingredients from the crafting grid and returns the output stack.
        /// </summary>
        public ItemStack PerformCraft()
        {
            ItemSlot outputSlot = inventory[OutputSlotStart];
            if (outputSlot == null || outputSlot.Empty) return null;

            ItemStack result = outputSlot.Itemstack.Clone();

            // Consume one unit of each ingredient in the crafting grid
            for (int i = 0; i < CRAFT_GRID_SLOTS; i++)
            {
                ItemSlot slot = inventory[CraftGridSlotStart + i];
                if (slot != null && !slot.Empty)
                {
                    slot.TakeOut(1);
                    slot.MarkDirty();
                }
            }

            // Recalculate output
            UpdateCraftingOutput();

            return result;
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
            if (inventory == null) return 0;
            int count = 0;
            for (int i = 0; i < STORAGE_SLOTS; i++)
            {
                if (inventory[i] != null && !inventory[i].Empty)
                {
                    count++;
                }
            }
            return count;
        }

        public int GetTotalSlotCount()
        {
            return STORAGE_SLOTS;
        }

        public void DropContents(IWorldAccessor world, BlockPos pos)
        {
            if (inventory == null) return;

            // Drop storage and crafting grid contents (not the virtual output)
            int dropCount = STORAGE_SLOTS + CRAFT_GRID_SLOTS;
            for (int i = 0; i < dropCount; i++)
            {
                ItemSlot slot = inventory[i];
                if (slot != null && !slot.Empty)
                {
                    world.SpawnItemEntity(slot.Itemstack, pos.ToVec3d().Add(0.5, 0.5, 0.5));
                    slot.Itemstack = null;
                    slot.MarkDirty();
                }
            }
        }

        /// <summary>
        /// Serializes the block entity's inventory into an item stack's tree attributes.
        /// Used when the block is picked up to preserve its contents.
        /// </summary>
        public void SaveInventoryToItemStack(ItemStack itemStack)
        {
            if (itemStack == null || inventory == null) return;

            ITreeAttribute invTree = new TreeAttribute();
            int savedCount = STORAGE_SLOTS + CRAFT_GRID_SLOTS; // Don't save the virtual output slot

            for (int i = 0; i < savedCount; i++)
            {
                ItemSlot slot = inventory[i];
                if (slot != null && !slot.Empty)
                {
                    ITreeAttribute slotTree = new TreeAttribute();
                    slot.Itemstack.ToTreeAttributes(slotTree);
                    invTree["slot" + i] = slotTree;
                }
            }

            invTree.SetInt("slotCount", savedCount);
            itemStack.Attributes["craftingTableInventory"] = invTree;
        }

        /// <summary>
        /// Restores the block entity's inventory from an item stack's tree attributes.
        /// Used when a previously picked-up crafting table is placed back down.
        /// </summary>
        public void RestoreInventoryFromItemStack(ItemStack itemStack)
        {
            if (itemStack?.Attributes == null || inventory == null || Api == null) return;

            ITreeAttribute invTree = itemStack.Attributes.GetTreeAttribute("craftingTableInventory");
            if (invTree == null) return;

            int savedCount = invTree.GetInt("slotCount", 0);
            bool restoredSuccessfully = true;

            for (int i = 0; i < savedCount && i < STORAGE_SLOTS + CRAFT_GRID_SLOTS; i++)
            {
                ITreeAttribute slotTree = invTree.GetTreeAttribute("slot" + i);
                if (slotTree != null)
                {
                    try
                    {
                        ItemStack stack = new ItemStack();
                        stack.FromTreeAttributes(slotTree, Api.World);
                        stack.ResolveBlockOrItem(Api.World);

                        if (stack.Collectible != null)
                        {
                            inventory[i].Itemstack = stack;
                            inventory[i].MarkDirty();
                        }
                    }
                    catch (Exception ex)
                    {
                        Api.World.Logger.Error($"VintageEssentials: Failed to restore inventory slot {i}: {ex.Message}");
                        restoredSuccessfully = false;
                    }
                }
            }

            // Only remove the inventory data if restoration completed successfully
            if (restoredSuccessfully)
            {
                itemStack.Attributes.RemoveAttribute("craftingTableInventory");
            }

            // Update crafting output in case grid has items
            UpdateCraftingOutput();
        }

        /// <summary>
        /// Scans nearby containers (within given radius) and returns all non-empty slots.
        /// Re-uses the same pattern as ChestRadiusInventoryDialog.
        /// </summary>
        public List<ItemSlot> GetNearbyContainerSlots(int radius)
        {
            List<ItemSlot> slots = new List<ItemSlot>();
            if (Api == null || Pos == null) return slots;

            BlockPos minPos = new BlockPos(Pos.X - radius, Pos.Y - radius, Pos.Z - radius);
            BlockPos maxPos = new BlockPos(Pos.X + radius, Pos.Y + radius, Pos.Z + radius);

            Api.World.BlockAccessor.WalkBlocks(minPos, maxPos, (block, x, y, z) =>
            {
                BlockPos bpos = new BlockPos(x, y, z);
                BlockEntity be = Api.World.BlockAccessor.GetBlockEntity(bpos);
                if (be is BlockEntityContainer container && container != this && container.Inventory != null)
                {
                    foreach (ItemSlot slot in container.Inventory)
                    {
                        if (slot != null && !slot.Empty)
                        {
                            slots.Add(slot);
                        }
                    }
                }
            });

            return slots;
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            if (inventory == null)
            {
                inventory = new InventoryGeneric(TOTAL_SLOTS, GetInventoryId(), worldForResolving.Api);
            }

            base.FromTreeAttributes(tree, worldForResolving);
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
        }

        public override void OnBlockUnloaded()
        {
            if (inventory != null)
            {
                inventory.SlotModified -= OnSlotModified;
            }
            dialog?.TryClose();
            base.OnBlockUnloaded();
        }
    }
}
