# Changelog

All notable changes to the Vintage Essentials mod will be documented in this file.

## [Unreleased]

### Added
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
