# Changelog

All notable changes to the Vintage Essentials mod will be documented in this file.

## [Unreleased]

### Added
- **Tabbed Configuration Menu** — `ModConfigDialog`:
  - Refactored the settings dialog from a single flat panel to a tabbed interface with 10 tabs.
  - **General tab**: Retains all existing settings (max locked slots, stack size multiplier, keybind customization).
  - **9 category tabs**: Storage & Inventory, Cooking & Food, Hunting & Animals, Building & Decoration, Crafting & Smithing, Combat & Skills, World & Exploration, Quality of Life, and Libraries.
  - Each category tab displays all conglomerate mods in that category with toggle switches and descriptions.
  - **Enable All / Disable All** buttons per category for quick bulk management.
  - Mod enable/disable state persists in the config file across sessions.

- **Conglomerate Mod Manager** — `ConglomerateModManager`:
  - New static manager that catalogs all 77 mods from the Conglomerate folder.
  - Mods are organized into 9 categories with display names and descriptions.
  - Provides category lookup, mod listing, and metadata access.

- **Extended Config** — `ModConfig`:
  - Added `ConglomerateModsEnabled` dictionary to persist per-mod enable/disable state.
  - Added `IsModEnabled()`, `SetModEnabled()`, and `EnsureConglomerateDefaults()` methods.

### Fixed
- **RTP Safe Location — Still Failing to Find Safe Ground** — `VintageEssentialsModSystem`:
  - **5×5 chunk loading**: Expanded chunk pre-loading grid from 3×3 (9 chunks) to 5×5 (25 chunks) around the target, providing fuller neighbor coverage for world generation.
  - **Grid surface search**: Each attempt now searches a 5×5 grid of block columns (25 positions at ±4/±8 block offsets) instead of a single column, greatly increasing the chance of finding safe ground even on difficult terrain.
  - **Progressive delays**: Wait time now increases with each retry (5s base + 1s per attempt) to give the server more time for terrain generation at new locations.
  - **Exception-safe retries**: Wrapped the async callback in try-catch to prevent silent failures — if an exception occurs during surface scanning, the retry chain continues instead of silently aborting.
  - **Player progress feedback**: Players now receive a notification on each retry attempt ("Attempt X/10 — searching a new area...") so they know the search is still active.
  - **Reduced RTP distance**: Lowered the random distance range from 10k–20k to 3k–10k blocks, which significantly improves chunk generation reliability at unvisited locations.
  - **Cleaner block scanning**: Refactored `FindSurfaceY` to skip null/air/liquid/lava blocks early with `continue` instead of nesting all logic inside a single large conditional.
  - **3×3 chunk loading**: Force-loads chunk columns around the target to ensure proper world generation (terrain gen depends on neighbor chunks).
  - **Passable block detection**: Surface check accepts plants, snow, and leaves as passable blocks above the ground, not just air.
  - **Lava exclusion**: Lava blocks are excluded from valid ground surfaces (separate from `EnumBlockMaterial.Liquid`).
  - **10 retry attempts** with ±500 block perpendicular offset so retries explore different terrain.
  - **Unloaded chunk detection**: Logs when an entire column returns no solid blocks, indicating the chunk may not have loaded in time.
  - **Debug logging**: RTP retry attempts are logged with position info for server-side troubleshooting.

- **Player Inventory Sort — Crash and World Corruption** — `PlayerInventorySortDialog.SortPlayerInventory()`:
  - **Crash**: `MarkDirty()` threw `ArgumentException: Supplied slot is not part of this inventory` because slots collected via `foreach` iterator lost their inventory association. Now accesses slots by index through `playerInv[index]` so the inventory correctly recognizes the slot.
  - **World corruption**: The old two-phase approach (clear ALL slots to null → reassign sorted items) meant that if the crash occurred mid-clear, items already nulled were permanently lost from the world save. Now assigns sorted items directly to each slot in a single pass with no intermediate empty state.
  - **Null safety**: Added null-coalescing fallback on `GetName()` (`?? ""`) to prevent crashes from items with null display names.

- **Player Inventory Sort — Wrong Inventory Class** — `PlayerInventorySortDialog.SortPlayerInventory()`:
  - Was calling `GetOwnInventory(GlobalConstants.characterInvClassName)` which returns the player's clothing and armor slots, causing `Shift+S` to silently sort armor/clothing slots instead of bag items.
  - Now correctly sorts the `backpackInvClassName` ("backpack") inventory, which contains the player's bag storage slots.
  - Removed the now-unnecessary "skip hotbar slots (0-9)" guard since the backpack inventory does not contain hotbar slots.

- **Chest Radius — Deposit All / Take All operating on wrong inventory** — `ChestRadiusInventoryDialog`:
  - `OnDepositClicked()` and `OnTakeAllClicked()` were calling `GetOwnInventory(GlobalConstants.characterInvClassName)`, causing the buttons to iterate over armor/clothing slots instead of the player's item slots.
  - Both methods now build a combined slot list from `hotBarInvClassName` ("hotbar") and `backpackInvClassName` ("backpack"), matching the inventory classes used by `PortableCraftingTableDialog`.

- **Chest Radius — Player Inventory Hook operating on wrong inventory** — `ChestRadiusInventoryDialog.SetupPlayerInventoryHook()` / `CleanupPlayerInventoryHook()`:
  - Was subscribing to `SlotModified` on `characterInvClassName`, so the display never refreshed when the player's actual item slots changed.
  - Now hooks into both `hotBarInvClassName` and `backpackInvClassName` for complete coverage.

## [1.6.0] - 2026-03-15

### Fixed
- **Portable Crafting Table Dialog — Wrong Slots Shown** — `PortableCraftingTableDialog.ComposeDialog()`:
  - All three `AddItemSlotGrid` calls for the block entity inventory used the overload without `indicestoshow`, causing every section (crafting grid, output slot, and storage) to render all 82 inventory slots instead of their respective subsets.
  - Added explicit `int[]` index arrays: crafting grid shows slots 72–80, output slot shows slot 81, and storage shows slots 0–71.

- **Portable Crafting Table Dialog — Wrong Player Inventory Shown** — `PortableCraftingTableDialog.ComposeDialog()`:
  - Used `GlobalConstants.characterInvClassName` ("character") which contains the player's clothing and armor slots, not the hotbar or bag storage.
  - Now correctly uses `GlobalConstants.hotBarInvClassName` ("hotbar") for the hotbar row and `GlobalConstants.backpackInvClassName` ("backpack") for the bag-storage rows.
  - Backpack display updated from 10×3 to 10×4 rows to match the standard backpack layout.

- **Crafting Ingredients Not Consumed** — `BlockEntityPortableCraftingTable`:
  - Taking the crafted item from the output slot did not consume the ingredients in the crafting grid, allowing infinite free crafting.
  - Added `outputSetBySystem` flag (set during `UpdateCraftingOutput()`) and `isHandlingOutputTaken` re-entrancy guard.
  - `OnSlotModified()` now detects when the output slot is emptied by the player (not by the system) and calls `HandleOutputTaken()`, which consumes one unit from each occupied crafting grid slot before refreshing the output preview.

- **Portable Crafting Table Block Info — Hardcoded Text** — `BlockPortableCraftingTable.GetPlacedBlockInfo()`:
  - The storage-usage tooltip was a hardcoded English string.
  - Now uses `Lang.Get("vintageessentials:crafttable-storageinfo", usedSlots, totalSlots)` for proper localization support.

### Added
- **Localization key** `vintageessentials:crafttable-storageinfo` in `en.json` for the placed block storage info tooltip ("Storage: {0}/{1} slots used\nShift+Right-click to pick up").

### Fixed
- **Invalid JSON in Storage Patches** — `assets/vintageessentials/patches/blocktypes/storage.json`:
  - Contained C++ style `//` comments which are invalid in JSON
  - Container slot patches (chests, storage vessels, crates) were silently failing to load
  - Replaced with JSON-compliant `_comment` fields

- **Slot Locking Null Reference** — `InventoryLockDialog`:
  - `TryToggleSlotLock()`, `ToggleLockingMode()`, and `IsSlotLocked()` could crash with a `NullReferenceException` if called before the player entity was initialized
  - Added null safety checks (`capi?.World?.Player`) in all methods that access `PlayerUID`

- **Slot Locking Logic Error** — `InventoryLockDialog.TryToggleSlotLock()`:
  - After unlocking a slot, the code checked `IsSlotLocked()` again which was always false, making the max-slots-reached feedback unreachable
  - Now correctly detects whether a lock attempt failed due to the max slots limit by comparing state before and after the toggle

- **Memory Leak on Dialog Close** — `ChestRadiusInventoryDialog.OnGuiClosed()`:
  - Nearby container references and slot lists were not cleared when the dialog closed, keeping stale references alive
  - Now clears `nearbyContainers`, `allSlots`, `filteredSlots`, and `displaySlotToActualSlot` on close

### Added
- **Phase 6: Polish & Testing** — UI refinement, bug fixes, and localization consistency:
  - All hardcoded UI strings replaced with `Lang.Get()` localization calls across all dialog classes
  - Fully functional scrollbar in Chest Radius Inventory dialog (previously a non-functional stub)

### Fixed
- **Inventory Restoration Logging** — `BlockEntityPortableCraftingTable.RestoreInventoryFromItemStack()`:
  - Bare `catch {}` replaced with proper error logging (`Api.World.Logger.Error`) per slot
  - Failed slot restorations now report the slot index and exception message

- **DummySlot Transfer Calculation** — `DummySlot.TryPutInto()` in Chest Radius Inventory:
  - Was using `Math.Min(this.StackSize, this.StackSize)` which always returns the same value
  - Now correctly computes the actual number of items transferred by comparing sink slot before/after

- **Chest Radius Scrollbar** — `ChestRadiusInventoryDialog.UpdateScrollbar()`:
  - Was a stub with only a comment; scroll bar never updated in response to content changes
  - Now calls `scrollbar.SetHeights()` to properly reflect visible vs total rows

### Changed
- **Localization Consistency** — All user-facing strings now use `Lang.Get()` with keys from `en.json`:
  - `ChestRadiusInventoryDialog`: title, button labels, search placeholder, status messages
  - `InventoryLockDialog`: locking mode messages, slot lock/unlock feedback, max slot warnings
  - `PlayerInventorySortDialog`: no-items and sort-complete messages
  - `ModConfigDialog`: title bar, labels, button text, keybind capture prompts, save confirmation
  - `KeybindConflictDialog`: header, title, buttons, feature descriptions, conflict messages
  - `VintageEssentialsClientSystem`: loaded message, locking mode callbacks, keybind update message

- Updated PORTABLE_CRAFTING_SPEC.md to reflect Phase 6 completion
  - "Handbook" button in the crafting table dialog opens the in-game handbook
  - `HandbookIntegration` helper class with recipe lookup by output item
  - `AutoFillRecipe()` method to automatically fill crafting grid from a selected recipe
  - Ingredient availability checking before attempting auto-fill
  - Crafting grid clearing with items returned to table storage

- **Block Portability (Phase 5)** - Pick up and carry the Portable Crafting Table:
  - Shift+right-click to pick up the crafting table block as an item
  - All inventory contents (storage + crafting grid) preserved in the item stack
  - Inventory automatically restored when the block is placed back down
  - Block interaction help text showing pickup instructions
  - `SaveInventoryToItemStack()` / `RestoreInventoryFromItemStack()` serialization methods
  - `PreventDropOnRemoval` flag prevents double-dropping during pickup

- **Portable Crafting Table (Phase 1)** - New placeable block with internal storage:
  - `BlockPortableCraftingTable` block class with interaction handling
  - `BlockEntityPortableCraftingTable` block entity with 72-slot (12×6) internal storage
  - `PortableCraftingTableDialog` GUI showing table storage and player inventory
  - Block type JSON definition with 3D model and crafting recipe
  - Recipe: 6 planks + 2 sticks + 1 iron ingot
  - Items drop when block is broken
  - Inventory persists via NBT serialization

- **Portable Crafting Table (Phase 2 — Storage Integration)**:
  - 3×3 crafting grid with recipe output slot in the dialog
  - Block entity now holds 72 storage + 9 crafting grid + 1 output = 82 inventory slots
  - Real-time recipe matching — placing items in the grid instantly shows the output
  - Taking the output consumes one unit of each ingredient
  - Items can be freely transferred between crafting grid, table storage, and player inventory

- **Cloud Crafting System (Phase 3)**:
  - "Pull Ingredients" button in the Portable Crafting Table dialog
  - Searches table's own storage first, then nearby containers (15-block radius)
  - `CloudCraftingSystem` static helper class for ingredient scanning and gathering
  - `RefillCraftingGrid()` utility to top up crafting grid stacks for batch crafting

- **Localization** - Added English language file (`assets/vintageessentials/lang/en.json`):
  - All UI strings for chest radius inventory, sorting, slot locking, and configuration dialogs
  - Portable crafting table block name and description
  - Keybind conflict resolution strings

- **Keybind Customization System** - Full GUI-based keybind configuration:
  - All keybinds can now be customized directly from the mod settings dialog
  - No need to go to game Options > Controls menu
  - Real-time keybind conflict detection against all game and mod hotkeys
  - Interactive conflict resolution dialog prompts users to change conflicting keybinds
  - Support for Ctrl, Shift, and Alt modifiers
  - All keybind settings are saved globally and persist across sessions
  
- **Global Stack Size Multiplier** - Configurable stack sizes for all items:
  - Slider control in mod settings (1x to 200x multiplier)
  - Applies to items that initially stack up to 10 (tools, equipment, etc.)
  - Default 100x multiplier (10-stack items become 1000-stack)
  - Changes require game restart to take effect
  - Provides fine-grained control over inventory management

- **Enhanced Mod Configuration Dialog**:
  - Interactive keybind editing with visual feedback
  - Stack size multiplier slider with real-time preview
  - Key capture functionality for easy keybind assignment
  - Clear indication of current keybind settings
  - Information about when restart is required

- **MODDING_GUIDELINES.md** - Comprehensive modding guidelines document covering:
  - Asset system structure and organization
  - JSON patching best practices with case sensitivity and wildcard limitations
  - Code modding patterns and ModSystem lifecycle methods
  - Common issues and solutions
  - Troubleshooting guide with development environment setup
  - Links to official documentation and community resources

- **ASSET_GENERATION.md** - Detailed asset creation guide including:
  - Texture creation specifications and best practices
  - 3D model creation with VS Model Creator
  - Complete item and block asset structure examples
  - Recipe creation for all recipe types (grid, shapeless, smithing, barrel)
  - Sound asset integration guidelines
  - Localization best practices

### Fixed
- **Player Inventory Sort** - Fixed two compilation bugs:
  - Removed duplicate `int slotIndex = 0;` variable declaration
  - Fixed undefined `lockedSlots` variable to properly retrieve locked slots from `LockedSlotsManager` using the player's UID

- **Inventory Slot Click Handler** - Implemented `DetermineClickedSlot()`:
  - Was a placeholder that always returned null, preventing slot locking from working
  - Now searches through open GUI dialogs to find the character/inventory dialog
  - Accesses slot grid elements and checks slot bounds against mouse position
  - Supports multiple common slot grid element key names for compatibility

- **Locked Slots HUD Overlay** - Implemented `RenderLockedSlots()`:
  - Was commented-out placeholder code with no functional rendering
  - Now renders yellow diagonal stripe overlay textures at actual slot positions
  - Uses the dialog composer's slot grid bounds for accurate positioning

- **Stack Size Implementation** - Migrated from broken JSON patches to proper code-based patching:
  - **Issue**: Previous implementation used wildcards in JSON patches (`game:itemtypes/*`) which are NOT supported by Vintage Story
  - **Issue**: Property name was case-sensitive (`maxstacksize` not `maxStackSize`)
  - **Solution**: Implemented proper stack size patching in `AssetsFinalize()` method using code-based asset patching
  - Stack sizes now correctly apply to all collectibles (items and blocks) up to 1000

- **Storage Container Patches** - Fixed JSON patch file for containers:
  - **Issue**: Used wildcard patterns (`chest-*.json`, `storagevessel-*.json`) which are NOT supported
  - **Solution**: Explicitly listed each chest variant (north, east, south, west) and storage vessel variant
  - Added inline comments explaining why wildcards don't work
  - Corrected file paths to match actual game asset structure (`game:blocktypes/wood/generic/chest-north`)
  
### Changed
- **Stack Size Implementation** - Now configurable via mod settings:
  - Previously hardcoded to 1000 max stack for all items
  - Now uses configurable multiplier (default 100x) from mod settings
  - Applies multiplier only to items with initial stack size ≤10 (as per design)
  - Items with higher initial stacks also get multiplied but capped at 10000
  - Provides player choice for inventory management style
  
- Updated README.md with new keybind customization and stack size features
- Added technical notes section explaining implementation choices
- Documented the proper approach for bulk asset modifications

### Technical Notes

**Why Code-Based Patching for Stack Sizes?**

The mod originally attempted to use JSON patches with wildcards:
```json
{
  "op": "replace",
  "path": "/maxstacksize",
  "value": 1000,
  "file": "game:itemtypes/*"  // ❌ Wildcards NOT supported!
}
```

This doesn't work because:
1. Vintage Story's JSON patching system does NOT support wildcards in file paths
2. Each file must be specified explicitly, which is impractical for hundreds of items
3. The property name is case-sensitive (must be `maxstacksize` not `maxStackSize`)

The correct approach is code-based patching:
```csharp
public override void AssetsFinalize(ICoreAPI api)
{
    foreach (var collectible in api.World.Collectibles)
    {
        if (collectible != null && collectible.MaxStackSize < 1000)
        {
            collectible.MaxStackSize = 1000;
        }
    }
}
```

**Benefits of Code-Based Patching:**
- Can modify ALL items/blocks without listing each one
- More maintainable for large-scale changes
- Better performance than hundreds of individual JSON patches
- Allows conditional logic and complex modifications

**When to Use JSON Patches vs Code Patching:**

Use **JSON patches** when:
- Modifying a small number of specific assets
- Making simple property changes
- Don't need conditional logic
- Want to keep changes declarative

Use **Code-based patching** when:
- Modifying many assets at once (bulk changes)
- Need conditional logic (e.g., only change items below a threshold)
- Making complex modifications
- Wildcards would be needed (which aren't supported)

## [1.2.0] - Previous Release

### Features
- `/sethome` and `/home` commands
- `/rtp` random teleport command
- Chest Radius Inventory (press R)
- Player Inventory Sorting (Shift+S)
- Inventory Slot Locking (Ctrl+L)
- Mod Configuration Dialog (Ctrl+Shift+V)
- Stack size increases (attempted via JSON patches - now fixed)
- Storage container capacity increases

---

**Version Format**: This project follows [Semantic Versioning](https://semver.org/).

**Categories**: Added, Changed, Deprecated, Removed, Fixed, Security
