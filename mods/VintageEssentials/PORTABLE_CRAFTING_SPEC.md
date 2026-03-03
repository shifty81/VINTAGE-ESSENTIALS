# Portable Crafting Table Feature Specification

## Overview

A portable crafting table block that can be carried and provides "cloud crafting" - the ability to craft using items from nearby storage containers (within 15 block radius) without manually moving items to the crafting grid.

## Current Status

### ✅ Already Implemented
The Chest Radius Inventory (activated with R key) already has:
- **Search functionality** - Text input field with filtering
- **Sort button** - Sorts items alphabetically (A-Z)
- **Scroll bar** - Handles large inventories with 6 visible rows of 8 slots each
- **15 block radius** - Scans for nearby containers within this range
- **Deposit/Take All** - Bulk transfer functionality

### ✅ Phase 1: Basic Portable Crafting Table Block (Complete)
- `BlockPortableCraftingTable` class with interaction and block removal handling
- `BlockEntityPortableCraftingTable` with 72-slot (12×6) internal storage
- `PortableCraftingTableDialog` GUI showing table storage and player inventory
- Block type JSON definition with 3D model shape
- Crafting recipe (6 planks + 2 sticks + 1 iron ingot)
- Item drop on block removal
- Inventory persistence via NBT serialization
- Block/entity class registration in mod system

### ✅ Phase 2: Storage Integration (Complete)
- 3×3 crafting grid with output slot added to the dialog
- Crafting grid slots (9) and output slot (1) added to block entity inventory
- Recipe matching logic scans all loaded `GridRecipe` entries
- Placing items in the crafting grid automatically shows the output
- Taking the output consumes one unit of each ingredient
- Items transfer freely between storage, crafting grid, and player inventory

### ✅ Phase 3: Cloud Crafting (Complete)
- "Pull Ingredients" button in crafting table dialog
- `CloudCraftingSystem` static helper scans nearby containers within 15 blocks
- `GetNearbyContainerSlots()` on block entity for container discovery
- Pulls matching items from table storage first, then nearby containers
- `RefillCraftingGrid()` helper to top up crafting grid stacks for repeat crafting

### ✅ Phase 4: Handbook Integration (Complete)
- "Handbook" button in crafting table dialog opens the game's handbook
- `HandbookIntegration` helper class for recipe lookup and auto-fill
- `AutoFillRecipe()` method to automatically fill crafting grid from a recipe
- Ingredient availability checking before attempting auto-fill
- Crafting grid clearing with items returned to table storage

### ✅ Phase 5: Portability (Complete)
- Shift+right-click to pick up the crafting table block
- Inventory contents preserved in the item stack's tree attributes
- Inventory restored when the block is placed back down
- Block interaction help showing pickup instructions
- `PreventDropOnRemoval` flag to avoid double-dropping items
- `SaveInventoryToItemStack()` and `RestoreInventoryFromItemStack()` methods

### 🚧 To Be Implemented

## Feature Requirements

### 1. Portable Crafting Table Block

**Block Properties:**
- Placeable block that functions as a crafting station
- Has internal storage: 12x6 grid = 72 inventory slots
- Can be picked up (compatible with "Carry On" mod style mechanics)
- Maintains inventory when moved

**Technical Implementation Needed:**
- Create `BlockPortableCraftingTable` class extending `Block`
- Create `BlockEntityPortableCraftingTable` extending `BlockEntityContainer`
- Define block JSON in `assets/vintageessentials/blocktypes/portablecraftingtable.json`
- Create 3D model/shape for the crafting table
- Add textures for the crafting table block

### 2. Crafting Interface Dialog

**GUI Features:**
- 3x3 crafting grid (standard)
- Recipe output slot
- 12x6 storage grid (72 slots) for the table's internal inventory
- Player inventory display
- Handbook integration button
- "Pull from Storage" button to gather ingredients from nearby containers

**Interface Layout:**
```
┌─────────────────────────────────────────┐
│  Portable Crafting Table                │
├─────────────────────────────────────────┤
│  [Handbook]  [Pull Ingredients]         │
│                                          │
│  Crafting Grid:    Output:              │
│  ┌─┬─┬─┐          ┌─┐                  │
│  │ │ │ │    →     │ │                  │
│  ├─┼─┼─┤          └─┘                  │
│  │ │ │ │                                │
│  ├─┼─┼─┤                                │
│  │ │ │ │                                │
│  └─┴─┴─┘                                │
│                                          │
│  Table Storage (12x6 = 72 slots):       │
│  ┌────────────────────────────────┐    │
│  │ [Scrollable Grid]               │    │
│  └────────────────────────────────┘    │
│                                          │
│  Player Inventory:                      │
│  [Standard player inventory display]    │
└─────────────────────────────────────────┘
```

**Technical Implementation Needed:**
- Create `PortableCraftingTableDialog` class extending `GuiDialogBlockEntity`
- Implement crafting grid with recipe matching
- Add storage grid with scrolling (similar to ChestRadiusInventoryDialog)
- Integrate handbook API for recipe lookup
- Network synchronization for multiplayer

### 3. Cloud Crafting System

**Functionality:**
- When a recipe is selected (via handbook or manually placed items):
  - Scan nearby containers (15 block radius) for ingredients
  - Check table's internal storage (72 slots) for ingredients
  - Automatically pull required items to craft
  - Return finished product to player or table storage

**Algorithm:**
```
1. Player selects recipe from handbook OR places items in grid
2. System identifies required ingredients
3. Search order:
   a. Items already in crafting grid
   b. Table's internal storage (72 slots)
   c. Nearby containers (15 block radius)
4. If all ingredients available:
   a. Pull items from storage to grid
   b. Execute craft
   c. Place output in result slot
5. If ingredients missing:
   a. Highlight missing items
   b. Show message indicating what's needed
```

**Technical Implementation Needed:**
- Recipe matching system using `GridRecipe` API
- Ingredient search across multiple inventories
- Transaction system to ensure atomic crafting (all ingredients or none)
- UI feedback for missing ingredients

### 4. Handbook Integration

**Features:**
- Button to open in-game handbook
- Recipe selection from handbook
- "Craft from Storage" button in handbook recipe view
- Automatic ingredient gathering when recipe selected

**Technical Implementation Needed:**
- Hook into handbook API
- Create recipe selection callback
- Implement ingredient gathering from recipe definition
- Handle recipe variants (e.g., different wood types)

### 5. Portability / Carry Compatibility

**Requirements:**
- Block can be picked up while maintaining inventory
- Compatible with "Carry On" and similar mods
- Inventory persists when moved
- Visual indicator when block is being carried

**Technical Implementation Needed:**
- Implement `IBlockEntityContainer` properly for external mod compatibility
- NBT data serialization/deserialization for inventory persistence
- Block behavior for pickup/place with inventory preservation
- Compatibility attributes in block JSON

## File Structure

New files to be created:
```
src/
├── PortableCraftingTableDialog.cs           (Main GUI) ✅
├── BlockPortableCraftingTable.cs            (Block class) ✅
├── BlockEntityPortableCraftingTable.cs      (Block entity with inventory + crafting) ✅
├── CloudCraftingSystem.cs                   (Cloud crafting helpers) ✅
└── HandbookIntegration.cs                   (Handbook API integration) ✅

assets/vintageessentials/
├── blocktypes/
│   └── portablecraftingtable.json          (Block definition)
├── shapes/
│   └── block/
│       └── portablecraftingtable.json      (3D model)
├── textures/
│   └── block/
│       └── portablecraftingtable.png       (Texture)
└── recipes/
    └── grid/
        └── portablecraftingtable.json      (Recipe to craft the table)
```

## API References

### Key Vintage Story Classes to Use:

1. **BlockEntityContainer**
   - Base class for block entities with inventory
   - Handles inventory serialization/deserialization
   - Network synchronization

2. **GuiDialogBlockEntity**
   - Base class for block entity GUI dialogs
   - Handles opening/closing
   - Inventory slot rendering

3. **GridRecipe**
   - Recipe definition and matching
   - Ingredient resolution
   - Crafting execution

4. **ItemSlot** / **InventoryBase**
   - Inventory management
   - Item transfer operations
   - Stack manipulation

5. **IBlockAccessor.WalkBlocks()**
   - Used to scan nearby containers
   - Already implemented in ChestRadiusInventoryDialog

## Implementation Phases

### Phase 1: Basic Portable Crafting Table Block ✅
- ~~Create block and block entity~~
- ~~Implement 72-slot internal storage~~
- ~~Basic interaction (open GUI)~~
- ~~Simple crafting grid (standard 3x3)~~ - completed in Phase 2

### Phase 2: Storage Integration ✅
- ~~Display table's internal storage in GUI~~
- ~~Allow item transfer between storage and crafting grid~~
- ~~3×3 crafting grid with output slot and recipe matching~~

### Phase 3: Cloud Crafting ✅
- ~~Scan nearby containers for ingredients~~
- ~~Implement "Pull from Storage" functionality~~
- ~~Auto-gather ingredients for manual crafts~~
- ~~CloudCraftingSystem helper class~~

### Phase 4: Handbook Integration ✅
- ~~Add handbook button~~
- ~~Recipe selection from handbook~~
- ~~Auto-craft from selected recipe~~
- ~~Ingredient availability checking~~

### Phase 5: Portability ✅
- ~~Implement pickup with inventory preservation~~
- ~~Test with "Carry On" mod compatibility~~
- ~~Add visual indicators~~

### Phase 6: Polish & Testing ✅
- Replaced all hardcoded UI strings with `Lang.Get()` localization calls
- Fixed bare `catch {}` in `RestoreInventoryFromItemStack()` with proper error logging
- Fixed misleading transfer amount calculation in `DummySlot.TryPutInto()`
- Made `ChestRadiusInventoryDialog.UpdateScrollbar()` functional (was previously a stub)
- UI refinement
- Performance optimization
- Bug fixes

## Estimated Complexity

- **Lines of Code**: ~2000-3000
- **Development Time**: 20-40 hours
- **Testing Time**: 10-15 hours
- **Difficulty**: Advanced
- **Risk Areas**: 
  - Network synchronization in multiplayer
  - Recipe matching with variants
  - Performance with many nearby containers
  - Mod compatibility

## Notes

- This is a substantial feature that essentially creates a new crafting system
- Should be implemented incrementally and tested at each phase
- May require updates to MODDING_GUIDELINES.md with new patterns learned
- Consider releasing as a separate optional feature that can be disabled in config

## References

- [Vintage Story Grid Recipes Guide](https://wiki.vintagestory.at/Modding:Grid_Recipes_Guide)
- [BlockEntityContainer Documentation](https://apidocs.vintagestory.at/api/Vintagestory.GameContent.BlockEntityContainer.html)
- [Modding: Creating Recipes](https://wiki.vintagestory.at/Modding:Grid_Recipes_Guide/en)
- [Improved Handbook Recipes Mod](https://mods.vintagestory.at/improvedhandbookrecipes) (for reference)
