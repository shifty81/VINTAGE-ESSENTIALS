using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace VintageEssentials
{
    public class VintageEssentialsClientSystem : ModSystem
    {
        private ICoreClientAPI clientApi;
        private ChestRadiusInventoryDialog chestRadiusDialog;
        private PlayerInventorySortDialog playerSortDialog;
        private InventoryLockDialog inventoryLockDialog;
        private LockedSlotsManager lockedSlotsManager;
        private ModConfig config;
        private KeybindConflictDialog keybindConflictDialog;
        private ModConfigDialog configDialog;
        private LockedSlotsHudOverlay lockedSlotsHudOverlay;
        private InventorySlotClickHandler slotClickHandler;

        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);
            this.clientApi = api;

            // Load configuration
            config = ModConfig.Load(api);

            // Create locked slots manager (client-side for UI, will sync with server)
            lockedSlotsManager = new LockedSlotsManager(api, config);

            // Create dialogs
            chestRadiusDialog = new ChestRadiusInventoryDialog(api);
            playerSortDialog = new PlayerInventorySortDialog(api, lockedSlotsManager);
            inventoryLockDialog = new InventoryLockDialog(api, lockedSlotsManager, config, OnLockingModeChanged);
            keybindConflictDialog = new KeybindConflictDialog(api, config, OnKeybindsChanged);
            configDialog = new ModConfigDialog(api, config, OnKeybindsChanged);

            // Create HUD overlay for locked slots
            lockedSlotsHudOverlay = new LockedSlotsHudOverlay(api, lockedSlotsManager);
            api.Gui.RegisterDialog(lockedSlotsHudOverlay);

            // Create and initialize slot click handler
            slotClickHandler = new InventorySlotClickHandler(api, inventoryLockDialog, lockedSlotsManager);
            slotClickHandler.Initialize();

            // Register keybinds using config
            RegisterKeybinds();

            // Register client chat command for config
            clientApi.RegisterCommand("veconfig", "Opens VintageEssentials configuration", "", (id, args) =>
            {
                configDialog.TryOpen();
            });

            // Check for keybind conflicts on player join
            clientApi.Event.PlayerEntitySpawn += OnPlayerSpawn;

            clientApi.Logger.Notification("VintageEssentials client-side loaded.");
            clientApi.ShowChatMessage("VintageEssentials loaded! Press Ctrl+Shift+V to open settings or use /veconfig");
        }
        
        private void RegisterKeybinds()
        {
            // Chest radius inventory
            var chestRadiusKey = config.CustomKeybinds.ContainsKey("chestradius") 
                ? config.CustomKeybinds["chestradius"] 
                : new KeybindConfig { Key = "R" };
            GlKeys chestRadiusGlKey = ParseGlKey(chestRadiusKey.Key);
            clientApi.Input.RegisterHotKey("chestradius", "Open Chest Radius Inventory", chestRadiusGlKey, 
                HotkeyType.GUIOrOtherControls, 
                shiftPressed: chestRadiusKey.Shift, 
                ctrlPressed: chestRadiusKey.Ctrl, 
                altPressed: chestRadiusKey.Alt);
            clientApi.Input.SetHotKeyHandler("chestradius", OnChestRadiusHotkey);

            // Player inventory sort
            var playerSortKey = config.CustomKeybinds.ContainsKey("playerinvsort") 
                ? config.CustomKeybinds["playerinvsort"] 
                : new KeybindConfig { Key = "S", Shift = true };
            GlKeys playerSortGlKey = ParseGlKey(playerSortKey.Key);
            clientApi.Input.RegisterHotKey("playerinvsort", "Sort Player Inventory", playerSortGlKey, 
                HotkeyType.InventoryHotkeys, 
                shiftPressed: playerSortKey.Shift, 
                ctrlPressed: playerSortKey.Ctrl, 
                altPressed: playerSortKey.Alt);
            clientApi.Input.SetHotKeyHandler("playerinvsort", OnPlayerSortHotkey);

            // Toggle slot locking
            var toggleLockKey = config.CustomKeybinds.ContainsKey("toggleslotlock") 
                ? config.CustomKeybinds["toggleslotlock"] 
                : new KeybindConfig { Key = "L", Ctrl = true };
            GlKeys toggleLockGlKey = ParseGlKey(toggleLockKey.Key);
            clientApi.Input.RegisterHotKey("toggleslotlock", "Toggle Inventory Slot Locking Mode", toggleLockGlKey, 
                HotkeyType.InventoryHotkeys, 
                shiftPressed: toggleLockKey.Shift, 
                ctrlPressed: toggleLockKey.Ctrl, 
                altPressed: toggleLockKey.Alt);
            clientApi.Input.SetHotKeyHandler("toggleslotlock", OnToggleSlotLockHotkey);

            // Config dialog
            var configKey = config.CustomKeybinds.ContainsKey("veconfig") 
                ? config.CustomKeybinds["veconfig"] 
                : new KeybindConfig { Key = "V", Ctrl = true, Shift = true };
            GlKeys configGlKey = ParseGlKey(configKey.Key);
            clientApi.Input.RegisterHotKey("veconfig", "Open VintageEssentials Settings", configGlKey, 
                HotkeyType.GUIOrOtherControls, 
                shiftPressed: configKey.Shift, 
                ctrlPressed: configKey.Ctrl, 
                altPressed: configKey.Alt);
            clientApi.Input.SetHotKeyHandler("veconfig", OnConfigHotkey);
        }
        
        private GlKeys ParseGlKey(string keyName)
        {
            // Try to parse the key name to GlKeys enum
            if (System.Enum.TryParse<GlKeys>(keyName, true, out GlKeys result))
            {
                return result;
            }
            // Default fallback
            return GlKeys.Unknown;
        }
        
        private void OnKeybindsChanged()
        {
            // Reload keybinds when they change
            // Note: We need to unregister and re-register hotkeys
            // Unfortunately, the VS API doesn't provide a direct way to unregister hotkeys
            // So we'll just re-register them which will override the previous handlers
            RegisterKeybinds();
            clientApi.ShowChatMessage("Keybinds updated!");
        }

        private void OnPlayerSpawn(IClientPlayer player)
        {
            // Check for keybind conflicts
            List<ConflictInfo> conflicts = CheckKeybindConflicts();
            if (conflicts.Count > 0)
            {
                // Show conflict resolution dialog
                keybindConflictDialog.ShowConflicts(conflicts);
            }
        }

        private List<ConflictInfo> CheckKeybindConflicts()
        {
            List<ConflictInfo> conflicts = new List<ConflictInfo>();
            
            // Get our registered hotkeys
            var ourHotkeys = new Dictionary<string, KeybindConfig>
            {
                { "chestradius", config.CustomKeybinds.ContainsKey("chestradius") ? config.CustomKeybinds["chestradius"] : new KeybindConfig { Key = "R" } },
                { "playerinvsort", config.CustomKeybinds.ContainsKey("playerinvsort") ? config.CustomKeybinds["playerinvsort"] : new KeybindConfig { Key = "S", Shift = true } },
                { "toggleslotlock", config.CustomKeybinds.ContainsKey("toggleslotlock") ? config.CustomKeybinds["toggleslotlock"] : new KeybindConfig { Key = "L", Ctrl = true } },
                { "veconfig", config.CustomKeybinds.ContainsKey("veconfig") ? config.CustomKeybinds["veconfig"] : new KeybindConfig { Key = "V", Ctrl = true, Shift = true } }
            };

            // Check for conflicts with all registered game hotkeys
            var allHotkeys = clientApi.Input.HotKeys;
            
            foreach (var ourHotkey in ourHotkeys)
            {
                string ourCode = ourHotkey.Key;
                KeybindConfig ourKey = ourHotkey.Value;
                
                // Skip if not registered yet (first time)
                if (!allHotkeys.ContainsKey(ourCode))
                {
                    continue;
                }
                
                // Check against all other hotkeys
                foreach (var gameHotkey in allHotkeys)
                {
                    // Skip if it's our own hotkey
                    if (gameHotkey.Key == ourCode)
                    {
                        continue;
                    }
                    
                    var gameKey = gameHotkey.Value;
                    
                    // Compare keybinds
                    if (gameKey.CurrentMapping.KeyCode == ParseGlKey(ourKey.Key) &&
                        gameKey.CurrentMapping.Shift == ourKey.Shift &&
                        gameKey.CurrentMapping.Ctrl == ourKey.Ctrl &&
                        gameKey.CurrentMapping.Alt == ourKey.Alt)
                    {
                        conflicts.Add(new ConflictInfo
                        {
                            HotkeyCode = ourCode,
                            ConflictingWith = $"{gameHotkey.Key} - {gameKey.Name}",
                            Description = $"Conflicts with {gameKey.Name}"
                        });
                        break; // Only report first conflict per hotkey
                    }
                }
            }

            return conflicts;
        }

        private void OnLockingModeChanged(bool lockingMode)
        {
            // This callback can be used to update UI or other systems when locking mode changes
            if (lockingMode)
            {
                clientApi.ShowChatMessage("Locking mode ON - Click inventory slots to lock/unlock them");
            }
            else
            {
                clientApi.ShowChatMessage("Locking mode OFF");
            }
        }

        private bool OnChestRadiusHotkey(KeyCombination keyCombination)
        {
            if (chestRadiusDialog.IsOpened())
            {
                chestRadiusDialog.TryClose();
            }
            else
            {
                chestRadiusDialog.TryOpen();
            }
            return true;
        }

        private bool OnPlayerSortHotkey(KeyCombination keyCombination)
        {
            playerSortDialog.SortPlayerInventory();
            return true;
        }

        private bool OnToggleSlotLockHotkey(KeyCombination keyCombination)
        {
            inventoryLockDialog.ToggleLockingMode();
            return true;
        }

        private bool OnConfigHotkey(KeyCombination keyCombination)
        {
            configDialog.TryOpen();
            return true;
        }

        public override void Dispose()
        {
            chestRadiusDialog?.Dispose();
            playerSortDialog?.Dispose();
            inventoryLockDialog?.Dispose();
            keybindConflictDialog?.Dispose();
            configDialog?.Dispose();
            lockedSlotsHudOverlay?.Dispose();
            slotClickHandler?.Dispose();
            base.Dispose();
        }
    }
}
