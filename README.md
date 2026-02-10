# VINTAGE-ESSENTIALS

A multi-mod repository for Vintage Story modding. Each mod lives in its own folder under `mods/`, making it easy to develop, reference, and manage individual mods independently.

## Repository Structure

```
VINTAGE-ESSENTIALS/
├── mods/
│   └── VintageEssentials/      # Our first mod — quality of life improvements
│       ├── src/                 # C# source code
│       ├── assets/              # Game assets (patches, shapes, textures, etc.)
│       ├── modinfo.json         # Mod metadata
│       ├── VintageEssentials.csproj  # Build project
│       ├── CHANGELOG.md         # Mod-specific changelog
│       └── PORTABLE_CRAFTING_SPEC.md # Feature spec for portable crafting
├── MODDING_GUIDELINES.md        # Shared modding reference (all mods)
├── ASSET_GENERATION.md          # Shared asset creation guide (all mods)
├── LICENSE
└── README.md                    # This file
```

## Mods

| Mod | Description | Folder |
|-----|-------------|--------|
| [Vintage Essentials](mods/VintageEssentials/) | Essential commands and quality of life improvements including `/home`, `/sethome`, `/rtp`, chest radius inventory, inventory sorting, slot locking, and configurable stack sizes | `mods/VintageEssentials/` |

## Working on a Mod

Each mod is self-contained in its own folder. To work on a specific mod, navigate to its folder:

```bash
cd mods/VintageEssentials
dotnet build
```

To add a new mod, create a new folder under `mods/`:

```bash
mkdir mods/YourNewMod
```

Then set up the standard Vintage Story mod structure inside it (see [MODDING_GUIDELINES.md](MODDING_GUIDELINES.md) for details).

## Shared Modding Resources

These reference documents apply to all mods in the repository:

- **[MODDING_GUIDELINES.md](MODDING_GUIDELINES.md)** — Comprehensive Vintage Story modding guidelines including asset system structure, JSON patching best practices, code modding patterns, and troubleshooting
- **[ASSET_GENERATION.md](ASSET_GENERATION.md)** — Detailed asset creation guide covering textures, 3D models, items, blocks, recipes, sounds, and localization

## License

See [LICENSE](LICENSE) file for details.
