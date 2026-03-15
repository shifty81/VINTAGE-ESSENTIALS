<p align="center">
  <img src="assets/banner.svg" alt="Vintage Story Modding — Vintage Essentials" width="100%" />
</p>

<p align="center">
  <a href="mods/VintageEssentials/"><img src="https://img.shields.io/badge/Vintage_Story-Mod-8B6914?style=for-the-badge" alt="VS Mod" /></a>
  <a href="#"><img src="https://img.shields.io/badge/C%23-.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt="C# .NET" /></a>
  <a href="LICENSE"><img src="https://img.shields.io/badge/License-MIT-green?style=for-the-badge" alt="License" /></a>
  <a href="mods/VintageEssentials/CHANGELOG.md"><img src="https://img.shields.io/badge/Version-1.6.0-blue?style=for-the-badge" alt="Version" /></a>
</p>

<p align="center">
  <i>🏗️ A multi-mod repository for Vintage Story — quality of life improvements, crafting enhancements, and essential commands.</i>
</p>

---

## ✨ Features at a Glance

<table>
<tr>
<td width="50%">

### 🏠 Teleportation Commands
- **`/sethome`** — Save your home location
- **`/home`** — Warp back instantly
- **`/rtp <direction>`** — Random teleport 10k–20k blocks

</td>
<td width="50%">

### 🔨 Portable Crafting Table
- Placeable 3×3 crafting grid with 72-slot storage
- **Cloud Crafting** — pull ingredients from nearby chests
- **Handbook Integration** — browse & auto-fill recipes
- Pick up with contents preserved

</td>
</tr>
<tr>
<td width="50%">

### 📦 Chest Radius Inventory
- Scan nearby containers (15-block radius)
- Search, sort, deposit all, take all
- Real-time item aggregation

</td>
<td width="50%">

### ⚙️ Quality of Life
- **Inventory Sorting** — sort by name, quantity, or type
- **Slot Locking** — protect important items
- **Stack Size Multiplier** — configurable up to 200×
- **Keybind Customization** — full GUI configuration

</td>
</tr>
</table>

---

## 📂 Repository Structure

```
VINTAGE-ESSENTIALS/
├── mods/
│   └── VintageEssentials/          🎮 Quality of life mod
│       ├── src/                    📝 C# source code
│       ├── assets/                 🎨 Game assets (patches, shapes, lang)
│       ├── modinfo.json            📋 Mod metadata
│       ├── VintageEssentials.csproj
│       ├── CHANGELOG.md
│       └── PORTABLE_CRAFTING_SPEC.md
├── assets/                         🖼️ Repository assets (banner, images)
├── MODDING_GUIDELINES.md           📖 Shared modding reference
├── ASSET_GENERATION.md             🎨 Asset creation guide
├── LICENSE
└── README.md
```

## 🎮 Mods

| Mod | Description | Folder |
|:---:|-------------|:------:|
| **[Vintage Essentials](mods/VintageEssentials/)** | Essential commands and quality of life improvements including `/home`, `/sethome`, `/rtp`, chest radius inventory, inventory sorting, slot locking, configurable stack sizes, and portable crafting table with cloud crafting, handbook integration, and block portability | `mods/VintageEssentials/` |

## 🚀 Getting Started

Each mod is self-contained in its own folder. To work on a specific mod:

```bash
cd mods/VintageEssentials
dotnet build
```

To add a new mod, create a new folder under `mods/`:

```bash
mkdir mods/YourNewMod
```

Then set up the standard Vintage Story mod structure inside it (see [MODDING_GUIDELINES.md](MODDING_GUIDELINES.md) for details).

## 📖 Shared Modding Resources

These reference documents apply to all mods in the repository:

| Resource | Description |
|:---------|:------------|
| 📘 **[MODDING_GUIDELINES.md](MODDING_GUIDELINES.md)** | Comprehensive modding guidelines — asset system, JSON patching, code patterns, troubleshooting |
| 🎨 **[ASSET_GENERATION.md](ASSET_GENERATION.md)** | Asset creation guide — textures, 3D models, items, blocks, recipes, sounds, localization |

## 📜 License

See [LICENSE](LICENSE) file for details.
