using System.Collections.Generic;
using System.Linq;

namespace VintageEssentials
{
    public enum ModCategory
    {
        Storage,
        Cooking,
        Hunting,
        Building,
        Crafting,
        Combat,
        WorldExploration,
        QualityOfLife,
        Libraries
    }

    public class ConglomerateModEntry
    {
        public string FileName { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public ModCategory Category { get; set; }
    }

    public static class ConglomerateModManager
    {
        public static readonly string[] CategoryNames = new string[]
        {
            "Storage & Inventory",
            "Cooking & Food",
            "Hunting & Animals",
            "Building & Decoration",
            "Crafting & Smithing",
            "Combat & Skills",
            "World & Exploration",
            "Quality of Life",
            "Libraries"
        };

        public static string GetCategoryName(ModCategory category)
        {
            return CategoryNames[(int)category];
        }

        public static List<ConglomerateModEntry> GetAllMods()
        {
            return new List<ConglomerateModEntry>
            {
                // Storage & Inventory
                new ConglomerateModEntry { FileName = "8xBackpackCapacityV0.0.1.zip", DisplayName = "8x Backpack Capacity", Description = "Increases backpack storage capacity by 8x", Category = ModCategory.Storage },
                new ConglomerateModEntry { FileName = "BetterCrates_v1.9.0.zip", DisplayName = "Better Crates", Description = "Improved crate storage with better UI", Category = ModCategory.Storage },
                new ConglomerateModEntry { FileName = "biggerpocketsv1.0.0.zip", DisplayName = "Bigger Pockets", Description = "Larger personal pocket storage", Category = ModCategory.Storage },
                new ConglomerateModEntry { FileName = "purposefulstorage_1.4.1.zip", DisplayName = "Purposeful Storage", Description = "Organized and labeled storage solutions", Category = ModCategory.Storage },
                new ConglomerateModEntry { FileName = "shelfobsessed_v1.6.0.zip", DisplayName = "Shelf Obsessed", Description = "Display shelves for showcasing items", Category = ModCategory.Storage },
                new ConglomerateModEntry { FileName = "sortablestorage_2.3.1.zip", DisplayName = "Sortable Storage", Description = "Add sorting to all storage containers", Category = ModCategory.Storage },
                new ConglomerateModEntry { FileName = "StorageOptions-v2.0.0.zip", DisplayName = "Storage Options", Description = "Additional storage container types", Category = ModCategory.Storage },
                new ConglomerateModEntry { FileName = "SpecialtyBagsRevived_1.1.0.zip", DisplayName = "Specialty Bags Revived", Description = "Special purpose bags for specific items", Category = ModCategory.Storage },
                new ConglomerateModEntry { FileName = "foodshelves_2.3.0.zip", DisplayName = "Food Shelves", Description = "Display shelves designed for food storage", Category = ModCategory.Storage },
                new ConglomerateModEntry { FileName = "temporal_gears_stack v1.zip", DisplayName = "Temporal Gears Stack", Description = "Makes temporal gears stackable", Category = ModCategory.Storage },

                // Cooking & Food
                new ConglomerateModEntry { FileName = "BetterFirepit-1.1.5.zip", DisplayName = "Better Firepit", Description = "Enhanced firepit cooking mechanics", Category = ModCategory.Cooking },
                new ConglomerateModEntry { FileName = "aculinaryartillerypatch_1.2.6-pre.4.zip", DisplayName = "Culinary Artillery Patch", Description = "Cooking mod compatibility patch", Category = ModCategory.Cooking },
                new ConglomerateModEntry { FileName = "expandedfoodspatch_1.7.6-pre.1.zip", DisplayName = "Expanded Foods Patch", Description = "Extended food recipes and options", Category = ModCategory.Cooking },
                new ConglomerateModEntry { FileName = "omnipan_1.0.0.zip", DisplayName = "Omni Pan", Description = "Universal cooking pan for all recipes", Category = ModCategory.Cooking },
                new ConglomerateModEntry { FileName = "stonebakeoven_1.2.0.zip", DisplayName = "Stone Bake Oven", Description = "Stone oven for bread and baked goods", Category = ModCategory.Cooking },
                new ConglomerateModEntry { FileName = "TankardsandGoblets_v1.2.1.zip", DisplayName = "Tankards and Goblets", Description = "Craftable drinking vessels", Category = ModCategory.Cooking },
                new ConglomerateModEntry { FileName = "apegrapes-v1.21.1-1.3.1.zip", DisplayName = "Ape Grapes", Description = "Grape cultivation and winemaking", Category = ModCategory.Cooking },
                new ConglomerateModEntry { FileName = "pipeleaf_2.1.0.zip", DisplayName = "Pipeleaf", Description = "Grow and process tobacco for pipes", Category = ModCategory.Cooking },

                // Hunting & Animals
                new ConglomerateModEntry { FileName = "butchering_1.10.5.zip", DisplayName = "Butchering", Description = "Enhanced animal butchering system", Category = ModCategory.Hunting },
                new ConglomerateModEntry { FileName = "butcheringxskillsxp_2.1.0.zip", DisplayName = "Butchering x Skills XP", Description = "Skill XP for butchering activities", Category = ModCategory.Hunting },
                new ConglomerateModEntry { FileName = "animalcages_v4.0.1.zip", DisplayName = "Animal Cages", Description = "Cages for capturing and transporting animals", Category = ModCategory.Hunting },
                new ConglomerateModEntry { FileName = "petai_v4.0.0.zip", DisplayName = "Pet AI", Description = "Tame and command pet companions", Category = ModCategory.Hunting },
                new ConglomerateModEntry { FileName = "FeverstoneHorses-v2.0.0-1.21.1.zip", DisplayName = "Feverstone Horses", Description = "Rideable horse mounts", Category = ModCategory.Hunting },
                new ConglomerateModEntry { FileName = "The_Critters_Pack_v1-3-3.zip", DisplayName = "The Critters Pack", Description = "Additional small wildlife creatures", Category = ModCategory.Hunting },
                new ConglomerateModEntry { FileName = "draconis_1.1.2.zip", DisplayName = "Draconis", Description = "Dragon creatures and encounters", Category = ModCategory.Hunting },
                new ConglomerateModEntry { FileName = "FromGoldenCombs-1.21-v1.9.4.zip", DisplayName = "From Golden Combs", Description = "Beekeeping and honey production", Category = ModCategory.Hunting },

                // Building & Decoration
                new ConglomerateModEntry { FileName = "kevinsfurniture_1.7.4.zip", DisplayName = "Kevin's Furniture", Description = "Craftable furniture and decor pieces", Category = ModCategory.Building },
                new ConglomerateModEntry { FileName = "Opdoorpack_0.0.1.zip", DisplayName = "OP Door Pack", Description = "Variety of decorative door types", Category = ModCategory.Building },
                new ConglomerateModEntry { FileName = "Mannequin-Stand-v1.0.7.zip", DisplayName = "Mannequin Stand", Description = "Armor and clothing display stands", Category = ModCategory.Building },
                new ConglomerateModEntry { FileName = "ndlwoodentorchholder_2.0.3.zip", DisplayName = "Wooden Torch Holder", Description = "Wall-mounted torch holders", Category = ModCategory.Building },
                new ConglomerateModEntry { FileName = "windchimes-V1.21.4-1.4.1.zip", DisplayName = "Wind Chimes", Description = "Decorative wind chimes with ambient sounds", Category = ModCategory.Building },
                new ConglomerateModEntry { FileName = "chiseltools1.15.1.zip", DisplayName = "Chisel Tools", Description = "Enhanced chiseling and micro-block tools", Category = ModCategory.Building },
                new ConglomerateModEntry { FileName = "unchisel_1.1.2.zip", DisplayName = "Unchisel", Description = "Undo chiseling to restore original blocks", Category = ModCategory.Building },
                new ConglomerateModEntry { FileName = "vsinstrumentsbase_2.0.4.zip", DisplayName = "VS Instruments", Description = "Playable musical instruments", Category = ModCategory.Building },

                // Crafting & Smithing
                new ConglomerateModEntry { FileName = "NailMold-1.0.2.zip", DisplayName = "Nail Mold", Description = "Casting mold for metal nails", Category = ModCategory.Crafting },
                new ConglomerateModEntry { FileName = "vanillamoremolds_1.1.2.zip", DisplayName = "Vanilla More Molds", Description = "Additional casting molds for vanilla items", Category = ModCategory.Crafting },
                new ConglomerateModEntry { FileName = "smithingplus_1.8.0-rc.3.zip", DisplayName = "Smithing Plus", Description = "Enhanced smithing recipes and tools", Category = ModCategory.Crafting },
                new ConglomerateModEntry { FileName = "Re-SmeltablesLightFix-v0.1.2.zip", DisplayName = "Re-Smeltables Light Fix", Description = "Fix for re-smelting items and lighting", Category = ModCategory.Crafting },
                new ConglomerateModEntry { FileName = "lootablebloomery_1.0.0.zip", DisplayName = "Lootable Bloomery", Description = "Access bloomery contents directly", Category = ModCategory.Crafting },
                new ConglomerateModEntry { FileName = "StoneQuarry_VS1.21.1_net8_v3.5.1.zip", DisplayName = "Stone Quarry", Description = "Quarrying system for bulk stone extraction", Category = ModCategory.Crafting },
                new ConglomerateModEntry { FileName = "SticksFromFirewood-1.1.0.zip", DisplayName = "Sticks From Firewood", Description = "Break firewood into sticks", Category = ModCategory.Crafting },
                new ConglomerateModEntry { FileName = "millwright_1.2.7.zip", DisplayName = "Millwright", Description = "Wind and water mill processing", Category = ModCategory.Crafting },

                // Combat & Skills
                new ConglomerateModEntry { FileName = "maltiezfirearms_1.3.5.zip", DisplayName = "Maltie's Firearms", Description = "Craftable firearms and ammunition", Category = ModCategory.Combat },
                new ConglomerateModEntry { FileName = "krpgenchantment_1.2.12.zip", DisplayName = "KRPG Enchantment", Description = "Item enchanting and magical effects", Category = ModCategory.Combat },
                new ConglomerateModEntry { FileName = "xskills_v0.8.23.zip", DisplayName = "XSkills", Description = "Extended skill tree and progression", Category = ModCategory.Combat },
                new ConglomerateModEntry { FileName = "itemrarity_1.1.4.zip", DisplayName = "Item Rarity", Description = "Color-coded rarity tiers for items", Category = ModCategory.Combat },
                new ConglomerateModEntry { FileName = "slowtox_3.0.0.zip", DisplayName = "Slow Tox", Description = "Poison and toxin combat mechanics", Category = ModCategory.Combat },

                // World & Exploration
                new ConglomerateModEntry { FileName = "BetterRuinsv0.5.0.zip", DisplayName = "Better Ruins", Description = "Improved world ruin generation", Category = ModCategory.WorldExploration },
                new ConglomerateModEntry { FileName = "BetterTradersv0.1.0.zip", DisplayName = "Better Traders", Description = "Enhanced trader inventories and behaviors", Category = ModCategory.WorldExploration },
                new ConglomerateModEntry { FileName = "th3dungeon_0.4.4.zip", DisplayName = "The Dungeon", Description = "Underground dungeon generation", Category = ModCategory.WorldExploration },
                new ConglomerateModEntry { FileName = "ProspectTogether-2.1.1.zip", DisplayName = "Prospect Together", Description = "Share prospecting data with other players", Category = ModCategory.WorldExploration },
                new ConglomerateModEntry { FileName = "Auto Map Markers 4.0.3 - Vintagestory 1.21-rc.zip", DisplayName = "Auto Map Markers", Description = "Automatically place map markers at points of interest", Category = ModCategory.WorldExploration },
                new ConglomerateModEntry { FileName = "primitivesurvival_3.9.8.zip", DisplayName = "Primitive Survival", Description = "Early-game survival tools and mechanics", Category = ModCategory.WorldExploration },
                new ConglomerateModEntry { FileName = "wildfarmingrevival_1.4.0.zip", DisplayName = "Wild Farming Revival", Description = "Wild plant and crop farming system", Category = ModCategory.WorldExploration },

                // Quality of Life
                new ConglomerateModEntry { FileName = "CarryOn-1.21.0_v1.10.0.zip", DisplayName = "Carry On", Description = "Pick up and carry blocks and chests", Category = ModCategory.QualityOfLife },
                new ConglomerateModEntry { FileName = "PlayerCorpse_VS1.20.7_net7_v1.11.1.zip", DisplayName = "Player Corpse", Description = "Drop a lootable corpse on death", Category = ModCategory.QualityOfLife },
                new ConglomerateModEntry { FileName = "jaunt_2.1.0-rc.1.zip", DisplayName = "Jaunt", Description = "Quick travel and teleportation system", Category = ModCategory.QualityOfLife },
                new ConglomerateModEntry { FileName = "blockoverlay-4.5.4.zip", DisplayName = "Block Overlay", Description = "Show block information on hover", Category = ModCategory.QualityOfLife },
                new ConglomerateModEntry { FileName = "HudClockPatch_v1.1.1.zip", DisplayName = "HUD Clock Patch", Description = "In-game clock display on HUD", Category = ModCategory.QualityOfLife },
                new ConglomerateModEntry { FileName = "zoombuttonreborn_2.0.0.zip", DisplayName = "Zoom Button Reborn", Description = "Configurable zoom functionality", Category = ModCategory.QualityOfLife },
                new ConglomerateModEntry { FileName = "vstweaks_0.6.1.zip", DisplayName = "VS Tweaks", Description = "Miscellaneous game tweaks and fixes", Category = ModCategory.QualityOfLife },
                new ConglomerateModEntry { FileName = "canoemod 1.21 Pitchless patch.zip", DisplayName = "Canoe Mod Patch", Description = "Canoe mod compatibility patch", Category = ModCategory.QualityOfLife },
                new ConglomerateModEntry { FileName = "ariasserverutils_1.2.0.zip", DisplayName = "Aria's Server Utils", Description = "Server administration utilities", Category = ModCategory.QualityOfLife },
                new ConglomerateModEntry { FileName = "stepfixelk.zip", DisplayName = "Step Fix Elk", Description = "Fix elk step-up height behavior", Category = ModCategory.QualityOfLife },
                new ConglomerateModEntry { FileName = "nomonsters_103.zip", DisplayName = "No Monsters", Description = "Disable hostile monster spawning", Category = ModCategory.QualityOfLife },
                new ConglomerateModEntry { FileName = "reallynomonsters1.21.0_1.2.2.zip", DisplayName = "Really No Monsters", Description = "Aggressively remove all hostile mobs", Category = ModCategory.QualityOfLife },
                new ConglomerateModEntry { FileName = "medievalexpansionpatch-1.3.4.zip", DisplayName = "Medieval Expansion Patch", Description = "Medieval expansion compatibility patch", Category = ModCategory.QualityOfLife },

                // Libraries
                new ConglomerateModEntry { FileName = "AttributeRenderingLibrary-v2.3.0.zip", DisplayName = "Attribute Rendering Library", Description = "Shared attribute rendering API", Category = ModCategory.Libraries },
                new ConglomerateModEntry { FileName = "CommonLib_VS1.21.1_net8_v2.8.0.zip", DisplayName = "Common Lib", Description = "Shared utility library for mods", Category = ModCategory.Libraries },
                new ConglomerateModEntry { FileName = "configlib_1.10.3.zip", DisplayName = "Config Lib", Description = "Configuration framework for mods", Category = ModCategory.Libraries },
                new ConglomerateModEntry { FileName = "overhaullib_1.12.13.zip", DisplayName = "Overhaul Lib", Description = "Game overhaul framework library", Category = ModCategory.Libraries },
                new ConglomerateModEntry { FileName = "xlib_v0.8.20.zip", DisplayName = "XLib", Description = "Core library for XSkills ecosystem", Category = ModCategory.Libraries },
                new ConglomerateModEntry { FileName = "vsimgui_1.1.13.zip", DisplayName = "VS ImGui", Description = "ImGui implementation for Vintage Story", Category = ModCategory.Libraries },
                new ConglomerateModEntry { FileName = "particlesplus-2.3.1.zip", DisplayName = "Particles Plus", Description = "Enhanced particle effects framework", Category = ModCategory.Libraries },
            };
        }

        public static List<ConglomerateModEntry> GetModsByCategory(ModCategory category)
        {
            return GetAllMods().Where(m => m.Category == category).ToList();
        }

        public static ModCategory[] GetAllCategories()
        {
            return new ModCategory[]
            {
                ModCategory.Storage,
                ModCategory.Cooking,
                ModCategory.Hunting,
                ModCategory.Building,
                ModCategory.Crafting,
                ModCategory.Combat,
                ModCategory.WorldExploration,
                ModCategory.QualityOfLife,
                ModCategory.Libraries
            };
        }
    }
}
