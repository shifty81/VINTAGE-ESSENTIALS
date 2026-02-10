using System;
using System.Collections.Generic;
using Vintagestory.API.Common;

namespace VintageEssentials
{
    public class KeybindConfig
    {
        public string Key { get; set; } = "";
        public bool Ctrl { get; set; } = false;
        public bool Shift { get; set; } = false;
        public bool Alt { get; set; } = false;
    }
    
    public class ModConfig
    {
        public int MaxLockedSlots { get; set; } = 10;
        public int StackSizeMultiplier { get; set; } = 100; // Default 100x multiplier (1000 max stack for 10x items)
        public Dictionary<string, KeybindConfig> CustomKeybinds { get; set; } = new Dictionary<string, KeybindConfig>();
        
        public static ModConfig Load(ICoreAPI api)
        {
            try
            {
                var config = api.LoadModConfig<ModConfig>("vintageessentials.json");
                if (config == null)
                {
                    config = new ModConfig();
                    config.InitializeDefaultKeybinds();
                    api.StoreModConfig(config, "vintageessentials.json");
                }
                else
                {
                    // Ensure all keybinds exist (for updates)
                    config.EnsureKeybindsExist();
                }
                return config;
            }
            catch (Exception e)
            {
                api.Logger.Error("VintageEssentials: Failed to load config, using defaults: " + e.Message);
                var config = new ModConfig();
                config.InitializeDefaultKeybinds();
                return config;
            }
        }
        
        public void InitializeDefaultKeybinds()
        {
            if (CustomKeybinds == null)
            {
                CustomKeybinds = new Dictionary<string, KeybindConfig>();
            }
            
            // Initialize default keybinds only if not already set
            if (!CustomKeybinds.ContainsKey("chestradius"))
            {
                CustomKeybinds["chestradius"] = new KeybindConfig { Key = "R" };
            }
            if (!CustomKeybinds.ContainsKey("playerinvsort"))
            {
                CustomKeybinds["playerinvsort"] = new KeybindConfig { Key = "S", Shift = true };
            }
            if (!CustomKeybinds.ContainsKey("toggleslotlock"))
            {
                CustomKeybinds["toggleslotlock"] = new KeybindConfig { Key = "L", Ctrl = true };
            }
            if (!CustomKeybinds.ContainsKey("veconfig"))
            {
                CustomKeybinds["veconfig"] = new KeybindConfig { Key = "V", Ctrl = true, Shift = true };
            }
        }
        
        public void EnsureKeybindsExist()
        {
            if (CustomKeybinds == null)
            {
                InitializeDefaultKeybinds();
                return;
            }
            
            // Add any missing keybinds with defaults
            if (!CustomKeybinds.ContainsKey("chestradius"))
            {
                CustomKeybinds["chestradius"] = new KeybindConfig { Key = "R" };
            }
            if (!CustomKeybinds.ContainsKey("playerinvsort"))
            {
                CustomKeybinds["playerinvsort"] = new KeybindConfig { Key = "S", Shift = true };
            }
            if (!CustomKeybinds.ContainsKey("toggleslotlock"))
            {
                CustomKeybinds["toggleslotlock"] = new KeybindConfig { Key = "L", Ctrl = true };
            }
            if (!CustomKeybinds.ContainsKey("veconfig"))
            {
                CustomKeybinds["veconfig"] = new KeybindConfig { Key = "V", Ctrl = true, Shift = true };
            }
        }
        
        public void Save(ICoreAPI api)
        {
            try
            {
                api.StoreModConfig(this, "vintageessentials.json");
            }
            catch (Exception e)
            {
                api.Logger.Error("VintageEssentials: Failed to save config: " + e.Message);
            }
        }
        
        public string GetKeybindDisplayString(string hotkeyCode)
        {
            if (!CustomKeybinds.ContainsKey(hotkeyCode))
            {
                return "Not Set";
            }
            
            var kb = CustomKeybinds[hotkeyCode];
            string result = "";
            
            if (kb.Ctrl) result += "Ctrl+";
            if (kb.Shift) result += "Shift+";
            if (kb.Alt) result += "Alt+";
            result += kb.Key;
            
            return result;
        }
    }
}
