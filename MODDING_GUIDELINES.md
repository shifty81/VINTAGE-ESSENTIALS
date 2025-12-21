# Vintage Story Modding Guidelines

This document provides comprehensive guidelines for modding Vintage Story, covering asset generation, code structure, and best practices based on official documentation and community experience.

## Table of Contents

1. [Asset System Structure](#asset-system-structure)
2. [JSON Patching](#json-patching)
3. [Code Modding Best Practices](#code-modding-best-practices)
4. [Asset Generation](#asset-generation)
5. [Common Issues and Solutions](#common-issues-and-solutions)
6. [Troubleshooting](#troubleshooting)

---

## Asset System Structure

### Directory Organization

All game assets are organized under an `assets/` folder. Each mod should have its own domain (folder) to avoid conflicts:

```
assets/
├── modid/
│   ├── blocktypes/
│   ├── itemtypes/
│   ├── shapes/
│   ├── textures/
│   ├── sounds/
│   ├── recipes/
│   ├── patches/
│   └── lang/
```

### Best Practices

- **Use your own mod domain**: Always use `assets/yourmodid/` to maintain compatibility with other mods
- **Lowercase naming**: Use lowercase, hyphenated names for mod IDs (e.g., `vintageessentials`, not `VintageEssentials`)
- **Descriptive names**: Use semantic and descriptive names for assets
- **Avoid ID conflicts**: Never duplicate IDs or asset names between mods

### modinfo.json

The modinfo.json is crucial for your mod. Include all relevant fields:

```json
{
  "type": "code",
  "modid": "yourmodid",
  "name": "Your Mod Name",
  "author": "YourName",
  "description": "A clear description of what your mod does",
  "version": "1.0.0",
  "dependency": {
    "game": "1.19.0"
  },
  "side": "Universal",
  "requiredOnClient": true,
  "requiredOnServer": true
}
```

---

## JSON Patching

### Basic Patch Structure

JSON patches modify existing game assets without overwriting files:

```json
[
  {
    "op": "replace",
    "path": "/maxstacksize",
    "value": 1000,
    "file": "game:itemtypes/resource/stone"
  }
]
```

### Patch Operations

- **`add`**: Add a new property or array element
- **`replace`**: Replace an existing property value
- **`remove`**: Remove a property or array element
- **`addmerge`**: Add or merge objects
- **`addeach`**: Add multiple elements to an array
- **`move`**: Move a value from one path to another
- **`copy`**: Copy a value from one path to another

### Important Rules

1. **Case Sensitivity**: JSON paths are case-sensitive. Use exact casing from asset files.
   - Correct: `/maxstacksize`
   - Incorrect: `/MaxStackSize` or `/maxStackSize`

2. **No Wildcards**: Vintage Story's vanilla patching system does NOT support wildcards in file paths.
   - **Wrong**: `"file": "game:itemtypes/*"`
   - **Right**: Use code-based patching (see below) or list each file separately

3. **Path Format**: Paths start with `/` and use forward slashes
   - Example: `/attributes/quantitySlots`

4. **Patch Location**: Place patches in `assets/modid/patches/`

### Common Property Names

- **Stack Size**: `maxstacksize` (lowercase)
- **Storage Slots**: `/attributes/quantitySlots`
- **Durability**: `/durability`

### Example Patches

**Modify chest storage:**
```json
[
  {
    "op": "replace",
    "path": "/attributes/quantitySlots",
    "value": 32,
    "file": "game:blocktypes/wood/chest-north"
  }
]
```

**Add a new recipe ingredient:**
```json
[
  {
    "op": "add",
    "path": "/ingredients/-",
    "value": {
      "type": "item",
      "code": "game:flax",
      "qty": 1
    },
    "file": "game:recipes/grid/clothing/linen-normal"
  }
]
```

---

## Code Modding Best Practices

### ModSystem Structure

Your mod's entry point must extend `ModSystem`:

```csharp
using Vintagestory.API.Common;

namespace YourModNamespace
{
    public class YourModSystem : ModSystem
    {
        // Called on both client and server
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            // Register common initialization
        }

        // Called after all assets are loaded
        public override void AssetsFinalize(ICoreAPI api)
        {
            // Patch assets in code here
        }

        // Server-only initialization
        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);
            // Register server-only features
        }

        // Client-only initialization
        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);
            // Register UI, rendering, etc.
        }

        // Cleanup
        public override void Dispose()
        {
            // Unregister event handlers
            base.Dispose();
        }
    }
}
```

### Asset Patching in Code

For bulk modifications (like changing all item stack sizes), use code-based patching:

```csharp
public override void AssetsFinalize(ICoreAPI api)
{
    // Patch stack sizes for all collectibles
    foreach (var collectible in api.World.Collectibles)
    {
        if (collectible != null && collectible.MaxStackSize < 1000)
        {
            collectible.MaxStackSize = 1000;
        }
    }
}
```

**Why use code patching?**
- Can apply changes to ALL items/blocks without listing each one
- More maintainable for large-scale changes
- Better performance than hundreds of JSON patches
- Can use conditional logic

### Lifecycle Methods

1. **`Start(ICoreAPI api)`**: Entry point for both client and server
2. **`AssetsLoaded(ICoreAPI api)`**: Called when assets are available
3. **`AssetsFinalize(ICoreAPI api)`**: Best place to modify loaded assets
4. **`StartServerSide(ICoreServerAPI api)`**: Server-only logic
5. **`StartClientSide(ICoreClientAPI api)`**: Client-only logic (UI, rendering)
6. **`Dispose()`**: Cleanup resources and event handlers

### Best Practices

- **Separation of Concerns**: Keep server and client logic separate
- **Documentation**: Use XML comments for public APIs
- **Event Cleanup**: Always unregister events in `Dispose()`
- **Modularity**: Split large mods into multiple ModSystem classes
- **Naming**: Use PascalCase for classes and methods
- **Testing**: Test iteratively with small changes

---

## Asset Generation

### Textures and Models

1. **VS Model Creator**: Use the official VS Model Creator tool for creating models and animations
2. **Texture Location**: Place textures in `assets/modid/textures/` with subdirectories matching asset types
3. **Naming Convention**: Use clear, consistent names matching your JSON asset references
4. **Testing**: Test models and textures frequently in-game during development

### Creating New Items

**Example item JSON** (`assets/modid/itemtypes/customitem.json`):

```json
{
  "code": "customitem",
  "class": "Item",
  "maxstacksize": 64,
  "durability": 100,
  "creativeinventory": {
    "general": ["*"]
  },
  "texture": {
    "base": "item/customitem"
  },
  "shape": {
    "base": "item/customitem"
  }
}
```

### Creating New Blocks

**Example block JSON** (`assets/modid/blocktypes/customblock.json`):

```json
{
  "code": "customblock",
  "class": "Block",
  "maxstacksize": 64,
  "creativeinventory": {
    "general": ["*"]
  },
  "textures": {
    "all": {
      "base": "block/customblock"
    }
  },
  "shape": {
    "base": "block/cube"
  }
}
```

---

## Common Issues and Solutions

### Issue: Stack Size Changes Not Working

**Problem**: Using wildcard patches like `"file": "game:itemtypes/*"`

**Solution**: Use code-based patching in `AssetsFinalize()`:

```csharp
public override void AssetsFinalize(ICoreAPI api)
{
    foreach (var collectible in api.World.Collectibles)
    {
        if (collectible != null)
        {
            collectible.MaxStackSize = 1000;
        }
    }
}
```

### Issue: JSON Patch Not Applying

**Causes**:
1. Incorrect case sensitivity (`/MaxStackSize` vs `/maxstacksize`)
2. Wrong file path (check domain prefix: `game:` vs `yourmod:`)
3. Patch file not in correct location (`assets/modid/patches/`)
4. Invalid JSON syntax

**Solution**: 
- Validate JSON with tools like [json5.net](https://json5.net/)
- Check exact casing in original asset files
- Verify file paths match game assets
- Check game logs for patch errors

### Issue: Mod Not Loading

**Causes**:
1. Missing or invalid `modinfo.json`
2. Incorrect namespace or class structure
3. Missing DLL references
4. Incompatible game version

**Solution**:
- Verify `modinfo.json` is in the mod root
- Check `type` field matches mod type (`code` for C# mods)
- Ensure all required references are in `.csproj`
- Match `dependency.game` version to your Vintage Story version

### Issue: Container Slot Changes Not Working

**Problem**: Trying to patch storage vessel or chest slots

**Solution**: Use correct path and verify entityclass:

```json
[
  {
    "op": "replace",
    "path": "/attributes/quantitySlots",
    "value": 32,
    "file": "game:blocktypes/wood/chest-north"
  }
]
```

Note: You may need to patch multiple chest variants (north, east, south, west).

---

## Troubleshooting

### Development Environment

**Requirements**:
- .NET 7.0 or later SDK
- Visual Studio, Rider, or VS Code with C# extension
- Vintage Story 1.19.0 or later

**Environment Variables**:
- `VINTAGE_STORY`: Path to game installation
- `VINTAGE_STORY_DATA`: Path to game data directory

### Building

```bash
# Standard build
dotnet build

# Release build (optimized)
dotnet build -c Release
```

### Testing

1. Build your mod
2. Copy `.zip` file to `VintagestoryData/Mods`
3. Launch game
4. Check Mod Manager (Esc → Mod Manager)
5. Review logs in `Logs` folder for errors

### Common Build Errors

**"Could not find VintagestoryAPI.dll"**
- Set `VINTAGE_STORY` environment variable
- Verify path points to game installation directory

**"Framework version not found"**
- Install correct .NET SDK version
- Check `TargetFramework` in `.csproj`

### Debugging Tips

1. **Add logging**: Use `api.World.Logger.Event()` for debug output
2. **Check logs**: Review game logs in `Logs` folder
3. **Incremental testing**: Test small changes frequently
4. **Use ModMaker**: Built-in tool for creating and testing patches
5. **Community resources**: Visit forums and Discord for help

---

## Additional Resources

### Official Documentation

- [Vintage Story Wiki - Modding Portal](https://wiki.vintagestory.at/Modding:Content_Guides_Portal/en)
- [JSON API Reference](https://apidocs.vintagestory.at/json-docs/)
- [C# API Documentation](https://apidocs.vintagestory.at/)
- [Asset System](https://wiki.vintagestory.at/index.php/Modding:Asset_System)
- [JSON Patching](https://wiki.vintagestory.at/index.php/Modding:JSON_Patching)
- [Asset Patching in Code](https://wiki.vintagestory.at/Special:MyLanguage/Modding:Asset_Patching_in_Code)

### Community Resources

- [Official Forums](https://www.vintagestory.at/forums/)
- [Discord Server](https://discord.gg/vintagestory)
- [Example Mods Repository](https://github.com/anegostudios/vsmodexamples)
- [VS API Repository](https://github.com/anegostudios/vsapi)

### Tools

- **ModMaker 3000**: Built into Vintage Story for creating patches
- **VS Model Creator**: Official tool for creating 3D models
- [JsonPatchMaker](https://github.com/ApacheTech-VintageStory-Tools/JsonPatchMaker): Tool for generating patches
- [Gantry MDK](https://github.com/ApacheTech-Nuget-Packages/VintageStory.Gantry): Advanced modding development kit

---

## Summary Checklist

When creating a Vintage Story mod:

- [ ] Use your own mod domain (`assets/yourmodid/`)
- [ ] Create valid `modinfo.json` with correct version
- [ ] Use lowercase, hyphenated mod IDs
- [ ] Validate all JSON files
- [ ] Use case-sensitive property names
- [ ] Avoid wildcards in JSON patches (use code patching instead)
- [ ] Separate client and server logic
- [ ] Document your code
- [ ] Test incrementally
- [ ] Check game logs for errors
- [ ] Clean up resources in `Dispose()`

---

**Version**: 1.0.0  
**Last Updated**: December 2024  
**Compatible with**: Vintage Story 1.19.0+
