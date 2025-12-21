using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Config;

namespace VintageEssentials
{
    public class ModConfigDialog : GuiDialogGeneric
    {
        private new ICoreClientAPI capi;
        private ModConfig config;
        private Action onKeybindsChanged;
        private Dictionary<string, bool> capturingKeys = new Dictionary<string, bool>();
        private Dictionary<string, KeybindConfig> tempKeybinds = new Dictionary<string, KeybindConfig>();

        public ModConfigDialog(ICoreClientAPI capi, ModConfig config, Action onKeybindsChanged) : base("Mod Settings", capi)
        {
            this.capi = capi;
            this.config = config;
            this.onKeybindsChanged = onKeybindsChanged;
        }

        private void ComposeDialog()
        {
            double elemWidth = 500;
            double currentY = 30;
            
            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);
            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            
            ElementBounds maxSlotsLabelBounds = ElementBounds.Fixed(0, currentY, elemWidth / 2, 30);
            ElementBounds maxSlotsInputBounds = ElementBounds.Fixed(elemWidth / 2 + 10, currentY, elemWidth / 2 - 10, 30);
            currentY += 40;
            
            // Stack size multiplier slider
            ElementBounds stackSizeLabelBounds = ElementBounds.Fixed(0, currentY, elemWidth, 30);
            currentY += 30;
            ElementBounds stackSizeSliderBounds = ElementBounds.Fixed(0, currentY, elemWidth - 80, 30);
            ElementBounds stackSizeValueBounds = ElementBounds.Fixed(elemWidth - 75, currentY, 75, 30);
            currentY += 40;
            
            ElementBounds stackSizeInfoBounds = ElementBounds.Fixed(0, currentY, elemWidth, 40);
            currentY += 45;
            
            ElementBounds keybindsTitleBounds = ElementBounds.Fixed(0, currentY, elemWidth, 30);
            currentY += 35;
            
            // Create bounds for each keybind
            var keybindBounds = new Dictionary<string, (ElementBounds label, ElementBounds current, ElementBounds button)>();
            var keybindNames = new Dictionary<string, string>
            {
                { "chestradius", "Chest Radius Inventory" },
                { "playerinvsort", "Sort Player Inventory" },
                { "toggleslotlock", "Toggle Slot Locking" },
                { "veconfig", "Open Settings" }
            };
            
            foreach (var kvp in keybindNames)
            {
                ElementBounds labelBounds = ElementBounds.Fixed(0, currentY, elemWidth * 0.4, 30);
                ElementBounds currentBounds = ElementBounds.Fixed(elemWidth * 0.4 + 5, currentY, elemWidth * 0.3, 30);
                ElementBounds buttonBounds = ElementBounds.Fixed(elemWidth * 0.7 + 10, currentY, elemWidth * 0.3 - 10, 30);
                keybindBounds[kvp.Key] = (labelBounds, currentBounds, buttonBounds);
                currentY += 35;
            }
            
            ElementBounds saveButtonBounds = ElementBounds.Fixed(0, currentY + 10, 230, 30);
            ElementBounds cancelButtonBounds = ElementBounds.Fixed(240, currentY + 10, 230, 30);
            
            var composer = capi.Gui.CreateCompo("modconfig", dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar("VintageEssentials Settings", OnTitleBarClose)
                .BeginChildElements(bgBounds)
                    .AddStaticText("Max Locked Slots:", CairoFont.WhiteDetailText(), maxSlotsLabelBounds)
                    .AddNumberInput(maxSlotsInputBounds, OnMaxSlotsChanged, CairoFont.WhiteDetailText(), "maxslotsInput")
                    .AddStaticText("Stack Size Multiplier:", CairoFont.WhiteDetailText(), stackSizeLabelBounds)
                    .AddSlider(OnStackSizeChanged, stackSizeSliderBounds, "stackSizeSlider")
                    .AddStaticText($"{config.StackSizeMultiplier}x", CairoFont.WhiteSmallText(), stackSizeValueBounds, "stackSizeValue")
                    .AddStaticText("Applies to items that initially stack ≤10\n(Requires game restart to take effect)", CairoFont.WhiteSmallText(), stackSizeInfoBounds)
                    .AddStaticText("Keybinds:", CairoFont.WhiteDetailText(), keybindsTitleBounds);
            
            // Add keybind controls
            foreach (var kvp in keybindNames)
            {
                string code = kvp.Key;
                string name = kvp.Value;
                var bounds = keybindBounds[code];
                
                // Initialize temp keybind if not exists
                if (!tempKeybinds.ContainsKey(code))
                {
                    tempKeybinds[code] = config.CustomKeybinds.ContainsKey(code) 
                        ? new KeybindConfig 
                        {
                            Key = config.CustomKeybinds[code].Key,
                            Ctrl = config.CustomKeybinds[code].Ctrl,
                            Shift = config.CustomKeybinds[code].Shift,
                            Alt = config.CustomKeybinds[code].Alt
                        }
                        : new KeybindConfig();
                }
                
                string currentKey = GetKeybindDisplay(tempKeybinds[code]);
                bool isCapturing = capturingKeys.ContainsKey(code) && capturingKeys[code];
                string buttonText = isCapturing ? "Press key..." : "Change";
                
                composer
                    .AddStaticText(name + ":", CairoFont.WhiteSmallText(), bounds.label)
                    .AddStaticText(currentKey, CairoFont.WhiteSmallText(), bounds.current, $"keybind_{code}_text")
                    .AddSmallButton(buttonText, () => OnChangeKeybind(code), bounds.button, EnumButtonStyle.Normal, $"keybind_{code}_btn");
            }
            
            composer
                .AddSmallButton("Save", OnSaveClicked, saveButtonBounds)
                .AddSmallButton("Cancel", OnCancelClicked, cancelButtonBounds)
                .EndChildElements();
            
            // Calculate children for auto-sizing
            var allBounds = new List<ElementBounds> { maxSlotsLabelBounds, maxSlotsInputBounds, 
                stackSizeLabelBounds, stackSizeSliderBounds, stackSizeValueBounds, stackSizeInfoBounds, keybindsTitleBounds };
            foreach (var bounds in keybindBounds.Values)
            {
                allBounds.Add(bounds.label);
                allBounds.Add(bounds.current);
                allBounds.Add(bounds.button);
            }
            allBounds.Add(saveButtonBounds);
            allBounds.Add(cancelButtonBounds);
            
            bgBounds.BothSizing = ElementSizing.FitToChildren;
            bgBounds.WithChildren(allBounds.ToArray());
            
            SingleComposer = composer.Compose();

            // Set initial values
            SingleComposer.GetNumberInput("maxslotsInput").SetValue(config.MaxLockedSlots);
            
            // Set slider value (range 1-200, representing 1x to 200x multiplier)
            var slider = SingleComposer.GetSlider("stackSizeSlider");
            slider.SetValues(config.StackSizeMultiplier, 1, 200, 1);
        }
        
        private bool OnStackSizeChanged(int newValue)
        {
            config.StackSizeMultiplier = newValue;
            
            // Update the display text
            var textElem = SingleComposer.GetStaticText("stackSizeValue");
            if (textElem != null)
            {
                textElem.SetNewText($"{newValue}x");
            }
            
            return true;
        }
        
        private string GetKeybindDisplay(KeybindConfig kb)
        {
            if (string.IsNullOrEmpty(kb.Key))
            {
                return "Not Set";
            }
            
            string result = "";
            if (kb.Ctrl) result += "Ctrl+";
            if (kb.Shift) result += "Shift+";
            if (kb.Alt) result += "Alt+";
            result += kb.Key;
            return result;
        }

        private bool OnChangeKeybind(string hotkeyCode)
        {
            // Check if we're already capturing a key
            bool alreadyCapturing = capturingKeys.Values.Contains(true);
            if (alreadyCapturing)
            {
                return true; // Ignore if already capturing
            }
            
            // Start capturing for this keybind
            capturingKeys[hotkeyCode] = true;
            ComposeDialog();
            capi.ShowChatMessage($"Press any key combination for {hotkeyCode} (press Escape to cancel)");
            return true;
        }
        
        public override void OnKeyDown(KeyEvent e)
        {
            // Check if we're capturing a key
            string capturingCode = null;
            foreach (var kvp in capturingKeys)
            {
                if (kvp.Value)
                {
                    capturingCode = kvp.Key;
                    break;
                }
            }
            
            if (capturingCode != null)
            {
                // Don't capture modifier keys alone
                if (e.KeyCode == (int)GlKeys.ControlLeft || e.KeyCode == (int)GlKeys.ControlRight ||
                    e.KeyCode == (int)GlKeys.ShiftLeft || e.KeyCode == (int)GlKeys.ShiftRight ||
                    e.KeyCode == (int)GlKeys.AltLeft || e.KeyCode == (int)GlKeys.AltRight)
                {
                    return;
                }
                
                if (e.KeyCode == (int)GlKeys.Escape)
                {
                    // Cancel capture
                    capturingKeys[capturingCode] = false;
                    ComposeDialog();
                    capi.ShowChatMessage("Keybind change cancelled");
                    return;
                }
                
                // Capture the key
                string keyName = System.Enum.GetName(typeof(GlKeys), e.KeyCode);
                if (!string.IsNullOrEmpty(keyName))
                {
                    tempKeybinds[capturingCode] = new KeybindConfig
                    {
                        Key = keyName,
                        Ctrl = capi.Input.KeyboardKeyState[(int)GlKeys.ControlLeft] || capi.Input.KeyboardKeyState[(int)GlKeys.ControlRight],
                        Shift = capi.Input.KeyboardKeyState[(int)GlKeys.ShiftLeft] || capi.Input.KeyboardKeyState[(int)GlKeys.ShiftRight],
                        Alt = capi.Input.KeyboardKeyState[(int)GlKeys.AltLeft] || capi.Input.KeyboardKeyState[(int)GlKeys.AltRight]
                    };
                    
                    capturingKeys[capturingCode] = false;
                    ComposeDialog();
                    
                    string keyDisplay = GetKeybindDisplay(tempKeybinds[capturingCode]);
                    capi.ShowChatMessage($"Keybind set to: {keyDisplay}");
                }
                return;
            }
            
            base.OnKeyDown(e);
        }

        private void OnMaxSlotsChanged(string value)
        {
            if (int.TryParse(value, out int slots))
            {
                config.MaxLockedSlots = Math.Max(1, Math.Min(20, slots)); // Clamp between 1 and 20
            }
        }

        private bool OnSaveClicked()
        {
            // Apply temp keybinds to config
            foreach (var kvp in tempKeybinds)
            {
                config.CustomKeybinds[kvp.Key] = kvp.Value;
            }
            
            config.Save(capi);
            capi.ShowChatMessage("Settings saved! Stack size changes require a game restart to take effect.");
            
            // Notify keybinds changed
            onKeybindsChanged?.Invoke();
            
            TryClose();
            return true;
        }

        private bool OnCancelClicked()
        {
            // Clear temp keybinds and reload config
            tempKeybinds.Clear();
            capturingKeys.Clear();
            config = ModConfig.Load(capi);
            TryClose();
            return true;
        }

        private void OnTitleBarClose()
        {
            OnCancelClicked();
        }

        public override bool TryOpen()
        {
            // Reset temp state
            tempKeybinds.Clear();
            capturingKeys.Clear();
            ComposeDialog();
            return base.TryOpen();
        }

        public override string ToggleKeyCombinationCode => "modconfig";
    }
}
