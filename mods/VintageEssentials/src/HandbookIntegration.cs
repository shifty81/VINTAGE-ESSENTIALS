using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace VintageEssentials
{
    /// <summary>
    /// Provides handbook and recipe integration for the Portable Crafting Table.
    /// Allows opening the handbook, looking up recipes, and auto-filling the
    /// crafting grid from a selected recipe using cloud crafting sources.
    /// </summary>
    public class HandbookIntegration
    {
        private readonly ICoreClientAPI capi;

        public HandbookIntegration(ICoreClientAPI capi)
        {
            this.capi = capi;
        }

        /// <summary>
        /// Opens the in-game handbook dialog.
        /// </summary>
        public void OpenHandbook()
        {
            capi.Input.TriggerOnHotKeyPressed("handbook", false);
        }

        /// <summary>
        /// Finds all grid recipes that produce the given item/block code.
        /// </summary>
        public List<GridRecipe> FindRecipesForOutput(AssetLocation outputCode)
        {
            List<GridRecipe> matches = new List<GridRecipe>();
            if (capi?.World?.GridRecipes == null || outputCode == null) return matches;

            foreach (var recipe in capi.World.GridRecipes)
            {
                if (recipe?.Output?.ResolvedItemstack == null) continue;

                AssetLocation recipeOutputCode = recipe.Output.ResolvedItemstack.Collectible?.Code;
                if (recipeOutputCode != null && recipeOutputCode.Equals(outputCode))
                {
                    matches.Add(recipe);
                }
            }

            return matches;
        }

        /// <summary>
        /// Attempts to fill the crafting grid of the given block entity with
        /// ingredients for the specified recipe, pulling items from table storage
        /// and nearby containers.
        /// Returns the number of ingredient slots successfully filled.
        /// </summary>
        public int AutoFillRecipe(GridRecipe recipe, BlockEntityPortableCraftingTable tableEntity)
        {
            if (recipe == null || tableEntity == null) return 0;
            if (recipe.resolvedIngredients == null) return 0;

            int rw = recipe.Width;
            int rh = recipe.Height;
            int filledCount = 0;

            // Clear the crafting grid first
            ClearCraftingGrid(tableEntity);

            // Collect all available source slots (table storage + nearby containers)
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
            sources.AddRange(tableEntity.GetNearbyContainerSlots(CloudCraftingSystem.DEFAULT_RADIUS));

            // First pass: check if all ingredients are available
            bool allAvailable = CheckIngredientsAvailable(recipe, sources);

            if (!allAvailable)
            {
                return -1; // Signal that ingredients are missing
            }

            // Second pass: actually fill the grid
            for (int ry = 0; ry < rh; ry++)
            {
                for (int rx = 0; rx < rw; rx++)
                {
                    int recipeIndex = ry * rw + rx;
                    if (recipeIndex >= recipe.resolvedIngredients.Length) continue;

                    CraftingRecipeIngredient ingredient = recipe.resolvedIngredients[recipeIndex];
                    if (ingredient == null || ingredient.IsTool) continue;

                    int gridIndex = ry * 3 + rx;
                    ItemSlot gridSlot = tableEntity.Inventory[tableEntity.CraftGridSlotStart + gridIndex];

                    // Try to find and move the ingredient from sources
                    if (GatherIngredient(ingredient, gridSlot, sources))
                    {
                        filledCount++;
                    }
                }
            }

            // Update the crafting output
            tableEntity.UpdateCraftingOutput();

            return filledCount;
        }

        /// <summary>
        /// Checks whether all ingredients for a recipe are available in the source slots.
        /// </summary>
        private bool CheckIngredientsAvailable(GridRecipe recipe, List<ItemSlot> sources)
        {
            if (recipe.resolvedIngredients == null) return false;

            // Track how many of each source slot we'd need to use
            Dictionary<int, int> usageCounts = new Dictionary<int, int>();

            foreach (var ingredient in recipe.resolvedIngredients)
            {
                if (ingredient == null || ingredient.IsTool) continue;

                bool found = false;
                for (int i = 0; i < sources.Count; i++)
                {
                    ItemSlot src = sources[i];
                    if (src == null || src.Empty) continue;

                    int alreadyUsed = usageCounts.ContainsKey(i) ? usageCounts[i] : 0;
                    int available = src.StackSize - alreadyUsed;

                    if (available >= ingredient.Quantity && ingredient.SatisfiesAsIngredient(src.Itemstack))
                    {
                        usageCounts[i] = alreadyUsed + ingredient.Quantity;
                        found = true;
                        break;
                    }
                }

                if (!found) return false;
            }

            return true;
        }

        /// <summary>
        /// Attempts to gather a specific ingredient from available sources into a grid slot.
        /// </summary>
        private bool GatherIngredient(CraftingRecipeIngredient ingredient, ItemSlot gridSlot, List<ItemSlot> sources)
        {
            if (ingredient == null || gridSlot == null) return false;

            int needed = ingredient.Quantity;

            foreach (var src in sources)
            {
                if (needed <= 0) break;
                if (src == null || src.Empty) continue;

                if (ingredient.SatisfiesAsIngredient(src.Itemstack))
                {
                    int canTake = Math.Min(needed, src.StackSize);
                    ItemStack taken = src.TakeOut(canTake);
                    if (taken != null && taken.StackSize > 0)
                    {
                        if (gridSlot.Empty)
                        {
                            gridSlot.Itemstack = taken;
                        }
                        else
                        {
                            gridSlot.Itemstack.StackSize += taken.StackSize;
                        }

                        needed -= taken.StackSize;
                        src.MarkDirty();
                        gridSlot.MarkDirty();
                    }
                }
            }

            return needed <= 0;
        }

        /// <summary>
        /// Clears all items in the crafting grid, returning them to table storage
        /// or dropping them if storage is full.
        /// </summary>
        public void ClearCraftingGrid(BlockEntityPortableCraftingTable tableEntity)
        {
            if (tableEntity == null) return;

            for (int i = 0; i < BlockEntityPortableCraftingTable.CRAFT_GRID_SLOTS; i++)
            {
                ItemSlot gridSlot = tableEntity.Inventory[tableEntity.CraftGridSlotStart + i];
                if (gridSlot == null || gridSlot.Empty) continue;

                // Try to move items back to table storage
                bool moved = false;
                for (int s = tableEntity.StorageSlotStart; s < tableEntity.StorageSlotStart + tableEntity.StorageSlotCount; s++)
                {
                    ItemSlot storageSlot = tableEntity.Inventory[s];
                    if (storageSlot == null) continue;

                    int movedQty = gridSlot.TryPutInto(capi.World, storageSlot);
                    if (gridSlot.Empty)
                    {
                        moved = true;
                        break;
                    }
                }

                if (!moved && !gridSlot.Empty)
                {
                    // Couldn't fit in storage, leave in grid (items won't be lost)
                    gridSlot.MarkDirty();
                }
            }

            // Clear the output slot
            ItemSlot outputSlot = tableEntity.Inventory[tableEntity.OutputSlotStart];
            if (outputSlot != null)
            {
                outputSlot.Itemstack = null;
                outputSlot.MarkDirty();
            }
        }

        /// <summary>
        /// Gets a list of all available grid recipes for display in a recipe browser.
        /// Returns recipes grouped by output item.
        /// </summary>
        public Dictionary<string, List<GridRecipe>> GetGroupedRecipes()
        {
            Dictionary<string, List<GridRecipe>> grouped = new Dictionary<string, List<GridRecipe>>();
            if (capi?.World?.GridRecipes == null) return grouped;

            foreach (var recipe in capi.World.GridRecipes)
            {
                if (recipe?.Output?.ResolvedItemstack == null) continue;

                string key = recipe.Output.ResolvedItemstack.Collectible?.Code?.ToString() ?? "unknown";
                if (!grouped.ContainsKey(key))
                {
                    grouped[key] = new List<GridRecipe>();
                }
                grouped[key].Add(recipe);
            }

            return grouped;
        }
    }
}
