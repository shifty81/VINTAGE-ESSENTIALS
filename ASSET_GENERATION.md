# Vintage Story Asset Generation Guide

A comprehensive guide for creating and managing assets in Vintage Story mods.

## Table of Contents

1. [Asset Types Overview](#asset-types-overview)
2. [Texture Creation](#texture-creation)
3. [Model Creation](#model-creation)
4. [Item Assets](#item-assets)
5. [Block Assets](#block-assets)
6. [Recipe Assets](#recipe-assets)
7. [Sound Assets](#sound-assets)
8. [Localization](#localization)
9. [Best Practices](#best-practices)

---

## Asset Types Overview

Vintage Story supports multiple asset types, each with its own purpose and structure:

| Asset Type | Folder | Description |
|------------|--------|-------------|
| Blocks | `blocktypes/` | Placeable blocks in the world |
| Items | `itemtypes/` | Items in inventory |
| Shapes | `shapes/` | 3D models (.json format) |
| Textures | `textures/` | Images (.png format) |
| Recipes | `recipes/` | Crafting recipes |
| Sounds | `sounds/` | Audio files (.ogg format) |
| Language | `lang/` | Translations (.json format) |
| Patches | `patches/` | Modifications to existing assets |

### Asset Naming Conventions

- Use **lowercase** with **hyphens** for multi-word names
- Be **descriptive** and **consistent**
- Avoid spaces and special characters
- Examples:
  - ✅ `iron-sword`, `oak-planks`, `leather-boots`
  - ❌ `IronSword`, `oak_planks`, `Leather Boots`

---

## Texture Creation

### Specifications

- **Format**: PNG with transparency support
- **Resolution**: Typically 16x16 or 32x32 pixels (power of 2)
- **Color Space**: RGB or RGBA
- **Compression**: PNG compression is fine

### Texture Organization

```
assets/modid/textures/
├── block/
│   ├── stone/
│   ├── wood/
│   └── metal/
├── item/
│   ├── tool/
│   ├── weapon/
│   └── resource/
└── entity/
    └── creature/
```

### Texture Atlas

Vintage Story uses texture atlases for performance. Your textures are automatically added to the atlas.

### Creating Textures

1. **Use appropriate size**: 16x16 for simple items, 32x32 for detailed items
2. **Match vanilla style**: Keep consistent with game art style
3. **Use transparency**: PNG alpha channel for non-rectangular items
4. **Test in-game**: View textures at different scales and lighting
5. **Optimize**: Keep file sizes reasonable

### Example Texture Reference in JSON

```json
{
  "texture": {
    "base": "block/stone/granite"
  }
}
```

Or for multiple textures:

```json
{
  "textures": {
    "up": { "base": "block/wood/top" },
    "down": { "base": "block/wood/top" },
    "north": { "base": "block/wood/side" },
    "south": { "base": "block/wood/side" },
    "east": { "base": "block/wood/side" },
    "west": { "base": "block/wood/side" }
  }
}
```

---

## Model Creation

### VS Model Creator

The official **VS Model Creator** is the recommended tool for creating 3D models:

- Built specifically for Vintage Story
- Ensures compatibility with the game engine
- Supports animations
- Exports to VS JSON format

### Shape File Structure

Shapes are 3D models stored as JSON files in `assets/modid/shapes/`:

```json
{
  "textureWidth": 16,
  "textureHeight": 16,
  "textures": {
    "wood": "block/wood/oak"
  },
  "elements": [
    {
      "name": "Base",
      "from": [0, 0, 0],
      "to": [16, 1, 16],
      "faces": {
        "up": { "texture": "#wood", "uv": [0, 0, 16, 16] },
        "down": { "texture": "#wood", "uv": [0, 0, 16, 16] }
      }
    }
  ]
}
```

### Model Types

1. **Block Models**: Placed in world, typically 1x1x1 meter
2. **Item Models**: Held in hand or inventory
3. **Entity Models**: Creatures and NPCs (more complex)

### Referencing Shapes in JSON

```json
{
  "shape": {
    "base": "block/wood/chest"
  }
}
```

Or with rotations:

```json
{
  "shapeByType": {
    "chest-north": { "base": "block/wood/chest", "rotateY": 0 },
    "chest-east": { "base": "block/wood/chest", "rotateY": 90 },
    "chest-south": { "base": "block/wood/chest", "rotateY": 180 },
    "chest-west": { "base": "block/wood/chest", "rotateY": 270 }
  }
}
```

---

## Item Assets

### Basic Item Structure

File: `assets/modid/itemtypes/myitem.json`

```json
{
  "code": "myitem",
  "class": "Item",
  "maxstacksize": 64,
  "durability": 100,
  "creativeinventory": {
    "general": ["*"],
    "items": ["*"]
  },
  "guiTransform": {
    "translation": { "x": 0, "y": 0, "z": 0 },
    "rotation": { "x": 0, "y": 45, "z": 0 },
    "scale": 1.0
  },
  "fpHandTransform": {
    "translation": { "x": 0, "y": 0, "z": 0 },
    "rotation": { "x": 0, "y": 0, "z": 0 },
    "scale": 1.0
  },
  "texture": {
    "base": "item/myitem"
  },
  "shape": {
    "base": "item/myitem"
  }
}
```

### Important Item Properties

- **`code`**: Unique identifier (lowercase, hyphenated)
- **`class`**: Item class (default: `"Item"`, or custom C# class)
- **`maxstacksize`**: Maximum items per slot (default: 64)
- **`durability`**: Tool/weapon durability (optional)
- **`creativeinventory`**: Which creative tabs it appears in
- **`texture`**: Path to texture file
- **`shape`**: Path to 3D model

### Tool/Weapon Items

```json
{
  "code": "pickaxe-copper",
  "class": "Item",
  "tool": "pickaxe",
  "maxstacksize": 1,
  "durability": 150,
  "damagetier": 1,
  "attackpower": 2.0,
  "attackrange": 2.5,
  "miningspeed": {
    "stone": 3.0
  },
  "texture": {
    "base": "item/tool/pickaxe-copper"
  }
}
```

### Food Items

```json
{
  "code": "bread",
  "class": "Item",
  "maxstacksize": 64,
  "nutritionprops": {
    "satiety": 160,
    "foodcategory": "Grain"
  },
  "transitionablePropsByType": {
    "*": [
      {
        "type": "Perish",
        "freshHours": { "avg": 480 },
        "transitionHours": { "avg": 72 },
        "transitionedStack": { "type": "item", "code": "rot" }
      }
    ]
  },
  "texture": {
    "base": "item/food/bread"
  }
}
```

---

## Block Assets

### Basic Block Structure

File: `assets/modid/blocktypes/myblock.json`

```json
{
  "code": "myblock",
  "class": "Block",
  "maxstacksize": 64,
  "creativeinventory": {
    "general": ["*"],
    "decorative": ["*"]
  },
  "blockmaterial": "Stone",
  "drawtype": "Cube",
  "resistance": 3.5,
  "lightAbsorption": 1,
  "textures": {
    "all": {
      "base": "block/myblock"
    }
  },
  "sounds": {
    "place": "game:block/dirt",
    "break": "game:block/dirt",
    "hit": "game:block/dirt",
    "walk": "game:block/dirt"
  }
}
```

### Important Block Properties

- **`blockmaterial`**: Material type (affects tools, sounds)
  - Stone, Wood, Metal, Gravel, Sand, etc.
- **`drawtype`**: Rendering method
  - Cube, JSON (custom shape), Transparent, etc.
- **`resistance`**: Mining resistance/hardness
- **`lightAbsorption`**: How much light it blocks (0-99)
- **`lightHsv`**: Light emission (if block emits light)

### Block with Custom Shape

```json
{
  "code": "barrel",
  "class": "BlockBarrel",
  "entityclass": "Barrel",
  "drawtype": "JSON",
  "shape": {
    "base": "block/wood/barrel"
  },
  "textures": {
    "wood": {
      "base": "block/wood/oak"
    }
  },
  "attributes": {
    "capacityLitres": 50
  }
}
```

### Storage Container Block

```json
{
  "code": "chest",
  "class": "BlockGenericTypedContainer",
  "entityclass": "GenericContainer",
  "attributes": {
    "quantitySlots": 16,
    "dialogTitleLangCode": "game:block-chest-north"
  },
  "shape": {
    "base": "block/wood/chest"
  }
}
```

---

## Recipe Assets

### Grid Recipes

File: `assets/modid/recipes/grid/myrecipe.json`

```json
{
  "ingredientPattern": "PP,SS",
  "ingredients": {
    "P": {
      "type": "item",
      "code": "game:plank-oak",
      "qty": 1
    },
    "S": {
      "type": "item",
      "code": "game:stick",
      "qty": 1
    }
  },
  "width": 2,
  "height": 2,
  "output": {
    "type": "item",
    "code": "modid:woodentool",
    "qty": 1
  }
}
```

### Shapeless Recipes

```json
{
  "ingredients": [
    {
      "type": "item",
      "code": "game:flax",
      "qty": 3
    },
    {
      "type": "item",
      "code": "game:stick",
      "qty": 1
    }
  ],
  "output": {
    "type": "item",
    "code": "modid:rope",
    "qty": 1
  }
}
```

### Smithing Recipes

File: `assets/modid/recipes/smithing/myrecipe.json`

```json
{
  "ingredient": {
    "type": "item",
    "code": "game:ingot-copper"
  },
  "output": {
    "type": "item",
    "code": "game:pickaxe-copper"
  },
  "voxels": "##...,##...,##...,##...,#....,#...."
}
```

### Barrel Recipes

```json
{
  "code": "brine",
  "ingredients": [
    {
      "type": "item",
      "code": "game:waterportion",
      "litres": 10
    },
    {
      "type": "item",
      "code": "game:salt",
      "qty": 1
    }
  ],
  "output": {
    "type": "item",
    "code": "game:brine",
    "litres": 10
  },
  "sealHours": 24
}
```

---

## Sound Assets

### Sound Format

- **Format**: OGG Vorbis
- **Sample Rate**: 44.1 kHz recommended
- **Channels**: Mono or Stereo

### Sound Organization

```
assets/modid/sounds/
├── block/
│   ├── break/
│   └── place/
├── player/
│   └── footstep/
└── effect/
    └── ambient/
```

### Referencing Sounds

In block JSON:

```json
{
  "sounds": {
    "place": "modid:block/place/wood",
    "break": "modid:block/break/wood",
    "hit": "modid:block/hit/wood",
    "walk": "modid:block/walk/wood"
  }
}
```

In code:

```csharp
api.World.PlaySoundAt(
    new AssetLocation("modid:sounds/effect/explosion"),
    pos.X, pos.Y, pos.Z,
    null, true, 32, 1.0f
);
```

---

## Localization

### Language Files

File: `assets/modid/lang/en.json`

```json
{
  "item-myitem": "My Item",
  "item-myitem-tooltip": "A useful item",
  "block-myblock": "My Block",
  "block-myblock-tooltip": "A decorative block",
  "game:tabname-modid": "My Mod"
}
```

### Translation Keys

- Items: `item-{code}`
- Blocks: `block-{code}`
- Tooltips: `{type}-{code}-tooltip`
- Custom: `modid:{custom-key}`

### Supported Languages

Create files for each language:
- `en.json` - English
- `de.json` - German
- `fr.json` - French
- `ru.json` - Russian
- `es.json` - Spanish
- etc.

---

## Best Practices

### Asset Creation Workflow

1. **Plan First**: Sketch or prototype your asset
2. **Create Texture**: Design in 16x16 or 32x32 resolution
3. **Model (if needed)**: Use VS Model Creator for 3D shapes
4. **Write JSON**: Define properties and references
5. **Test Early**: Load in-game and iterate
6. **Validate**: Check JSON syntax and paths
7. **Optimize**: Ensure reasonable file sizes

### Organization Tips

- **Group related assets**: Keep similar items together
- **Use subdirectories**: Organize by type (tools, weapons, food)
- **Consistent naming**: Follow a naming convention throughout
- **Document variants**: Use clear names for variants (copper-pickaxe, iron-pickaxe)

### Performance Considerations

- **Texture size**: Don't use unnecessarily large textures
- **Model complexity**: Keep vertex counts reasonable
- **Sound files**: Compress audio appropriately
- **Recipe efficiency**: Avoid overly complex recipe chains

### Compatibility

- **Avoid vanilla overrides**: Use patches or code-based modification
- **Namespace properly**: Always use your mod domain
- **Test with other mods**: Ensure no conflicts
- **Document dependencies**: List required mods in modinfo.json

### Quality Checklist

- [ ] All textures are properly sized and formatted
- [ ] Models are created with VS Model Creator
- [ ] JSON files are validated and error-free
- [ ] Asset names follow conventions (lowercase, hyphenated)
- [ ] Localization files include all necessary translations
- [ ] Sounds are in OGG format
- [ ] Assets are organized in proper directories
- [ ] No duplicate IDs or names
- [ ] Tested in-game at different scales
- [ ] File sizes are optimized

---

## Testing Your Assets

### In-Game Testing

1. **Build your mod**: `dotnet build`
2. **Copy to mods folder**: Place `.zip` in `VintagestoryData/Mods`
3. **Launch game**: Start Vintage Story
4. **Check Mod Manager**: Verify mod loaded successfully
5. **Test in creative**: Use creative mode to access items
6. **Test functionality**: Verify all features work as expected

### Common Issues

**Textures not showing:**
- Check file path matches JSON reference
- Verify PNG format and transparency
- Ensure file is in correct directory

**Models not loading:**
- Validate JSON shape file syntax
- Check texture references in shape file
- Verify shape path in asset JSON

**Items not appearing:**
- Check creativeinventory settings
- Verify code matches file name (minus .json)
- Check mod is enabled in Mod Manager

**Recipes not working:**
- Validate recipe JSON syntax
- Check ingredient codes are correct
- Verify output code exists

---

## Advanced Topics

### Variants

Use `*` in code to create multiple variants:

```json
{
  "codebytype": {
    "oak": "plank-oak",
    "birch": "plank-birch",
    "pine": "plank-pine"
  },
  "texturesByType": {
    "oak": { "all": { "base": "block/wood/oak" } },
    "birch": { "all": { "base": "block/wood/birch" } },
    "pine": { "all": { "base": "block/wood/pine" } }
  }
}
```

### Animations

Define in shape file:

```json
{
  "animations": [
    {
      "name": "idle",
      "code": "idle",
      "quantityframes": 30,
      "frametime": 33
    }
  ]
}
```

### Custom Block Behavior

Reference custom C# class:

```json
{
  "code": "customblock",
  "class": "ModNamespace.BlockCustom"
}
```

Then implement in code:

```csharp
public class BlockCustom : Block
{
    public override bool OnBlockInteractStart(
        IWorldAccessor world,
        IPlayer byPlayer,
        BlockSelection blockSel)
    {
        // Custom interaction logic
        return true;
    }
}
```

---

## Resources

- [VS Model Creator Tutorial](https://www.vintagestory.at/)
- [Asset Type Documentation](https://wiki.vintagestory.at/Modding:Asset_Type)
- [JSON Reference](https://apidocs.vintagestory.at/json-docs/)
- [Texture Atlas Info](https://wiki.vintagestory.at/Modding:Textures)
- [Recipe System](https://wiki.vintagestory.at/Modding:Recipes)

---

**Version**: 1.0.0  
**Last Updated**: December 2024  
**Compatible with**: Vintage Story 1.19.0+
